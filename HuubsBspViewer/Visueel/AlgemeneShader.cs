using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class AlgemeneShader { // basis om vanaf te leiden
  public int Handle { get; private set; }

  public AlgemeneShader(string vertexCode, string fragmentCode) {
    // Compileer Vertex Shader
    int vShader = GL.CreateShader(ShaderType.VertexShader);
    GL.ShaderSource(vShader, vertexCode);
    GL.CompileShader(vShader);
    CheckShaderError(vShader, "Vertex");

    // Compileer Fragment Shader
    int fShader = GL.CreateShader(ShaderType.FragmentShader);
    GL.ShaderSource(fShader, fragmentCode);
    GL.CompileShader(fShader);
    CheckShaderError(vShader, "Fragment");

    // Link naar Programma
    Handle = GL.CreateProgram();
    GL.AttachShader(Handle, vShader);
    GL.AttachShader(Handle, fShader);
    GL.LinkProgram(Handle);
    CheckProgramError(Handle);

    // Opruimen
    GL.DetachShader(Handle, vShader);
    GL.DetachShader(Handle, fShader);
    GL.DeleteShader(vShader);
    GL.DeleteShader(fShader);
  }
  public void Gebruiken() => GL.UseProgram(Handle);
  public void SetMatrix4(string naam, Matrix4 data) {
    // 1. Zoek de locatie van de variabele in de shader (bijv. "model" of "view")
    int location = GL.GetUniformLocation(Handle, naam);

    // 2. Stuur de matrix-data naar de GPU
    // false = niet transponeren (OpenTK matrices zijn al in het juiste formaat voor OpenGL)
    GL.UniformMatrix4(location, false, ref data);
  }
  public void SetInt(string naam, int data) {
    int location = GL.GetUniformLocation(Handle, naam);
    GL.Uniform1(location, data);
  }

  private void CheckShaderError(int shader, string type) {
    GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
    if(success == 0) {
      string infoLog = GL.GetShaderInfoLog(shader);
      Console.WriteLine($"ERROR: {type} Shader Compilatie mislukt:\n{infoLog}");
    }
  }

  private void CheckProgramError(int program) {
    GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
    if(success == 0) {
      string infoLog = GL.GetProgramInfoLog(program);
      Console.WriteLine($"ERROR: Shader Linken mislukt:\n{infoLog}");
    }
  }
  internal void SetVector4(string naam, Vector4 data) {
    // 1. Zoek de locatie van de variabele in de shader (bijv. "model" of "view")
    int location = GL.GetUniformLocation(Handle, naam);

    // 2. Stuur de matrix-data naar de GPU
    GL.Uniform4(location, data.X, data.Y, data.Z, data.W);
  }
}