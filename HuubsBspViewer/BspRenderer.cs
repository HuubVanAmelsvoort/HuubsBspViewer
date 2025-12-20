using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

public class BspRenderer {
  private int _vbo; // Vertex Buffer Object
  private int _vao; // Vertex Array Object
  private int _aantalPunten;
  public void Voorbereiden(List<Vector3> punten) {
    _aantalPunten = punten.Count;
    // We maken ruimte voor Positie (3 floats) EN Kleur (3 floats) per punt
    float[] data = new float[punten.Count * 6];

    Random rand = new Random();

    for (int i = 0; i < punten.Count; i++) {
      // Positie (X, Y, Z)
      data[i * 6 + 0] = punten[i].X;
      data[i * 6 + 1] = punten[i].Z;  // Hoogte
      data[i * 6 + 2] = -punten[i].Y; // Diepte

      // Kleur (R, G, B) - Genereer een kleur per driehoek (per 3 punten)
      if (i % 3 == 0) {
        float r = (float)rand.NextDouble();
        float g = (float)rand.NextDouble();
        float b = (float)rand.NextDouble();

        for (int j = 0; j < 3; j++) { // Geef alle 3 de punten van de driehoek dezelfde kleur
          if (i + j < punten.Count) {
            data[(i + j) * 6 + 3] = r;
            data[(i + j) * 6 + 4] = g;
            data[(i + j) * 6 + 5] = b;
          }
        }
      }
    }

    _vao = GL.GenVertexArray();
    GL.BindVertexArray(_vao);

    _vbo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

    // Layout 0: Positie (3 floats)
    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
    GL.EnableVertexAttribArray(0);

    // Layout 1: Kleur (3 floats, begint na de eerste 3 floats)
    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
    GL.EnableVertexAttribArray(1);
  }
  public void Tekenen() {
    GL.BindVertexArray(_vao);
    GL.DrawArrays(PrimitiveType.Triangles, 0, _aantalPunten);
  }
}