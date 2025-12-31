using OpenTK.Mathematics;

public class BspBotsing {
  private readonly BspLader _data;
  public BspBotsing(BspLader geladenData) {
    _data = geladenData; // We gebruiken de reeds geladen data
  }
  public Vector3 ValideerVerplaatsing(Vector3 huidigePos, Vector3 doelPos) {
    // Recursief door de boom wandelen om te zien of doelPos in een 'Leaf' (lege ruimte) valt
    if (IsPuntInSolideRuimte(doelPos, 0)) {
      // Als de nieuwe plek solide is, mag de speler daar niet heen.
      // Voor nu blokkeren we de beweging volledig (stoppen voor de muur).
      return huidigePos;
    }
    return doelPos;
  }

  public bool IsPuntInSolideRuimte(Vector3 punt, int knoopIndex) {
    if (knoopIndex < 0) {
      int bladIndex = ~knoopIndex; // Bitwise NOT om de echte index te krijgen
      var blad = _data.Bladeren[bladIndex];
      //Console.WriteLine($"blad.Inhoud: {blad.Inhoud}");
      // In Quake 2 BSP: 
      // CONTENTS_SOLID is 1. We kijken of dit bitje aan staat.
      return (blad.Inhoud & 1) != 0;
    }

    var knoop = _data.Knooppunten[knoopIndex];
    var vlak = _data.Planes[knoop.VlakIndex];

    // Quake assen naar OpenGL assen voor de berekening
    //Vector3 normaal = new Vector3(vlak.Normal.X, vlak.Normal.Z, -vlak.Normal.Y);
    Vector3 normaal = new Vector3(vlak.Normal.X, vlak.Normal.Y, vlak.Normal.Z);

    // Bereken aan welke kant van het vlak het punt staat
    float afstand = Vector3.Dot(punt, normaal) - vlak.Afstand;

    if (afstand >= 0) {
      return IsPuntInSolideRuimte(punt, knoop.Kinderen[0]);
    } else {
      return IsPuntInSolideRuimte(punt, knoop.Kinderen[1]);
    }
  }
  /**
   * afkomstig van CheckCollision
   */
  public bool ControleerBotsing(Vector3 spelerPos, TriggerZone zone) {
    return spelerPos.X >= zone.Mins.X && spelerPos.X <= zone.Maxs.X &&
           spelerPos.Y >= zone.Mins.Y && spelerPos.Y <= zone.Maxs.Y &&
           spelerPos.Z >= zone.Mins.Z && spelerPos.Z <= zone.Maxs.Z;
  }
}