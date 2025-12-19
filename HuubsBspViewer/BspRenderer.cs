using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

public class BspRenderer {
  private int _vbo; // Vertex Buffer Object
  private int _vao; // Vertex Array Object
  private int _aantalPunten;

  public void Voorbereiden(List<Vector3> punten) {
    _aantalPunten = punten.Count;
    float[] data = new float[punten.Count * 3];

    for (int i = 0; i < punten.Count; i++) {
      data[i * 3] = punten[i].X;
      data[i * 3 + 1] = punten[i].Z; // Quake Z is omhoog, OpenGL Y is omhoog
      data[i * 3 + 2] = -punten[i].Y; // Assen aanpassen voor juiste weergave
    }

    _vao = GL.GenVertexArray();
    GL.BindVertexArray(_vao);

    _vbo = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
    GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

    // Vertel OpenGL dat we 3 floats per hoekpunt gebruiken (X, Y, Z)
    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
    GL.EnableVertexAttribArray(0);
  }

  public void Tekenen() {
    GL.BindVertexArray(_vao);
    GL.DrawArrays(PrimitiveType.Triangles, 0, _aantalPunten);
  }
}