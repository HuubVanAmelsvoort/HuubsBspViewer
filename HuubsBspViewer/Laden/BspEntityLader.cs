using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
public class BspEntityLader {
  public Vector3 StartPositie { get; private set; } = new Vector3(0, 100, 0);

  private Vector3 ExtraheerOrigin(string entiteitTekst) {
    string[] lijnen = entiteitTekst.Split('\n');
    foreach (var lijn in lijnen) {
      if (lijn.Contains("\"origin\"")) {
        // De lijn ziet eruit als: "origin" "128 -256 16"
        string waarden = lijn.Replace("\"origin\"", "").Replace("\"", "").Trim();
        string[] assen = waarden.Split(' ');

        if (assen.Length == 3) {
          float x = float.Parse(assen[0]);
          float y = float.Parse(assen[1]);
          float z = float.Parse(assen[2]);
          // Let op: ook hier Quake Z -> OpenGL Y
          return new Vector3(x, z, -y);
        }
      }
    }
    return new Vector3(0, 100, 0);
  }
}