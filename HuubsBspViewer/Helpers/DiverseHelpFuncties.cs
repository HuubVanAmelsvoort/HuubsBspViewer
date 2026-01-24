using StbImageSharp;
using OpenTK.Audio.OpenAL;
using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;

public static class DiverseHelpFuncties {

  public static string basispadQuake2 = @"D:/werkdirectory/SpelData/baseq2uitgepakt/baseq2";

  private static Vector2 _vorigeMuisPositie;

  //public static Botsing _botsing; // Komt uit BspVenster
  //========================= Shader code ==============================
  public static Camera _camera; // Komt uit BspVenster
  public static Vector3 startPos = new Vector3(); // -1768 1536 128 uit entiteiten of 128 -320 32
  public static Vector3 beweging = Vector3.Zero;
  //========================= Shader code ==============================
  internal static string vertexCode; // shader code wordt ingeladen vanuit bestand in BspLader
  internal static string fragmentCode;

  internal static string vertexCodeLucht;
  internal static string fragmentCodeLucht;
  //========================= Einde Shader code ==============================

  internal static int LUCHT = 4;
  internal static string luchtNaam;
  internal static float deStartHoek = 270;

  //========================= Toetsen status ==============================
  public static bool wIngedrukt;
  public static bool sIngedrukt;
  public static bool dIngedrukt;
  public static bool aIngedrukt;
  //========================= Einde Toetsen status ==============================
  public static Dictionary<string, (int breedte, int hoogte)> textureAfmetingen = new();

  internal static void ConsoleLog(string tekst, Lader lader) {
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
    // 2. Animaties ophalen
    //var animaties = model.LogicalAnimations.FirstOrDefault();
    foreach(var animatie in lader.HetModel.LogicalAnimations) {
      var naam = animatie.Name;
      Console.WriteLine($"Animatie gevonden: {naam}");
    }
    // Gebruik animation.Animatible om de transformaties per frame te berekenen


  }
  // De callback methode:
  public static void HuubDebugCallback(DebugSource source, DebugType type, int id
  , DebugSeverity severity, int length, IntPtr message, IntPtr userParam) {
    if(severity == DebugSeverity.DebugSeverityNotification) return; // negeer notificaties

    string msg = Marshal.PtrToStringAnsi(message, length);
    Console.WriteLine($"[GL DEBUG] {msg}");
  }
  public static List<Vector3> HaalVlakPuntenOp(BspVlak vlak, Lader lader) {
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
  internal static void BerekenBeweging(float tijd) {
    float snelheid = 200.0f * tijd;

    if(wIngedrukt) beweging += _camera.Vooruit;
    if(sIngedrukt) beweging -= _camera.Vooruit;
    if(dIngedrukt) beweging += _camera.Rechts;
    if(aIngedrukt) beweging -= _camera.Rechts;

    // GEBRUIK DE RECHTS VECTOR VAN DE CAMERA
    // De camera class berekent deze al correct met Vector3.Cross(Vooruit, UnitZ)
    if(beweging != Vector3.Zero) {
      beweging = Vector3.Normalize(beweging) * snelheid;
    }
  }
  public static void ZoekStartPositieEnLuchtNaam(List<Entiteit> entiteiten) {
    luchtNaam = "space1"; // Standaard lucht

    foreach(Entiteit entiteit in entiteiten) {
      if(entiteit.Attributen.ContainsKey("sky")) {
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
    deStartHoek = float.Parse(waarde) + 90f;
    //deStartHoek = 180f;
    // TODO misschien dat hier de starthoek nog omgezet moet worden
  }
  /**
   * Bepaal kleur voor lijnblokje op basis van class naam
   * Voor de blokjes rond triggers en andere entiteiten
   */
  public static Vector3 KleurVanClassNaam(string classNaam) {
    Vector3 kleurLijn;
    switch(classNaam) {
      case "ammo_bullets":
      case "ammo_grenades":
      case "ammo_rockets":
      case "ammo_shells":
      case "ammo_slugs":
      case "func_areaportal":
      case "func_button":
      case "func_door":
      case "func_door_rotating":
      case "func_explosive":
      case "func_group":
      case "func_plat":
      case "func_rotating":
      case "func_timer":
      case "func_wall":
      case "info_player_coop":
      case "info_player_deathmatch":
      case "info_player_intermission":
      case "info_player_start":
      case "item_armor_combat":
      case "item_armor_jacket":
      case "item_armor_shard":
      case "item_health":
      case "item_health_large":
      case "item_health_small":
      case "item_invulnerability":
      case "item_quad":
      case "light":
      case "misc_deadsoldier":
      case "misc_explobox":
      case "misc_gib_head":
      case "monster_flyer":
      case "monster_gunner":
      case "monster_infantry":
      case "monster_soldier_light":
      case "monster_soldier_ss":
      case "path_corner":
      case "point_combat":
      case "target_changelevel":
      case "target_crosslevel_target":
      case "target_explosion":
      case "target_goal":
      case "target_help":
      case "target_secret":
      case "weapon_chaingun":
      case "weapon_grenadelauncher":
      case "weapon_machinegun":
      case "weapon_railgun":
      case "weapon_rocketlauncher":
      case "weapon_shotgun":
      case "weapon_supershotgun":
      case "worldspawn":
        kleurLijn = new Vector3(0.5f, 0.5f, 0.5f); // Grijs
        break;
      case "target_speaker":
        kleurLijn = new Vector3(1, 0, 0); // Rood
        break;
      case "trigger_always":
        kleurLijn = new Vector3(0, 1, 0); // Groen
        break;
      case "trigger_multiple":
        kleurLijn = new Vector3(0, 0, 1); // Blauw
        break;
      case "trigger_once":
        kleurLijn = new Vector3(1, 1, 0); // Geel
        break;
      case "trigger_relay":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "target_character":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "trigger_push":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "ammo_cells":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "func_clock":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "func_killbox":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "func_object":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "func_train":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "target_string":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "target_laser":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "monster_boss2":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "target_actor":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "target_earthquake":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      case "weapon_bfg":
        kleurLijn = new Vector3(1, 0, 1); // Magenta
        break;
      default:
        kleurLijn = new Vector3(0, 1, 1); // Cyaan
        Console.WriteLine($"Default in trigger: {classNaam}");
        break;
    }
    return kleurLijn;
  }
  /**
   * Zowel een AL als GL fout afvangen.
   */
  internal static void ControleerGLFout(string herkomst) {
    ALError fout = AL.GetError();
    if(fout != ALError.NoError) {
      Console.WriteLine($"OpenAL Fout bij herkomst: {fout}");
    }
    //GLErrorCode glFout = GL.GetError();

    OpenTK.Graphics.OpenGL.ErrorCode glFout = GL.GetError();
    if(glFout != OpenTK.Graphics.OpenGL.ErrorCode.NoError) {
      Console.WriteLine($"glFout bij herkomst: {glFout}");
    }
  }
  public static void LadenAfmetingen(Lader lader) {
    foreach(var naam in lader.TextureNamen) {
      string pad = $"{basispadQuake2}/textures/{naam}.png"; // Pas aan naar jouw pad
      if(File.Exists(pad)) {
        using(Stream stream = File.OpenRead(pad)) {
          // Dit laadt alleen de metadata, erg snel!
          ImageResult info = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
          textureAfmetingen[naam] = (info.Width, info.Height);
        }
      } else {
        // Fallback als het plaatje ontbreekt (voorkom delen door nul!)
        textureAfmetingen[naam] = (128, 128);
      }
    }
  }
}