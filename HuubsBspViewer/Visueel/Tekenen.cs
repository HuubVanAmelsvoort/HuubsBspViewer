using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
public class Tekenen {
  // We slaan nu per textuur de GPU-data op
  private Dictionary<string, (int vao, int vbo, int aantal)> _batches = new();
  public void Voorbereiden(List<TextureGroep> groepen) {
    foreach (var groep in groepen) {

      if(groep.TextureNaam.Contains("sky")) {
        var d = 1;
        continue;
      }

      string texNaam = groep.TextureNaam;
      float[] data = groep.VertexData.ToArray(); // Hier zet je de List om naar Array
      int aantalPunten = data.Length / 5;

      int VertexTabelInstructies = GL.GenVertexArray();
      int VertexBufferMagazijn = GL.GenBuffer();

      GL.BindVertexArray(VertexTabelInstructies);
      GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferMagazijn);
      GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

      int stride = 5 * sizeof(float);
      // Positie
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
      GL.EnableVertexAttribArray(0);
      // UV
      GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
      GL.EnableVertexAttribArray(1);

      // Bewaar de handles in onze Dictionary
      _batches[texNaam] = (VertexTabelInstructies, VertexBufferMagazijn, aantalPunten);
    }
  }
  public void HetTekenen(Dictionary<string, Texture> geladenTextures, Shader shader) {
    foreach (KeyValuePair<string, (int vao, int vbo, int aantal)> batch in _batches) {
      string texNaam = batch.Key;
      var (VertexArrayObjectInstructies, VertexBufferObjectMagazijn, aantal) = batch.Value;

      // 1. Activeer de juiste textuur voor deze batch
      if (geladenTextures.ContainsKey(texNaam)) {
        geladenTextures[texNaam].Gebruik();
      }

      // 2. Teken de muren die bij deze textuur horen
      GL.BindVertexArray(VertexArrayObjectInstructies);
      GL.DrawArrays(PrimitiveType.Triangles, 0, aantal);
    }
  }
}
