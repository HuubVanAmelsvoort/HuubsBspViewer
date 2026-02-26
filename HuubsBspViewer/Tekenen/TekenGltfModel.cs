using OpenTK.Graphics.OpenGL4;
using SharpGLTF.Schema2;
using S = System.Numerics;
using TK = OpenTK.Mathematics;

public class TekenGltfModel {
  private ModelRoot _model;
  private List<GltfMeshOnderdeel> _onderdelen = new List<GltfMeshOnderdeel>();
  private float _animatieTijd = 0f;
  public TekenGltfModel(string modelPad) {
    _model = ModelRoot.Load(modelPad);
    VoorbereidenGaasNetWerk();
  }

  private void VoorbereidenGaasNetWerk() {
    foreach(var node in _model.LogicalNodes) {
      if(node.Mesh == null) continue;

      foreach(var primitief in node.Mesh.Primitives) {
        var posities = primitief.GetVertexAccessor("POSITION").AsVector3Array();
        var uvs = primitief.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
        var indexen = primitief.GetIndexAccessor().AsIndicesArray();

        List<float> vertexData = new List<float>();
        foreach(uint i in indexen) {
          vertexData.Add(posities[(int)i].X);
          vertexData.Add(posities[(int)i].Y);
          vertexData.Add(posities[(int)i].Z);
          vertexData.Add(uvs[(int)i].X);
          vertexData.Add(uvs[(int)i].Y);
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

        Plaatje onderdeelTexture = null;
        var baseColor = primitief.Material?.FindChannel("BaseColor");
        if(baseColor?.Texture != null) {
          onderdeelTexture = new Plaatje(baseColor.Value.Texture.PrimaryImage.Content.Content.ToArray());
        }

        _onderdelen.Add(new GltfMeshOnderdeel {
          Vao = vao,
          VertexCount = vertexData.Count / 5,
          TargetNode = node,
          TargetTexture = onderdeelTexture
        });
      }
    }
  }

  public void UpdateAnimatie(float deltaTime) {
    _animatieTijd += deltaTime;
    if(_model.LogicalAnimations.Count == 0) return;

    // Kies de animatie (index 0 is meestal de standaard)
    var animatie = _model.LogicalAnimations[0];
    float tijd = _animatieTijd % animatie.Duration;

    // We groeperen de kanalen per node zodat we T, R en S tegelijk kunnen zetten
    var geanimeerdeNodes = animatie.Channels.Select(c => c.TargetNode).Distinct();

    foreach(var node in geanimeerdeNodes) {
      var t = node.LocalTransform.Translation;
      var r = node.LocalTransform.Rotation;
      var s = node.LocalTransform.Scale;

      // Zoek alle kanalen voor DEZE specifieke node
      var kanalenVoorNode = animatie.Channels.Where(c => c.TargetNode == node);

      foreach(var channel in kanalenVoorNode) {
        if(channel.TargetNodePath == PropertyPath.translation)
          t = channel.GetTranslationSampler().CreateCurveSampler().GetPoint(tijd);
        if(channel.TargetNodePath == PropertyPath.rotation)
          r = channel.GetRotationSampler().CreateCurveSampler().GetPoint(tijd);
        if(channel.TargetNodePath == PropertyPath.scale)
          s = channel.GetScaleSampler().CreateCurveSampler().GetPoint(tijd);
      }

      // Pas alles in één keer toe om read-only fouten te voorkomen
      node.LocalTransform = new SharpGLTF.Transforms.AffineTransform(s, r, t);
    }
  }
  public void Teken(AlgemeneShader shader, TK.Vector3 wereldPositie, float schaal) {
    shader.Gebruiken();
    GL.Uniform1(GL.GetUniformLocation(shader.Handle, "isMarker"), 0);

    foreach(var onderdeel in _onderdelen) {
      S.Matrix4x4 resultMatrix = DiverseHelpFuncties.GeefDeWereldMatrix(onderdeel.TargetNode);
      TK.Matrix4 gltfMatrix = DiverseHelpFuncties.ConverteerMatrix(resultMatrix);

      TK.Matrix4 quakeFix = TK.Matrix4.CreateRotationX(TK.MathHelper.DegreesToRadians(90));

       TK.Matrix4 worldMatrix = gltfMatrix
        * TK.Matrix4.CreateScale(schaal)
        * quakeFix
        * TK.Matrix4.CreateTranslation(wereldPositie)
        ;
      shader.SetMatrix4("model", worldMatrix);

      if(onderdeel.TargetTexture != null) {
        onderdeel.TargetTexture.BindHetPlaatje();
        shader.SetInt("uTexture", 0);
      }

      GL.BindVertexArray(onderdeel.Vao);
      // FIX: Expliciet OpenTK.Graphics.OpenGL4.PrimitiveType gebruiken
      GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, onderdeel.VertexCount);
    }
  }

}