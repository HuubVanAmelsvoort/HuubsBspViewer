using OpenTK.Mathematics;
public class BspBuilder {

  public List<TextureGroep> GenereerDriehoeken(Lader lader, Dictionary<string, (int W, int H)> afmetingen) {
    // We gebruiken een Dictionary om vertices per textuur te groeperen
    Dictionary<string, TextureGroep> groepen = new Dictionary<string, TextureGroep>();
    int texBreedte = 128; // Haal dit uit je geladen Texture object
    int texHoogte = 128;

    foreach (var vlak in lader.Vlakken) {

      // 1. Haal de textuur-info en de naam op
      var texInfo = lader.TextureInfos[vlak.TextureInfoIndex];

      // Zorg dat je in je BspLader de namenlijst hebt gevuld (bijv. "walls/brick1")
      string texNaam = lader.TextureNamen[vlak.TextureInfoIndex];

      // Volgende: speciale behandeling voor lucht-texturen
      if((texInfo.Flags & DiverseHelpFuncties.LUCHT) != 0) {
        // Dit is een sky-vlak. 
        // Voeg het toe aan een speciale groep genaamd "sky"
        texNaam = "internal_sky";
      }

      // 2. Als we deze textuur nog niet eerder hebben gezien, maak een nieuwe lijst aan
      if(!groepen.ContainsKey(texNaam)) {
        groepen[texNaam] = new TextureGroep {
          TextureNaam = texNaam
        };
      }
      // Haal de afmetingen op voor de huidige textuur
      var (breedte, hoogte) = afmetingen.ContainsKey(texNaam) ? afmetingen[texNaam] : (128, 128);

      // 3. Haal de punten van het vlak op
      List<Vector3> punten = DiverseHelpFuncties.HaalVlakPuntenOp(vlak, lader);

      // 4. Maak driehoeken van de polygon (Triangle Fan)
      for (int i = 1; i < punten.Count - 1; i++) {
        // Voeg de 3 punten van de driehoek toe aan de lijst van SPECIFIEKE textuur
        VoegPuntToe(groepen[texNaam].VertexData, punten[0], texInfo);
        VoegPuntToe(groepen[texNaam].VertexData, punten[i], texInfo);
        VoegPuntToe(groepen[texNaam].VertexData, punten[i + 1], texInfo);
      }
    }

    return groepen.Values.ToList();
  }

  private void VoegPuntToe(List<float> lijst, Vector3 punt, BspTexInfo tex) {
    // Haal de maten op uit je register (bijv. 256x256)
    var afmeting = DiverseHelpFuncties.textureAfmetingen[tex.TextureNaam]; // Positie (Gebruik de assen die voor jou werkten, bijv. X, Y, Z)

    lijst.Add(punt.X);
    lijst.Add(punt.Y);
    lijst.Add(punt.Z);

    float u = (Vector3.Dot(punt, tex.U_As) + tex.U_Offset) * 2.0f / afmeting.breedte;
    float v = (Vector3.Dot(punt, tex.V_As) + tex.V_Offset) * 2.0f / afmeting.hoogte;

    lijst.Add(u);
    lijst.Add(v);
  }
}
