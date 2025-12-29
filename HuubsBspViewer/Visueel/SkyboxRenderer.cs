// SkyboxRenderer.cs
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp; // Voor het laden van PNG's

public class SkyboxRenderer {
  private int _vao;
  private int _vbo;
  private int _cubemapHandle;
  private AlgemeneShader _LuchtShader;

  // De hoekpunten van een kubus die we tekenen
  private readonly float[] _vertices = {
        // Posities (1)
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        // vlak (2)
        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,
        // vlak (3)
         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
        // vlak (4)
        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,
        // vlak (5)
        -1.0f,  1.0f, -1.0f,
         1.0f,  1.0f, -1.0f,
         1.0f,  1.0f,  1.0f,
         1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,
        // vlak (6)
        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
         1.0f, -1.0f, -1.0f,
         1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
         1.0f, -1.0f,  1.0f
    };

  public SkyboxRenderer(List<string> texturePaden) {
    _LuchtShader = new AlgemeneShader(DiverseHelpFuncties.vertexCodeLucht, DiverseHelpFuncties.fragmentCodeLucht); // Compileer de skybox shader

    // 1. VAO en VBO voor de kubus
    _vao = GL.GenVertexArray();
    _vbo = GL.GenBuffer();

    GL.BindVertexArray(_vao);
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

    GL.EnableVertexAttribArray(0);
    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

    // 2. Laad de 6 CubeMap textures
    _cubemapHandle = LaadCubeMap(texturePaden);
  }

  private int LaadCubeMap(List<string> paden) {
    int handle = GL.GenTexture();
    GL.BindTexture(TextureTarget.TextureCubeMap, handle);
    StbImage.stbi_set_flip_vertically_on_load(0); // draait de afbeeldingen niet om

    // Volgorde voor Quake 2 is: (Left, Right, Up, Down, Front, Back)
    // Maar OpenGL verwacht: (+X, -X, +Y, -Y, +Z, -Z)
    // Je moet dus de juiste PNG's aan de juiste Target koppelen!
    // Bijv. "rt" (right) is +X, "lf" (left) is -X etc.
    // Dit is een voorbeeldvolgorde, pas aan op basis van je Quake 2 PNG-namen!
    TextureTarget[] targets = {
            TextureTarget.TextureCubeMapPositiveX, // rt
            TextureTarget.TextureCubeMapNegativeX, // lf
            TextureTarget.TextureCubeMapPositiveY, // up
            TextureTarget.TextureCubeMapNegativeY, // dn
            TextureTarget.TextureCubeMapPositiveZ, // bk (back)
            TextureTarget.TextureCubeMapNegativeZ  // ft (front)
        };

    for(int i = 0; i < paden.Count; i++) {
      Console.WriteLine("Laden skybox: " + Path.GetFullPath(paden[i]));

      using(Stream stream = File.OpenRead(paden[i])) {
        // Forceer 4 componenten voor consistentie, ook al is het 3 (RGB)
        ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        GL.TexImage2D(targets[i], 0, PixelInternalFormat.Rgba,
                      image.Width, image.Height, 0, PixelFormat.Rgba,
                      PixelType.UnsignedByte, image.Data);
      }
    }

    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

    // TODO Huub de volgende 3 weer aanzetten als je wilt dat de skybox niet herhaalt
    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

    return handle;
  }

  public void Render(Matrix4 view, Matrix4 projection) {
    GL.DepthFunc(DepthFunction.Lequal); // Zorgt ervoor dat de skybox achter alles komt

    _LuchtShader.Gebruiken();
    _LuchtShader.SetMatrix4("view", view);
    _LuchtShader.SetMatrix4("projection", projection);

    _LuchtShader.SetInt("skybox", 0); // Activeer Texture Unit 0

    GL.BindVertexArray(_vao);
    GL.ActiveTexture(TextureUnit.Texture0); // Zeg dat we unit 0 gebruiken
    GL.BindTexture(TextureTarget.TextureCubeMap, _cubemapHandle);

    GL.Disable(EnableCap.CullFace);
    GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // Teken de kubus (36 vertices)

    GL.DepthFunc(DepthFunction.Less); // Zet terug naar standaard dieptetest
  }
}