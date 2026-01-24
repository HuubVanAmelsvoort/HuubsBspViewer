using OpenTK.Graphics.OpenGL4;

public class TekenLevel {
  // We slaan nu per textuur de GPU-data op
  private Dictionary<string, (int vao, int vbo, int aantal)> MurenEnPlaatjes = new();
  public void Voorbereiden(List<TextureGroep> groepen) {
    foreach (var groep in groepen) {
      string texNaam = groep.TextureNaam.ToLower(); // Veiligheidshalve lowercase
      if(texNaam.Contains("sky")
      || texNaam.Contains("trigger")
      || texNaam.Contains("clip")
      || texNaam.Contains("null")
      || texNaam.Contains("hint")
      || texNaam.Contains("skip")
      ) {
        continue;
      }
      
      //Console.WriteLine($"Voorbereiden textuur: {texNaam}");

      texNaam = groep.TextureNaam;
      float[] data = groep.VertexData.ToArray(); // Hier zet je de List om naar Array

      data = VeranderDataVoorTexture(data);
      
      int aantalPunten = data.Length / 5;

      int VertexTabelInstructies = GL.GenVertexArray();
      int VertexBufferMagazijn = GL.GenBuffer();

      GL.BindVertexArray(VertexTabelInstructies);
      GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferMagazijn);

      DiverseHelpFuncties.ControleerGLFout("TekenenVanLevel");

      GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, BufferUsageHint.StaticDraw);

      int stride = 5 * sizeof(float);
      // Positie
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
      GL.EnableVertexAttribArray(0);

      // UV
      GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
      GL.EnableVertexAttribArray(1);

      // Bewaar de handles in onze Dictionary
      MurenEnPlaatjes[texNaam] = (VertexTabelInstructies, VertexBufferMagazijn, aantalPunten);
    }
    var deb=1;
  }

  /**
   * Tekent alle muren en plaatjes die we hebben voorbereid
   */
  public void TekenenVanHetLevel(Dictionary<string, Texture> geladenTextures, AlgemeneShader shader) {
    foreach (KeyValuePair<string, (int vao, int vbo, int aantal)> muurEnPlaatje in MurenEnPlaatjes) {
      string texNaam = muurEnPlaatje.Key;
      var (VertexArrayObjectInstructies, VertexBufferObjectMagazijn, aantal) = muurEnPlaatje.Value;

      // 1. Activeer de juiste textuur voor deze batch
      if (geladenTextures.ContainsKey(texNaam)) {
        geladenTextures[texNaam].Gebruik();
      }

      // 2. Teken de muren die bij deze textuur horen
      GL.BindVertexArray(VertexArrayObjectInstructies);
      GL.DrawArrays(PrimitiveType.Triangles, 0, aantal); 
    }
  }
  private float[] VeranderDataVoorTexture(float[] data) {
    // Loop door de data. Stapgrootte is 5 (X, Y, Z, U, V)
    for(int i = 0; i < data.Length; i += 5) {
      // data[i+3] is de U (S) coördinaat
      // data[i+4] is de V (T) coördinaat

      // Pas hier de schaal aan. 
      // Als hij nu 2x herhaalt, moet je deze waarden door 2 delen.
      data[i + 3] *= 0.5f;
      data[i + 4] *= 0.5f;
      //      data[i + 4] *= -data[i + 4] * 0.5f;
      //float vOorspronkelijk = data[i + 4];
      //data[i + 4] = 1.0f - (vOorspronkelijk * 0.25f);
    }
    return data;
  }

}
