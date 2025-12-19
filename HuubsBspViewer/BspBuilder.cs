using OpenTK.Mathematics;
using System.Collections.Generic;

public class BspBuilder {
  //public List<Vector3> GenereerDriehoeken(BspLader lader) {
  //  List<Vector3> driehoekPunten = new List<Vector3>();

  //  foreach (var vlak in lader.Vlakken) {
  //    List<Vector3> vlakPunten = new List<Vector3>();

  //    // 1. Verzamel alle hoekpunten van dit vlak
  //    for (int i = 0; i < vlak.AantalEdges; i++) {
  //      int surfEdge = lader.SurfEdges[vlak.EersteEdgeIndex + i];
  //      BspRand rand = lader.Randen[Math.Abs(surfEdge)];

  //      // Als surfEdge negatief is, draaien we de rand om (volgorde is belangrijk!)
  //      int vIndex = (surfEdge >= 0) ? rand.V1 : rand.V2;
  //      vlakPunten.Add(lader.Hoekpunten[vIndex]);
  //    }

  //    // 2. Maak driehoeken van de polygoon (Triangle Fan methode)
  //    for (int i = 1; i < vlakPunten.Count - 1; i++) {
  //      driehoekPunten.Add(vlakPunten[0]);
  //      driehoekPunten.Add(vlakPunten[i]);
  //      driehoekPunten.Add(vlakPunten[i + 1]);
  //    }
  //  }
  //  return driehoekPunten;
  //}
  public List<Vector3> GenereerDriehoeken(BspLader lader) {
    List<Vector3> driehoekPunten = new List<Vector3>();

    foreach (var vlak in lader.Vlakken) {
      List<Vector3> vlakPunten = new List<Vector3>();

      for (int i = 0; i < vlak.AantalEdges; i++) {
        // SurfEdge is de index in de lijst van randen
        int surfEdgeIndex = lader.SurfEdges[vlak.EersteEdgeIndex + i];

        // Haal de rand op (gebruik de absolute waarde van de index)
        BspRand rand = lader.Randen[Math.Abs(surfEdgeIndex)];

        // BELANGRIJK: Als de surfEdge negatief is, betekent dit dat we 
        // de rand 'omgekeerd' moeten aflopen om de juiste volgorde te houden.
        int vIndex;
        if (surfEdgeIndex >= 0) {
          vIndex = rand.V1;
        } else {
          vIndex = rand.V2;
        }

        vlakPunten.Add(lader.Hoekpunten[vIndex]);
      }

      // Maak driehoeken van de verzamelde punten (Triangle Fan)
      for (int i = 1; i < vlakPunten.Count - 1; i++) {
        driehoekPunten.Add(vlakPunten[0]);
        driehoekPunten.Add(vlakPunten[i]);
        driehoekPunten.Add(vlakPunten[i + 1]);
      }
    }
    return driehoekPunten;
  }
}
