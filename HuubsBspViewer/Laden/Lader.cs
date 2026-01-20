using OpenTK.Mathematics;
using System.Text;
using System.Text.RegularExpressions;
public class Lader {
  public enum DatablokTiepe {
    Entities = 0,
    Planes = 1,
    Vertices = 2,
    Visibility = 3,
    Nodes = 4,
    TexInfo = 5,
    Faces = 6,
    Lightmaps = 7,
    Leaves = 8,
    LeafFaces = 9,
    LeafBrushes = 10,
    Edges = 11,
    SurfEdges = 12,
    Models = 13,
    Brushes = 14,
    BrushSides = 15,
    Pop = 16,
    Areas = 17,
    AreaPortals = 18
  }
  public Datablok[] DataBlokken;

  public List<Vector3> Hoekpunten = new List<Vector3>();
  public List<BspRand> Randen = new List<BspRand>();
  public List<BspVlak> Vlakken = new List<BspVlak>();
  public List<int> SurfEdges = new List<int>();
  public List<BspPlane> Planes = new List<BspPlane>();
  public List<BspKnooppunt> Knooppunten = new List<BspKnooppunt>();
  public List<BspBlad> Bladeren = new List<BspBlad>();
  public List<BspTexInfo> TextureInfos = new List<BspTexInfo>();
  public List<string> TextureNamen = new List<string>();
  public List<Entiteit> entiteiten = new List<Entiteit>();
  public List<EntiteitAlgemeen> EntiteitenAlgemeen = new List<EntiteitAlgemeen>();
  public List<Model> Modellen = new List<Model>();

  //================= Voor Entiteiten ==========================
  private static readonly Regex entityHeaderRegex =
          new Regex(@"//\s*Entity\s*(\d+)", RegexOptions.Compiled);

  private static readonly Regex keyValueRegex =
      new Regex("\"([^\"]+)\"\\s+\"([^\"]+)\"", RegexOptions.Compiled);

  //================= Entiteiten ==========================

  public void LaadBestand(string pad) {
    using(BinaryReader reader = new BinaryReader(File.Open(pad, FileMode.Open))) {

      // 3. Alle secties vullen met behulp van de Enum
      DataBlokken = LeesKop(reader);

      LeesPlanes(reader, DataBlokken[(int)DatablokTiepe.Planes]);
      LeesHoekpunten(reader, DataBlokken[(int)DatablokTiepe.Vertices]);
      LeesKnooppunten(reader, DataBlokken[(int)DatablokTiepe.Nodes]);
      LeesVlakken(reader, DataBlokken[(int)DatablokTiepe.Faces]);
      LeesBladeren(reader, DataBlokken[(int)DatablokTiepe.Leaves]);
      LeesRanden(reader, DataBlokken[(int)DatablokTiepe.Edges]);
      LeesSurfEdges(reader, DataBlokken[(int)DatablokTiepe.SurfEdges]);
      LeesTextureInfos(reader, DataBlokken[(int)DatablokTiepe.TexInfo]);
      LeesEntiteiten(reader, DataBlokken[(int)DatablokTiepe.Entities]);
      LeesModellen(reader, DataBlokken[(int)DatablokTiepe.Models]);

      //ZoekTriggers();

      VerwerkEntiteiten();
      LeesShader();

      DiverseHelpFuncties.ConsoleLog($"BSP bestand geladen: {pad}", this);
    }
  }

  private Datablok[] LeesKop(BinaryReader reader) {
    // 1. Header lezen
    BspHeader versieBspHeader = new BspHeader();
    versieBspHeader.MoetZijnIBSP = Encoding.ASCII.GetString(reader.ReadBytes(4));
    versieBspHeader.Versie = reader.ReadInt32();

    if(versieBspHeader.MoetZijnIBSP != "IBSP" || versieBspHeader.Versie != 38)
      throw new Exception("Geen geldig Quake 2 BSP bestand.");

    // 2. Datablok (inhoudsopgave) inlezen
    DataBlokken = new Datablok[(int)DatablokTiepe.AreaPortals + 1];
    for(int i = 0; i < (int)DatablokTiepe.AreaPortals + 1; i++) {
      DataBlokken[i] = new Datablok {
        Offset = reader.ReadInt32()
      , Lengte = reader.ReadInt32()
      };
    }
    return DataBlokken;
  }
  /**
   * Leest de shader code uit bestanden in de Shaders map.
   */
  private void LeesShader() {
    try {
      // Zorg ervoor dat "bestand.txt" in dezelfde map staat, of geef het volledige pad op.
      DiverseHelpFuncties.fragmentCode = File.ReadAllText("Shaders/shader.frag.cs");
      DiverseHelpFuncties.vertexCode = File.ReadAllText("Shaders/shader.vert.cs");
      DiverseHelpFuncties.fragmentCodeLucht = File.ReadAllText("Shaders/lucht.frag.cs");
      DiverseHelpFuncties.vertexCodeLucht = File.ReadAllText("Shaders/lucht.vert.cs");
      // Console.WriteLine(BspHelper.fragmentCode);
      // Console.WriteLine(BspHelper.vertexCode);
    } catch(Exception e) {
      Console.WriteLine($"Er is een fout opgetreden: {e.Message}");
    }
  }

