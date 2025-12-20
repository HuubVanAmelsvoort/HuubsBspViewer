using OpenTK.Mathematics;
public static class BspHelper {
  public static Vector3 ZoekStartPositie(string entiteitenTekst) {
    // Standaardpositie als er niets gevonden wordt
    Vector3 startPos = new Vector3(0, 100, 0);

    string[] entiteiten = entiteitenTekst.Split('}');
    foreach (var ent in entiteiten) {
      if (ent.Contains("info_player_start")) {
        string[] lijnen = ent.Split('\n');
        foreach (var lijn in lijnen) {
          if (lijn.Contains("\"origin\"")) {
            // Haal de getallen uit de regel: "origin" "128 -256 16"
            string waarden = lijn.Replace("\"origin\"", "").Replace("\"", "").Trim();
            string[] assen = waarden.Split(' ');

            if (assen.Length == 3) {
              float x = float.Parse(assen[0]);
              float y = float.Parse(assen[1]);
              float z = float.Parse(assen[2]);
              // Quake -> OpenGL: (X, Z, -Y)
              return new Vector3(x, z, -y);
            }
          }
        }
      }
    }
    return startPos;
  }
}