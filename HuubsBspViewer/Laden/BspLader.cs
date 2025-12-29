using OpenTK.Mathematics;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
public class BspLader {
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
      DiverseHelpFuncties.fragmentCode = File.ReadAllText("Shaders/shader.frag");
      DiverseHelpFuncties.vertexCode = File.ReadAllText("Shaders/shader.vert");
      DiverseHelpFuncties.fragmentCodeLucht = File.ReadAllText("Shaders/lucht.frag");
      DiverseHelpFuncties.vertexCodeLucht = File.ReadAllText("Shaders/lucht.vert");
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

      // Optional entity comment
      if(line.StartsWith("// Entity")) {
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

      if(current != null && line.StartsWith("\"")) {
        int split = line.IndexOf("\" ", StringComparison.Ordinal);
        if(split > 0) {
          string key = line.Substring(1, split - 1);
          string value = line.Substring(split + 3, line.Length - split - 4);
          current.Attributen[key] = value;
        }
      }
    }
    return entities;
  }
}