  private void LeesPlanes(BinaryReader reader, Datablok lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 20;
    for(int i = 0; i < aantal; i++) {
      Planes.Add(new BspPlane {
        Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
        Afstand = reader.ReadSingle(),
        Type = reader.ReadInt32()
      });
    }
  }
  private void LeesHoekpunten(BinaryReader reader, Datablok lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 12;
    for(int i = 0; i < aantal; i++) {
      Hoekpunten.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
    }
  }
  private void LeesKnooppunten(BinaryReader reader, Datablok lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 28;
    for(int i = 0; i < aantal; i++) {
      Knooppunten.Add(new BspKnooppunt {
        VlakIndex = reader.ReadInt32(),
        Kinderen = new int[] { reader.ReadInt32(), reader.ReadInt32() },
        MinBbox = new short[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() },
        MaxBbox = new short[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() },
        EersteVlakIndex = reader.ReadUInt16(),
        AantalVlakken = reader.ReadUInt16()
      });
    }
  }
  private void LeesBladeren(BinaryReader reader, Datablok lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 28;
    for(int i = 0; i < aantal; i++) {
      Bladeren.Add(new BspBlad {
        Inhoud = reader.ReadInt32(),
        Cluster = reader.ReadInt16(),
        Gebied = reader.ReadInt16(),
        MinBbox = new short[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() },
        MaxBbox = new short[] { reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16() },
        EersteBladVlak = reader.ReadUInt16(),
        AantalBladVlakken = reader.ReadUInt16(),
        EersteBladBrush = reader.ReadUInt16(),
        AantalBladBrushes = reader.ReadUInt16()
      });
    }
  }
  private void LeesVlakken(BinaryReader reader, Datablok lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 20;
    for(int i = 0; i < aantal; i++) {
      Vlakken.Add(new BspVlak {
        PlaneIndex = reader.ReadUInt16(),
        Side = reader.ReadUInt16(),
        EersteEdgeIndex = reader.ReadInt32(),
        AantalEdges = reader.ReadUInt16(),
        TextureInfoIndex = reader.ReadUInt16(),
        LightmapStyles = reader.ReadBytes(4),
        LightmapOffset = reader.ReadInt32()
      });
    }
  }
  private void LeesRanden(BinaryReader reader, Datablok lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 4;
    for(int i = 0; i < aantal; i++) {
      Randen.Add(new BspRand { V1 = reader.ReadUInt16(), V2 = reader.ReadUInt16() });
    }
  }

