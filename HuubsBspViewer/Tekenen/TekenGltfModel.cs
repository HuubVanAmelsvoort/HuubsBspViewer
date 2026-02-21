using OpenTK.Graphics.OpenGL4;
using GLTF = SharpGLTF.Schema2;
using S = System.Numerics;
using TK = OpenTK.Mathematics;

public class GltfMeshOnderdeel {
  public int Vao;
  public int VertexCount;
  public GLTF.Node TargetNode;
  public Texture TargetTexture; // Specifieke texture voor dit onderdeel
}

public class TekenGltfModel {
  private GLTF.ModelRoot _model;
  private List<GltfMeshOnderdeel> _GltfMeshOnderdeelOnderdelen = new List<GltfMeshOnderdeel>();

  public TekenGltfModel(string modelPad) {
    _model = GLTF.ModelRoot.Load(modelPad);
    VoorbereidenGaasNetWerk();
  }

  private void VoorbereidenGaasNetWerk() {
    foreach(var node in _model.LogicalNodes) {
      if(node.Mesh == null) continue;

      foreach(var primitief in node.Mesh.Primitives) {
        var accessoirePositie = primitief.GetVertexAccessor("POSITION").AsVector3Array();
        var accessoireUv = primitief.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
        var indexen = primitief.GetIndexAccessor().AsIndicesArray();

        List<float> vertexData = new List<float>();
        foreach(uint deIndex in indexen) {
          vertexData.Add(accessoirePositie[(int)deIndex].X);
          vertexData.Add(accessoirePositie[(int)deIndex].Y);
          vertexData.Add(accessoirePositie[(int)deIndex].Z);
          vertexData.Add(accessoireUv[(int)deIndex].X);
          //vertexData.Add(1.0f - accessoireUv[(int)deIndex].Y); // UV flip voor OpenGL
          vertexData.Add(accessoireUv[(int)deIndex].Y); 
        }

        int vao = GL.GenVertexArray();
        int vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Count * sizeof(float), vertexData.ToArray(), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        // Texture ophalen uit glTF materiaal
        Texture onderdeelTexture = null;
        var baseColor = primitief.Material?.FindChannel("BaseColor");
        if(baseColor?.Texture != null) {
          onderdeelTexture = new Texture(baseColor.Value.Texture.PrimaryImage.Content.Content.ToArray());
        }

        _GltfMeshOnderdeelOnderdelen.Add(new GltfMeshOnderdeel {
          Vao = vao,
          VertexCount = vertexData.Count / 5,
          TargetNode = node,
          TargetTexture = onderdeelTexture
        });
      }
    }
  }

  public void Teken(AlgemeneShader shader, TK.Vector3 wereldPositie, float schaal) {
    shader.Gebruiken();
    GL.Uniform1(GL.GetUniformLocation(shader.Handle, "isMarker"), 0); // Forceer texture modus

    foreach(var onderdeel in _GltfMeshOnderdeelOnderdelen) {
      TK.Matrix4 localMatrix = ConverteerMatrix(GeefDeWereldMatrix(onderdeel.TargetNode));

      // Quake Fix: +90 graden om X-as om model rechtop te zetten (Y-up naar Z-up)
      TK.Matrix4 quakeFix = TK.Matrix4.CreateRotationX(TK.MathHelper.DegreesToRadians(90));
      TK.Matrix4 wereldMatrix = localMatrix * TK.Matrix4.CreateScale(schaal) * quakeFix * TK.Matrix4.CreateTranslation(wereldPositie);

      shader.SetMatrix4("model", wereldMatrix);

      if(onderdeel.TargetTexture != null) {
        onderdeel.TargetTexture.Gebruik();
        shader.SetInt("uTexture", 0);
      }

      GL.BindVertexArray(onderdeel.Vao);
      GL.DrawArrays(PrimitiveType.Triangles, 0, onderdeel.VertexCount);
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
    return new TK.Matrix4(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
  }
}