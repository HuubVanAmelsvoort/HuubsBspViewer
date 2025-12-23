using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Reflection.Emit;
public class BspBuilder {

  public List<TextureGroep> GenereerDriehoeken(BspLader lader) {
    // We gebruiken een Dictionary om vertices per textuur te groeperen
    Dictionary<string, TextureGroep> groepen = new Dictionary<string, TextureGroep>();

    foreach (var vlak in lader.Vlakken) {
      // 1. Haal de textuur-info en de naam op
      var texInfo = lader.TextureInfos[vlak.TextureInfoIndex];

      // Zorg dat je in je BspLader de namenlijst hebt gevuld (bijv. "walls/brick1")
      string texNaam = lader.TextureNamen[vlak.TextureInfoIndex];

      // 2. Als we deze textuur nog niet eerder hebben gezien, maak een nieuwe lijst aan
      if (!groepen.ContainsKey(texNaam)) {
        groepen[texNaam] = new TextureGroep {
          TextureNaam = texNaam
        };
      }

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
    // Positie (Gebruik de assen die voor jou werkten, bijv. X, Y, Z)
    lijst.Add(punt.X);
    lijst.Add(punt.Y);
    lijst.Add(punt.Z);

    // UV-berekening (met de 1.0 - v fix voor ondersteboven textures)
    float u = (Vector3.Dot(punt, tex.U_As) + tex.U_Offset) / 64.0f;
    float v = 1.0f - ((Vector3.Dot(punt, tex.V_As) + tex.V_Offset) / 64.0f);

    lijst.Add(u);
    lijst.Add(v);
  }
  private void DeTexturing(BspLader lader, BspVlak vlak, Vector3 Punt) {
    // Voor elk hoekpunt (Punt) in een Vlak:
    var tex = lader.TextureInfos[vlak.TextureInfoIndex];

    // De Quake-formule voor texture mapping:
    float u = Vector3.Dot(Punt, tex.U_As) + tex.U_Offset;
    float v = Vector3.Dot(Punt, tex.V_As) + tex.V_Offset;

    // Belangrijk: Voorlopig delen we door een standaard grootte (bijv. 64 of 128)
    // omdat we de echte breedte van het .wal bestand nog niet weten.
    float finaleU = u / 64.0f;
    float finaleV = v / 64.0f;
  }
}
