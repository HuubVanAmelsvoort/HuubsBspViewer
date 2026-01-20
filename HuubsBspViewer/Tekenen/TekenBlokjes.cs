using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Globalization;

public class TekenBlokjes {
  private readonly int _vao;
  private readonly int _vbo;
  private readonly int _vertexCount;

  public TekenBlokjes() {
    // Definiëer een kleine kubus (-8 tot 8 units)
    float[] vertices = {
    // Achterkant
    -8f, -8f, -8f,  8f, -8f, -8f,  8f,  8f, -8f,
     8f,  8f, -8f, -8f,  8f, -8f, -8f, -8f, -8f,
    // Voorkant
    -8f, -8f,  8f,  8f, -8f,  8f,  8f,  8f,  8f,
     8f,  8f,  8f, -8f,  8f,  8f, -8f, -8f,  8f,
    // Linkerkant
    -8f,  8f,  8f, -8f,  8f, -8f, -8f, -8f, -8f,
    -8f, -8f, -8f, -8f, -8f,  8f, -8f,  8f,  8f,
    // Rechterkant
     8f,  8f,  8f,  8f,  8f, -8f,  8f, -8f, -8f,
     8f, -8f, -8f,  8f, -8f,  8f,  8f,  8f,  8f,
    // Onderkant
    -8f, -8f, -8f,  8f, -8f, -8f,  8f, -8f,  8f,
     8f, -8f,  8f, -8f, -8f,  8f, -8f, -8f, -8f,
    // Bovenkant
    -8f,  8f, -8f,  8f,  8f, -8f,  8f,  8f,  8f,
     8f,  8f,  8f, -8f,  8f,  8f, -8f,  8f, -8f
      };
    _vertexCount = vertices.Length / 3; 

    _vao = GL.GenVertexArray();
    _vbo = GL.GenBuffer();

    GL.BindVertexArray(_vao);
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

    DiverseHelpFuncties.ControleerGLFout("TekenBlokjes");

    GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
    GL.EnableVertexAttribArray(0);
  }
  public void TekenBlokjesDiverseKleuren(AlgemeneShader shader, List<EntiteitAlgemeen> entiteitenAlgemeen) {
    shader.Gebruiken();

    Vector4 kleur;
    int isMarker = GL.GetUniformLocation(shader.Handle, "isMarker");
    int markerKleur = GL.GetUniformLocation(shader.Handle, "markerKleur");

    GL.Uniform1(isMarker, 1); // zet de marker modus aan in de shader

    GL.BindVertexArray(_vao);

    foreach(EntiteitAlgemeen entiteitAlgemeen in entiteitenAlgemeen) {
      Vector3 nieuwePositie = ParseQuakeCoordinaten(entiteitAlgemeen.Positie.ToString());

      // Bepaal kleur op basis van type
      Vector3 kleur3 = DiverseHelpFuncties.KleurVanClassNaam(entiteitAlgemeen.ClassNaam);
      kleur = new Vector4(kleur3, 1.0f);

      // De 'doel.Positie' is hier al de geconverteerde Vector3
      Matrix4 model = Matrix4.CreateTranslation(entiteitAlgemeen.Positie);
      shader.SetMatrix4("model", model);

      GL.Uniform4(markerKleur, kleur);
      GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
    }

    GL.Uniform1(isMarker, 0); // zet de marker modus uit in de shader
  }
  public Vector3 ParseQuakeCoordinaten(string input) {
    // Verwijder haakjes en vervang puntkomma's door spaties
    string schoon = input.Replace("(", "").Replace(")", "").Replace(";", " ");

    var delen = schoon.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if(delen.Length >= 3) {
      // Quake assen inlezen
      float qX = float.Parse(delen[0], CultureInfo.InvariantCulture);
      float qY = float.Parse(delen[1], CultureInfo.InvariantCulture);
      float qZ = float.Parse(delen[2], CultureInfo.InvariantCulture);

      // DIRECT DE CONVERSIE:
      // Quake X -> OpenGL X
      // Quake Z -> OpenGL Y (omhoog)
      // Quake Y -> OpenGL -Z (diepte)
      return new Vector3(qX, qZ, -qY);
    }

    return Vector3.Zero;
  }
}