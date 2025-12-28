// SkyboxShader.cs
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class SkyboxShader {
  public int Handle { get; private set; }

  private int GetUniformLocation(string name) {
    return GL.GetUniformLocation(Handle, name);
  }

  public SkyboxShader() {
    int vertexShader = GL.CreateShader(ShaderType.VertexShader);
    GL.ShaderSource(vertexShader, DiverseHelpFuncties.vertexCodeLucht);
    GL.CompileShader(vertexShader);

    int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
    GL.ShaderSource(fragmentShader, DiverseHelpFuncties.fragmentCodeLucht);
    GL.CompileShader(fragmentShader);

    Handle = GL.CreateProgram();
    GL.AttachShader(Handle, vertexShader);
    GL.AttachShader(Handle, fragmentShader);
    GL.LinkProgram(Handle);

    GL.DetachShader(Handle, vertexShader);
    GL.DetachShader(Handle, fragmentShader);

    GL.DeleteShader(vertexShader);
    GL.DeleteShader(fragmentShader);
  }

  public void Gebruiken() {
    GL.UseProgram(Handle);
  }

  public void SetMatrix4(string name, Matrix4 data) {
    GL.UseProgram(Handle);
    // was    GL.UniformMatrix4(GetUniformLocation(name), true, ref data);
    GL.UniformMatrix4(GetUniformLocation(name), false, ref data);
  }
  public void SetInt(string name, int data) {
    GL.UseProgram(Handle);
    GL.Uniform1(GetUniformLocation(name), data);
  }
}