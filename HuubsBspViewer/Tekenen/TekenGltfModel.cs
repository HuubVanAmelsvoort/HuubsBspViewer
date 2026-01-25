//using OpenTK.Graphics.OpenGL;
//using SharpGLTF.Runtime;
//using TK = OpenTK.Mathematics;
//using SN = System.Numerics;
//public class TekenGltfModel {
//  private readonly int _vao;
//  private readonly int _vbo;
//  private readonly int _vertexCount;
//  //private readonly int _sneeuwpopTextureHandle;
//  public TekenGltfModel(string pad) {
//    var model = SharpGLTF.Schema2.ModelRoot.Load(pad);
//    var scene = model.DefaultScene;
//    var instance = SharpGLTF.Runtime. RuntimeModelLayer.CreateSceneInstance(scene);
//    List<float> vertexData = new List<float>();

//    foreach(var mesh in model.LogicalMeshes) {
//      foreach(var primitive in mesh.Primitives) {
//        // Haal posities op
//        var positions = primitive.GetVertexAccessor("POSITION").AsVector3Array();
//        var uvAccessor = primitive.GetVertexAccessor("TEXCOORD_0").AsVector2Array();
//        //var indices = primitive.GetIndexAccessor().AsIndicesArray();

//        // Als je model indices gebruikt (meestal wel bij glTF)
//        if(primitive.IndexAccessor != null) {
//          var indices = primitive.GetIndexAccessor().AsIndicesArray();
//          foreach(var idx in indices) {
//            var pos = ToTK(positions[(int)idx]);
//            vertexData.Add(pos.X);
//            vertexData.Add(pos.Y);
//            vertexData.Add(pos.Z);

//            // UV coördinaten
//            var uv = uvAccessor[(int)idx];
//            vertexData.Add(uv.X);
//            vertexData.Add(1.0f - uv.Y); // glTF is top-left, OpenGL is bottom-left
//            //vertexData.Add(uv.Y); // glTF is top-left, OpenGL is bottom-left
//          } // eind foreach indices
//        } else {
//          // Geen indices, gewoon vertices achter elkaar
//          foreach(var posNum in positions) {
//            var pos = ToTK(posNum);
//            vertexData.Add(pos.X);
//            vertexData.Add(pos.Y);
//            vertexData.Add(pos.Z);
//          }
//        }
//      }
//    }

//    _vertexCount = vertexData.Count / 3;
//    float[] vertices = vertexData.ToArray();

//    int stride = 5 * sizeof(float);

//    _vao = GL.GenVertexArray();
//    _vbo = GL.GenBuffer();

//    GL.BindVertexArray(_vao);
//    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
//    GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

//    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
//    GL.EnableVertexAttribArray(0);

//    // Locatie 1: UV (2 floats)
//    // De '3 * sizeof(float)' is de offset: de UV's beginnen na de X,Y,Z
//    GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
//    GL.EnableVertexAttribArray(1);
//  }

//  private TK.Vector3 ToTK(SN.Vector3 v) {
//    // eerst kijken of het wel nodig is om te converteren
//    TK.Vector3 posNum = new TK.Vector3(v.X, v.Z, v.Y); // Direct je Quake-fix toegepast!
//    //TK.Vector3 posNum = new TK.Vector3(v.X, v.Y, v.Z); 

//    return posNum;
//  }

//  public void Teken(AlgemeneShader shader, TK.Vector3 positie, TK.Vector3 schaal) {
//    shader.Gebruiken();

//    GL.ActiveTexture(TextureUnit.Texture0);
//    Texture texture = new Texture(@"D:\\werkdirectory\\BlockbenchBestanden\\VolledigeSneeuwpop.png");
//    texture.Gebruik();

//    GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
//    // Zorg dat je shader een sampler2D uniform heeft voor de texture

//    // Matrix maken: Positie * Schaal (Blockbench modellen zijn vaak klein, dus schaal ze evt. op)
//    TK.Matrix4 model = TK.Matrix4.CreateScale(schaal) * TK.Matrix4.CreateTranslation(positie);
//    shader.SetMatrix4("model", model);

//    GL.BindVertexArray(_vao);
//    GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);

//  }
//}
//using OpenTK.Graphics.OpenGL;
//using SharpGLTF.Runtime;
