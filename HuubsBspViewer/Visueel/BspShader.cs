using OpenTK.Graphics.OpenGL4;
public class BspShader {
  public int Handle;

  public BspShader(string vertexCode, string fragmentCode) {
    // Compileer Vertex Shader
    int vShader = GL.CreateShader(ShaderType.VertexShader);
    GL.ShaderSource(vShader, vertexCode);
    GL.CompileShader(vShader);

    // Compileer Fragment Shader
    int fShader = GL.CreateShader(ShaderType.FragmentShader);
    GL.ShaderSource(fShader, fragmentCode);
    GL.CompileShader(fShader);

    // Link naar Programma
    Handle = GL.CreateProgram();
    GL.AttachShader(Handle, vShader);
    GL.AttachShader(Handle, fShader);
    GL.LinkProgram(Handle);
  }

  public void Gebruiken() => GL.UseProgram(Handle);
}