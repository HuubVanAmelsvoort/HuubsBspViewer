using SharpGLTF.Schema2;
using System.Numerics;
using TK = OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4; // Zorgt dat GL. functies werken
using System.Collections.Generic;

public class GltfMeshOnderdeel {
  public int Vao;
  public int VertexCount;
  public Node TargetNode;
}

public class TekenGltfModel {
  private ModelRoot _model;
  private List<GltfMeshOnderdeel> _onderdelen = new List<GltfMeshOnderdeel>();

  public TekenGltfModel(string pad) {
    _model = ModelRoot.Load(pad);
    PrepareerMeshes();
  }

  private void PrepareerMeshes() {
    foreach(var node in _model.LogicalNodes) {
      if(node.Mesh == null) continue;

      foreach(var primitive in node.Mesh.Primitives) {
        var posAccessor = primitive.GetVertexAccessor("POSITION").AsVector3Array();
        var uvAccessor = primitive.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
        var indices = primitive.GetIndexAccessor().AsIndicesArray();

        List<float> vertexData = new List<float>();
        foreach(var idx in indices) {
          // We pakken hier de RAW posities (X, Y, Z) uit de glTF
          var p = posAccessor[(int)idx];
          var u = uvAccessor[(int)idx];

          vertexData.Add(p.X);
          vertexData.Add(p.Y);
          vertexData.Add(p.Z);
          vertexData.Add(u.X);
          vertexData.Add(1.0f - u.Y); // Flip UV voor OpenGL
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

        _onderdelen.Add(new GltfMeshOnderdeel {
          Vao = vao,
          VertexCount = vertexData.Count / 5, // We delen door 5 omdat we X,Y,Z,U,V hebben
          TargetNode = node
        });
      }
    }
  }

  public void Teken(AlgemeneShader shader, TK.Vector3 wereldPositie, float schaal) {
    shader.Gebruiken();

    foreach(var onderdeel in _onderdelen) {
      // Bereken hiërarchie (voor de neus, armen etc.)
      Matrix4x4 resultMatrix = GetWorldMatrix(onderdeel.TargetNode);
      TK.Matrix4 localMatrix = ConverteerMatrix(resultMatrix);

      // De Quake-as swap passen we nu pas toe op de uiteindelijke matrix
      // We zetten de glTF (Y-up) om naar Quake (Z-up)
      TK.Matrix4 quakeFix = TK.Matrix4.CreateRotationX(TK.MathHelper.DegreesToRadians(-90));

      TK.Matrix4 worldMatrix = localMatrix * TK.Matrix4.CreateScale(schaal) * quakeFix * TK.Matrix4.CreateTranslation(wereldPositie);

      shader.SetMatrix4("model", worldMatrix);

      GL.BindVertexArray(onderdeel.Vao);
      GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, onderdeel.VertexCount);
    }
  }

  private Matrix4x4 GetWorldMatrix(Node node) {
    var matrix = node.LocalMatrix;
    var parent = node.VisualParent;

    while(parent != null) {
      matrix = matrix * parent.LocalMatrix;
      parent = parent.VisualParent;
    }
    return matrix;
  }

  private TK.Matrix4 ConverteerMatrix(Matrix4x4 m) {
    return new TK.Matrix4(
        m.M11, m.M12, m.M13, m.M14,
        m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34,
        m.M41, m.M42, m.M43, m.M44);
  }
}