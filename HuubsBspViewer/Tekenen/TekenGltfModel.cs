using OpenTK.Graphics.OpenGL4;
using SharpGLTF.Memory;
using System.Numerics;
using GLTF = SharpGLTF.Schema2;
using S = System.Numerics;
using TK = OpenTK.Mathematics;

public class GltfMeshOnderdeel {
  public int Vao;
  public int VertexCount;
  public GLTF.Node TargetNode;
  public Texture TargetTexture;
}

public class TekenGltfModel {
  private GLTF.ModelRoot _model;
  private List<GltfMeshOnderdeel> _GltfMeshOnderdeelOnderdelen = new List<GltfMeshOnderdeel>();
  private Texture _hoofdTexture; // Voeg dit toe bovenaan de klasse

  public TekenGltfModel(string modelPad) {
    Console.WriteLine($"Het volgende model wordt geladen: {modelPad}");

    _model = GLTF.ModelRoot.Load(modelPad);
    ControleerPlaatjes();
    VoorbereidenGaasNetWerk();
  }
  /**
   * Voorlopige controle, maar kan later weg.
   */
  private void ControleerPlaatjes() {
    foreach(var image in _model.LogicalImages) {
      // Print de naam of bron van de afbeelding naar je console
      Console.WriteLine($"Afbeelding gevonden: {image.Name ?? "Naamloos"}");

      // Controleer of er daadwerkelijk data aanwezig is
      var data = image.Content.Content.ToArray();
      if(data.Length > 0) {
        Console.WriteLine($"- Data geladen: {data.Length} bytes");
      } else {
        Console.WriteLine("- WAARSCHUWING: Geen data gevonden voor deze afbeelding!");
      }
    }
  }

  private S.Matrix4x4 GeefDeWereldMatrix(GLTF.Node node) {
    var matrix = node.LocalMatrix;
    var parent = node.VisualParent;

    while(parent != null) {
      matrix = matrix * parent.LocalMatrix;
      parent = parent.VisualParent;
    }
    return matrix;
  }
  private TK.Matrix4 ConverteerMatrix(S.Matrix4x4 m) {
    return new TK.Matrix4(
        m.M11, m.M12, m.M13, m.M14,
        m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34,
        m.M41, m.M42, m.M43, m.M44);
  }
  public void Teken(AlgemeneShader shader, TK.Vector3 wereldPositie, float schaal) {
    shader.Gebruiken();

    int isMarker = GL.GetUniformLocation(shader.Handle, "isMarker");
    GL.Uniform1(isMarker, 0); // Zorg dat we textures gebruiken, geen marker-kleur

    foreach(GltfMeshOnderdeel gltfMeshOnderdeel in _GltfMeshOnderdeelOnderdelen) {
      S.Matrix4x4 resultMatrix = GeefDeWereldMatrix(gltfMeshOnderdeel.TargetNode);
      TK.Matrix4 localMatrix = ConverteerMatrix(resultMatrix);

      // De "Rechtopzetter": glTF (Y-up) naar Quake (Z-up).
      // Gebruik -90 graden als hij nog op zijn kop staat.
      TK.Matrix4 quakeFix = TK.Matrix4.CreateRotationX(TK.MathHelper.DegreesToRadians(90));

      // Volgorde: Hiërarchie -> Draaien -> Schalen -> Verplaatsen
      TK.Matrix4 worldMatrix = localMatrix * quakeFix * TK.Matrix4.CreateScale(schaal) * TK.Matrix4.CreateTranslation(wereldPositie);
      shader.SetMatrix4("model", worldMatrix);

      // CRUCIAAL: Bind de specifieke texture voor dit onderdeel (mond, ogen, etc.)
      if(gltfMeshOnderdeel.TargetTexture != null) {
        GL.ActiveTexture(TextureUnit.Texture0);
        gltfMeshOnderdeel.TargetTexture.Gebruik();
        shader.SetInt("uTexture", 0);
      }

      GL.BindVertexArray(gltfMeshOnderdeel.Vao);
      GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, gltfMeshOnderdeel.VertexCount);
    }
  }

  private Texture LaadTextureUitGltf(GLTF.Image gltfImage) {
    // Haal de ruwe bytes op uit de glTF data
    byte[] imageData = gltfImage.Content.Content.ToArray();

    // Hier maken we gebruik van je bestaande Texture klasse.
    // Zorg dat je Texture klasse een constructor heeft die een byte[] of Stream accepteert.
    // Als je die niet hebt, kun je de data tijdelijk opslaan of je Texture klasse uitbreiden.
    return new Texture(imageData);
  }

  private void VoorbereidenGaasNetWerk() {
    foreach(GLTF.Node node in _model.LogicalNodes) {
      if(node.Mesh == null) continue;

      foreach(GLTF.MeshPrimitive primitief in node.Mesh.Primitives) {
        Vector3Array accessoirePositie = (Vector3Array)primitief.GetVertexAccessor("POSITION").AsVector3Array();
        Vector2Array accessoireUv = (Vector2Array)primitief.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
        IntegerArray indexen = (IntegerArray)primitief.GetIndexAccessor().AsIndicesArray();

        List<float> vertexData = new List<float>();
        foreach(uint deIndex in indexen) {
          // We pakken hier de RAW posities (X, Y, Z) uit de glTF
          S.Vector3 positie = accessoirePositie[(int)deIndex];
          S.Vector2 plaatje = accessoireUv[(int)deIndex];

          vertexData.Add(positie.X);
          vertexData.Add(positie.Y);
          vertexData.Add(positie.Z);

          vertexData.Add(plaatje.X);
          vertexData.Add(1.0f - plaatje.Y); // Flip UV voor OpenGL
        }

        // GL BUFFER LOGICA
        int stride = 5 * sizeof(float);
        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Count * sizeof(float), vertexData.ToArray(), BufferUsageHint.StaticDraw);

        // Locatie 0: Positie (X, Y, Z)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);

        // Locatie 1: UV (U, V) - begint na 3 floats
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        Texture onderdeelTexture = null;

        // Vraag aan glTF welk plaatje bij dit onderdeel hoort
        var materiaal = primitief.Material;
        var baseColor = materiaal?.FindChannel("BaseColor");

        if(baseColor?.Texture != null) {
          var gltfImage = baseColor.Value.Texture.PrimaryImage;
          // Laad de texture alleen als we hem nog niet hebben (bespaart geheugen)
          onderdeelTexture = LaadTextureUitGltf(gltfImage);
        }

        _GltfMeshOnderdeelOnderdelen.Add(new GltfMeshOnderdeel {
          Vao = vao,
          VertexCount = vertexData.Count / 5, // We delen door 5 omdat we X,Y,Z,U,V hebben
          TargetNode = node,
          TargetTexture = onderdeelTexture // Sla dit op per onderdeel!
        });
      }
    }
  }
}