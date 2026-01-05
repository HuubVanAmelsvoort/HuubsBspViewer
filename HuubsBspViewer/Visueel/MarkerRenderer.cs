using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Globalization;

public class MarkerRenderer {
  private readonly int _vao;
  private readonly int _vbo;
  private readonly int _vertexCount;

  public MarkerRenderer() {
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
    _vertexCount = vertices.Length / 3; // Dit wordt nu 36

    _vao = GL.GenVertexArray();
    _vbo = GL.GenBuffer();
    GL.BindVertexArray(_vao);
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
    GL.EnableVertexAttribArray(0);
  }
  public void TekenMarkers(AlgemeneShader shader, List<EntiteitAlgemeen> doelen) {
    shader.Gebruiken();
    //int isMarkerLoc = GL.GetUniformLocation(shader.Handle, "isMarker");

    int markerFlagLoc = GL.GetUniformLocation(shader.Handle, "isMarker");
    int colorLoc = GL.GetUniformLocation(shader.Handle, "markerKleur");

    GL.Uniform1(markerFlagLoc, 1);

    GL.BindVertexArray(_vao);

    foreach(EntiteitAlgemeen doel in doelen) {
      Vector3 nieuwePositie = ParseQuakeCoordinaten(doel.Positie.ToString());

      //if(doel.ClassNaam == "target_speaker") {
      //  Console.WriteLine($"Speaker gevonden op: X:{doel.Positie.X} Y:{doel.Positie.Y} Z:{doel.Positie.Z} - Geluid: {doel.GeluidPad}");
      //}
      // De 'doel.Positie' is hier al de geconverteerde Vector3
      Matrix4 model = Matrix4.CreateTranslation(doel.Positie);
      shader.SetMatrix4("model", model);

      // Bepaal kleur op basis van type
      Vector4 kleur = doel.ClassNaam == "target_speaker"
          ? new Vector4(0.0f, 1.0f, 1.0f, 1.0f) // Cyaan voor geluid
          : new Vector4(1.0f, 1.0f, 0.0f, 1.0f); // Geel voor de rest

      // Zet de model matrix voor dit specifieke blokje
      //Matrix4 model = Matrix4.CreateTranslation(doel.Positie);
      //shader.SetMatrix4("model", model);
      //shader.SetVector4("markerKleur", kleur); // Als je shader kleur ondersteunt
      //int colorLoc = GL.GetUniformLocation(shader.Handle, "markerKleur");
      GL.Uniform4(colorLoc, kleur);

      GL.DrawArrays(PrimitiveType.Triangles, 0, _vertexCount);
    }
    GL.Uniform1(markerFlagLoc, 0);
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