  private void LeesSurfEdges(BinaryReader reader, Datablok lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 4;
    for(int i = 0; i < aantal; i++) {
      SurfEdges.Add(reader.ReadInt32());
    }
  }
  private void LeesTextureInfos(BinaryReader reader, Datablok lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 76;
    for(int i = 0; i < aantal; i++) {
      BspTexInfo info = new BspTexInfo();
      info.U_As = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
      info.U_Offset = reader.ReadSingle();
      info.V_As = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
      info.V_Offset = reader.ReadSingle();
      info.Flags = reader.ReadInt32();
      info.Waarde = reader.ReadInt32();

      // Lees de naam (32 bytes ASCII)
      byte[] naamBytes = reader.ReadBytes(32);
      info.TextureNaam = Encoding.ASCII.GetString(naamBytes).TrimEnd('\0');

      info.VolgendeTexInfo = reader.ReadInt32();

      TextureNamen.Add(info.TextureNaam);
      TextureInfos.Add(info);
    }
  }
  private void LeesModellen(BinaryReader reader, Datablok modelLump) {
    reader.BaseStream.Seek(modelLump.Offset, SeekOrigin.Begin);
    int aantalModellen = modelLump.Lengte / 48; // Elk model is 48 bytes

    for(int i = 0; i < aantalModellen; i++) {
      Model model = new Model();
      // Quake gebruikt X, Y, Z (float is 4 bytes)
      model.Index = i;
      model.MinimumCoordinaat = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
      model.MaximumCoordinaat = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
      model.Origin = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
      model.HeadNode = reader.ReadInt32();
      model.FirstFace = reader.ReadInt32();
      model.NumFaces = reader.ReadInt32();

      Modellen.Add(model);
    }
  }
  private void LeesEntiteiten(BinaryReader reader, Datablok lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    byte[] bytes = reader.ReadBytes(lump.Lengte);
    string EntiteitenTekst = Encoding.ASCII.GetString(bytes);
    entiteiten = Parse(EntiteitenTekst);
  }
  public static List<Entiteit> Parse(string text) {
    var entities = new List<Entiteit>();
    Entiteit current = null;
    int lastEntityId = -1;

    using var reader = new StringReader(text);
    string line;

    while((line = reader.ReadLine()) != null) {
      line = line.Trim();

      if(string.IsNullOrEmpty(line)) continue;

      // Commentaar afhandeling
      if(line.StartsWith("//")) {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if(parts.Length >= 3 && int.TryParse(parts[2], out int id))
          lastEntityId = id;
        continue;
      }

      if(line == "{") {
        current = new Entiteit { Id = lastEntityId >= 0 ? lastEntityId : null };
        lastEntityId = -1;
        continue;
      }

      if(line == "}") {
        if(current != null)
          entities.Add(current);
        current = null;
        continue;
      }

      // Attributen parsen: "key" "value"
      if(current != null && line.StartsWith("\"")) {
        // We splitsen de regel op basis van de aanhalingstekens
        // De regel ziet eruit als: "classname" "worldspawn"
        var matches = line.Split('"', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => s.Trim())
                          .Where(s => !string.IsNullOrEmpty(s))
                          .ToList();

        if(matches.Count >= 2) {
          string key = matches[0];
          string value = matches[1];
          current.Attributen[key] = value;
        }
      }
    }
    return entities;
  }

