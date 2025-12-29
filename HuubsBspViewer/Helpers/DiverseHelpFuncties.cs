using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;

public static class DiverseHelpFuncties {

  public static string basispadQuake2 = @"D:/werkdirectory/SpelData/baseq2uitgepakt/baseq2";

  private static Vector2 _vorigeMuisPositie;

  public static BspBotsing _botsing; // Komt uit BspVenster
  //========================= Shader code ==============================
  public static Camera _camera; // Komt uit BspVenster
  public static Vector3 startPos = new Vector3(); // -1768 1536 128 uit entiteiten of 128 -320 32
  //========================= Shader code ==============================
  internal static string vertexCode; // shader code wordt ingeladen vanuit bestand in BspLader
  internal static string fragmentCode;

  internal static string vertexCodeLucht;
  internal static string fragmentCodeLucht;
  //========================= Einde Shader code ==============================

  internal static int LUCHT = 4;
  internal static string luchtNaam;
  internal static float deStartHoek;

  internal static void ConsoleLog(string tekst, BspLader lader) {
    Console.WriteLine("[HuubsBspViewer] " + tekst);
    Console.WriteLine("[HuubsBspViewer] Aantal vlakken in map: " + lader.Vlakken.Count);
    Console.WriteLine("[HuubsBspViewer] Aantal hoekpunten in map: " + lader.Hoekpunten.Count);
    Console.WriteLine("[HuubsBspViewer] Aantal randen in map: " + lader.Randen.Count);
    Console.WriteLine("[HuubsBspViewer] Aantal bladeren in map: " + lader.Bladeren.Count);
    Console.WriteLine("[HuubsBspViewer] Aantal knooppunten in map: " + lader.Knooppunten.Count);
    Console.WriteLine("[HuubsBspViewer] Aantal planes in map: " + lader.Planes.Count);
    Console.WriteLine("[HuubsBspViewer] Aantal surfedges in map: " + lader.SurfEdges.Count);
    Console.WriteLine("[HuubsBspViewer] Aantal texture infos in map: " + lader.TextureInfos.Count);
    Console.WriteLine("[HuubsBspViewer] Aantal entiteiten in map: " + lader.entiteiten.Count);

  }
  // De callback methode:
  public static void HuubDebugCallback(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam) {
    if(severity == DebugSeverity.DebugSeverityNotification) return; // negeer notificaties

    string msg = Marshal.PtrToStringAnsi(message, length);
    Console.WriteLine($"[GL DEBUG] {msg}");
  }
  public static List<Vector3> HaalVlakPuntenOp(BspVlak vlak, BspLader lader) {
    List<Vector3> vlijst = new List<Vector3>();
    for(int i = 0; i < vlak.AantalEdges; i++) {
      int surfEdgeIndex = lader.SurfEdges[vlak.EersteEdgeIndex + i];

      if(surfEdgeIndex >= 0) {
        // Positief: gebruik het eerste punt van de edge
        vlijst.Add(lader.Hoekpunten[lader.Randen[surfEdgeIndex].V1]);
      } else {
        // Negatief: gebruik het TWEEDE punt van de edge (omdraaien)
        vlijst.Add(lader.Hoekpunten[lader.Randen[-surfEdgeIndex].V2]);
      }
    }
    return vlijst;
  }
  public static void VerwerkMuis(MouseState muis, Camera _camera) {
    if(muis.IsButtonDown(MouseButton.Left) || true) { // Altijd of bij klik
      float gevoeligheid = 0.2f;
      float deltaX = muis.X - _vorigeMuisPositie.X;
      float deltaY = muis.Y - _vorigeMuisPositie.Y;

      // Roep de camera functie aan die de vectoren herrekent
      _camera.VerwerkMuis(deltaX, deltaY);
    }
    _vorigeMuisPositie = new Vector2(muis.X, muis.Y);
  }
  internal static Vector3 BerekenBeweging(KeyboardState input, float tijd) {
    float snelheid = 500.0f * tijd;
    Vector3 beweging = Vector3.Zero;

    if(input.IsKeyDown(Keys.W)) beweging += _camera.Vooruit;
    if(input.IsKeyDown(Keys.S)) beweging -= _camera.Vooruit;

    // GEBRUIK DE RECHTS VECTOR VAN DE CAMERA
    // De camera class berekent deze al correct met Vector3.Cross(Vooruit, UnitZ)
    if(input.IsKeyDown(Keys.A)) beweging -= _camera.Rechts;
    if(input.IsKeyDown(Keys.D)) beweging += _camera.Rechts;

    if(beweging != Vector3.Zero) {
      return Vector3.Normalize(beweging) * snelheid;
    }
    return Vector3.Zero;
  }

  //internal static void ControleerOpFouten(string herkomst) {
  //  var error = GL.GetError();
  //  while (error != OpenTK.Graphics.OpenGL4.ErrorCode.NoError) {
  //    Console.WriteLine($"In {herkomst} fout: {error}");
  //    error = GL.GetError();
  //  }
  //}
  public static void ZoekStartPositieEnLuchtNaam(List<Entiteit> entiteiten) {
    luchtNaam = "space1"; // Standaard lucht

    foreach(Entiteit entiteit in entiteiten) {
      if (entiteit.Attributen.ContainsKey("sky")) {
        luchtNaam = entiteit.Attributen["sky"];
      }

      if(entiteit.Attributen["classname"].Contains("info_player_start")) {
        BepaalStartPositie(entiteit);
        BepaalDeStartHoek(entiteit);
      }
    } // einde foreach regel
  }

  private static void BepaalStartPositie(Entiteit entiteit) {
      if(entiteit.Attributen["origin"] != null) {
        // We splitsen op de quotes en pakken de laatste waarde
        // Haal de getallen uit de regel: "origin" "128 -256 16"
        string waarden = entiteit.Attributen["origin"];
        string[] assen = waarden.Split(' ');

        if(assen.Length == 3) {
          float x = float.Parse(assen[0]);
          float y = float.Parse(assen[1]);
          float z = float.Parse(assen[2]);
          // Quake -> OpenGL: (X, Z, -Y) doet de camera al goed
          //              return new Vector3(x, z, -y); // OKE DIT IS GOED
          startPos = new Vector3(x, y, z);
          //startPos = new Vector3(832, 2292, -224); //TODO deze regel weghalen na testen
      } // assen
    }
  }
  private static void BepaalDeStartHoek(Entiteit entiteit) {
    string waarde = entiteit.Attributen["angle"];
    deStartHoek = float.Parse(waarde);
    //deStartHoek = 180f;
    // TODO misschien dat hier de starthoek nog omgezet moet worden
  }
}