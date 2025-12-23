using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class Shader {
  public int Handle;

  public Shader() {
    // Compileer Vertex Shader
    int vShader = GL.CreateShader(ShaderType.VertexShader);
    GL.ShaderSource(vShader, DiverseHelpFuncties.vertexCode);
    GL.CompileShader(vShader);

    // Compileer Fragment Shader
    int fShader = GL.CreateShader(ShaderType.FragmentShader);
    GL.ShaderSource(fShader, DiverseHelpFuncties.fragmentCode);
    GL.CompileShader(fShader);

    // Link naar Programma
    Handle = GL.CreateProgram();
    GL.AttachShader(Handle, vShader);
    GL.AttachShader(Handle, fShader);
    GL.LinkProgram(Handle);
    GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int code2);
    if (code2 != (int)All.True) {
      string infoLog = GL.GetProgramInfoLog(Handle);
      Console.WriteLine($"FOUT2: Shader Linken mislukt: {infoLog}");
    }

    GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int code);
    if (code != (int)All.True) {
      string infoLog = GL.GetProgramInfoLog(Handle);
      Console.WriteLine($"FOUT3: Shader Linken mislukt: {infoLog}");
    }
  }
  public void SetMatrix4(string naam, Matrix4 data) {
    // 1. Zoek de locatie van de variabele in de shader (bijv. "model" of "view")
    int location = GL.GetUniformLocation(Handle, naam);

    // 2. Stuur de matrix-data naar de GPU
    // false = niet transcponeren (OpenTK matrices zijn al in het juiste formaat voor OpenGL)
    GL.UniformMatrix4(location, false, ref data);
  }
  public void Gebruiken() => GL.UseProgram(Handle);
}