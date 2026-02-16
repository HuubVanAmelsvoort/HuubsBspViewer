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
    foreach (var image in _model.LogicalImages) {
      // Print de naam of bron van de afbeelding naar je console
      Console.WriteLine($"Afbeelding gevonden: {image.Name ?? "Naamloos"}");

      // Controleer of er daadwerkelijk data aanwezig is
      var data = image.Content.Content.ToArray();
      if (data.Length > 0) {
        Console.WriteLine($"- Data geladen: {data.Length} bytes");
      } else {
        Console.WriteLine("- WAARSCHUWING: Geen data gevonden voor deze afbeelding!");
      }
    }
  }

  private void VoorbereidenGaasNetWerk() {
    foreach (GLTF.Node node in _model.LogicalNodes) {
      if (node.Mesh == null) continue;

      foreach (GLTF.MeshPrimitive primitief in node.Mesh.Primitives) {
        Vector3Array accessoirePositie = (Vector3Array)primitief.GetVertexAccessor("POSITION").AsVector3Array();
        Vector2Array accessoireUv = (Vector2Array)primitief.GetVertexAccessor("TEXCOORD_0").AsVector2Array();       
        IntegerArray indexen = (IntegerArray)primitief.GetIndexAccessor().AsIndicesArray();

        List<float> vertexData = new List<float>();
        foreach (uint deIndex in indexen) {
          // We pakken hier de RAW posities (X, Y, Z) uit de glTF
          S.Vector3 positie = accessoirePositie[(int)deIndex];
          S.Vector2 plaatje = accessoireUv[(int)deIndex];

          vertexData.Add(positie.X);
          vertexData.Add(1.0f - positie.Y);
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

        _GltfMeshOnderdeelOnderdelen.Add(new GltfMeshOnderdeel {
          Vao = vao,
          VertexCount = vertexData.Count / 5, // We delen door 5 omdat we X,Y,Z,U,V hebben
          TargetNode = node
        });
      }
    }
  }
  public void Teken(AlgemeneShader shader, TK.Vector3 wereldPositie, float schaal) {
    shader.Gebruiken();
    TK.Vector4 kleur;

    int isMarker = GL.GetUniformLocation(shader.Handle, "isMarker");
    int markerKleur = GL.GetUniformLocation(shader.Handle, "markerKleur");

    GL.Uniform1(GL.GetUniformLocation(shader.Handle, "isMarker"), 0);

    GL.Uniform1(isMarker, 0); // zet de marker modus aan in de shader

    TK.Vector3 kleur3 = DiverseHelpFuncties.KleurVanClassNaam("huub");
    kleur = new TK.Vector4(kleur3, 1.0f);

    foreach (GltfMeshOnderdeel gltfMeshOnderdeel in _GltfMeshOnderdeelOnderdelen) {
      // Bereken hiërarchie (voor de neus, armen etc.)
      S.Matrix4x4 resultMatrix = GeefDeWereldMatrix(gltfMeshOnderdeel.TargetNode);
      TK.Matrix4 localMatrix = ConverteerMatrix(resultMatrix);

      // De Quake-as swap passen we nu pas toe op de uiteindelijke matrix
      // We zetten de glTF (Y-up) om naar Quake (Z-up)
      TK.Matrix4 quakeFix = TK.Matrix4.CreateRotationX(TK.MathHelper.DegreesToRadians(-90));

      TK.Matrix4 worldMatrix = localMatrix * TK.Matrix4.CreateScale(schaal) * quakeFix * TK.Matrix4.CreateTranslation(wereldPositie);
      GL.ActiveTexture(TextureUnit.Texture0);
      Texture texture = new Texture(@"C:\werkdirectory\BlockbenchBestanden\SneeuwPopHuub\VolledigeSneeuwpop.png");
      texture.Gebruik();

      //GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
      GL.ActiveTexture(TextureUnit.Texture0);
      GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
      shader.SetInt("uTexture", 0); // Vertel de shader dat hij unit 0 moet gebruiken

      shader.SetMatrix4("model", worldMatrix);

      GL.BindVertexArray(gltfMeshOnderdeel.Vao);
      GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, gltfMeshOnderdeel.VertexCount);
    }

    GL.Uniform1(isMarker, 0); // zet de marker modus uit in de shader

  }

  private S.Matrix4x4 GeefDeWereldMatrix(GLTF.Node node) {
    var matrix = node.LocalMatrix;
    var parent = node.VisualParent;

    while (parent != null) {
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
}