  public void VerwerkEntiteiten() {
    foreach(Entiteit entiteit in entiteiten) {
      EntiteitAlgemeen entiteitAlgemeen = new();
      string classNaam = entiteit.Attributen.ContainsKey("classname") ? entiteit.Attributen["classname"] : "";
      
      if (classNaam.Contains("func_button")) {
        var deb=1;
      }
      
      entiteitAlgemeen.ClassNaam = classNaam;

      if(entiteit.Attributen.ContainsKey("sky")) {
        entiteitAlgemeen.Lucht = entiteit.Attributen["sky"];
      }
      if(entiteit.Attributen.ContainsKey("sounds")) {
        entiteitAlgemeen.Geluiden = entiteit.Attributen["sounds"];
      }
      if(entiteit.Attributen.ContainsKey("target")) {
        entiteitAlgemeen.Doel = entiteit.Attributen["target"];
      }
      if(entiteit.Attributen.ContainsKey("targetname")) {
        entiteitAlgemeen.DoelNaam = entiteit.Attributen["targetname"];
      }
      if(entiteit.Attributen.ContainsKey("noise")) {
        entiteitAlgemeen.GeluidPad = entiteit.Attributen["noise"];
      }
      if(entiteit.Attributen.ContainsKey("looped")) {
        entiteitAlgemeen.IsLopen = entiteit.Attributen["looped"] == "1";
      }
      if(entiteit.Attributen.ContainsKey("message")) {
        entiteitAlgemeen.Boodschap = entiteit.Attributen["message"];
      }
      if(entiteit.Attributen.ContainsKey("origin")) {
        string[] tekst = entiteit.Attributen["origin"].Split();
        entiteitAlgemeen.Positie = new Vector3(float.Parse(tekst[0]), float.Parse(tekst[1]), float.Parse(tekst[2]));
      }
      if(entiteit.Attributen.ContainsKey("nextmap")) {
        entiteitAlgemeen.VolgendeMap = entiteit.Attributen["nextmap"];
      }
      if(entiteit.Attributen.ContainsKey("angle")) {
        entiteitAlgemeen.DeHoek = entiteit.Attributen["angle"];
      }
      if(entiteit.Attributen.ContainsKey("wait")) {
        entiteitAlgemeen.Wachten = entiteit.Attributen["wait"];
      }
      if(entiteit.Attributen.ContainsKey("attenuation")) {
        entiteitAlgemeen.Verzwakking = entiteit.Attributen["attenuation"];
      }

      //entiteitAlgemeen.MinimumCoordinaat = new();
      //entiteitAlgemeen.MaximumCoordinaat = new();

      if(entiteit.Attributen.ContainsKey("model") && entiteit.Attributen["model"].StartsWith("*")) {
        string modelSleutel = entiteit.Attributen["model"]; // bijv. "*1"
        int modelIndex = int.Parse(modelSleutel.Substring(1));
        entiteitAlgemeen.ModelIndex = modelIndex;
        if(modelIndex < Modellen.Count) {
          // Haal de mins/maxs uit de Models Lump van de BSP
          Model model = Modellen[modelIndex];
          entiteitAlgemeen.MinimumCoordinaat = model.MinimumCoordinaat;
          entiteitAlgemeen.MaximumCoordinaat = model.MaximumCoordinaat;
        } else {
          Console.WriteLine($"Model index {modelIndex} ligt buiten het bereik van beschikbare modellen.");
        }
      }

      if(entiteit.Attributen.ContainsKey("delay")) {
        if(entiteit.Attributen["delay"].StartsWith(".")) {
          entiteitAlgemeen.Vertraging = float.Parse("0" + entiteit.Attributen["delay"]);
        } else {
          entiteitAlgemeen.Vertraging = float.Parse(entiteit.Attributen["delay"]);
        }
      }
      //===================================================================================================  
      entiteitAlgemeen.GedragVlaggen = 0;
      if(entiteit.Attributen.ContainsKey("spawnflags")) {
        entiteitAlgemeen.GedragVlaggenTekst = entiteit.Attributen["spawnflags"];
        bool gelukt = int.TryParse(entiteitAlgemeen.GedragVlaggenTekst, out entiteitAlgemeen.GedragVlaggen);
        if(!gelukt) {
          Console.WriteLine($"Kan spawnflags niet parsen: {entiteitAlgemeen.GedragVlaggenTekst}");
        }
      }
      if(entiteit.Attributen.ContainsKey("dmg")) {
         bool gelukt2 = int.TryParse(entiteit.Attributen["dmg"], out entiteitAlgemeen.Schade);
      }
      if(entiteitAlgemeen.ClassNaam.StartsWith("trigger_")) {
        entiteitAlgemeen.IsTriggerBox = true;
        // Hier voeg je de logica toe om de 'mins' en 'maxs' van de trigger te bepalen
      }
      if(entiteitAlgemeen.ClassNaam == "target_speaker"
      && string.IsNullOrEmpty(entiteitAlgemeen.DoelNaam) && (entiteitAlgemeen.GedragVlaggen & 1) != 0) {
        entiteitAlgemeen.IsLopen = true;
        entiteitAlgemeen.IsActief = true; // Deze mag direct geluid maken
      }

      //===================================================================================================
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
          EntiteitenAlgemeen.Add(entiteitAlgemeen);
          break;
        //==========================================================================
        case "target_changelevel":
        case "target_crosslevel_target":
        case "target_explosion":
        case "target_goal":
        case "target_help":
        case "target_secret":
        case "target_speaker":
        case "target_character":
          
          EntiteitenAlgemeen.Add(entiteitAlgemeen);
          break;
        //==========================================================================
        case "trigger_always":
        case "trigger_multiple":
        case "trigger_once":
        case "trigger_relay":
        case "trigger_push":
          EntiteitenAlgemeen.Add(entiteitAlgemeen);
          break;
        //==========================================================================
        case "weapon_chaingun":
        case "weapon_grenadelauncher":
        case "weapon_machinegun":
        case "weapon_railgun":
        case "weapon_rocketlauncher":
        case "weapon_shotgun":
        case "weapon_supershotgun":
        case "worldspawn":
          EntiteitenAlgemeen.Add(entiteitAlgemeen);
          break;
        default:
          Console.WriteLine($"Onbekende entiteit: {classNaam}");
          break;
      }
    } // eind foreach
  }
}