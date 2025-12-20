using OpenTK.Mathematics;
using System.Text;
public class BspLader {
  public List<Vector3> Hoekpunten = new List<Vector3>();
  public Lump[] Lumps;
  public List<BspRand> Randen = new List<BspRand>();
  public List<BspVlak> Vlakken = new List<BspVlak>();
  public List<int> SurfEdges = new List<int>();
  public List<BspPlane> Planes = new List<BspPlane>();
  public List<BspKnooppunt> Knooppunten = new List<BspKnooppunt>();
  public List<BspBlad> Bladeren = new List<BspBlad>();

  // Voeg dit toe aan de variabelen bovenin BspLader.cs
  public string EntiteitenTekst = "";
  public void LaadBestand(string pad) {
    using (BinaryReader reader = new BinaryReader(File.Open(pad, FileMode.Open))) {
      // 1. Header lezen
      string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
      int versie = reader.ReadInt32();
      //BspHeader.Magic = magic;

      if (magic != "IBSP" || versie != 38)
        throw new Exception("Geen geldig Quake 2 BSP bestand.");

      // 2. Lumps (inhoudsopgave) inlezen
      Lumps = new Lump[19];
      for (int i = 0; i < 19; i++) {
        Lumps[i] = new Lump { Offset = reader.ReadInt32(), Lengte = reader.ReadInt32() };
      }

      // 3. Alle secties vullen met behulp van de Enum
      LeesPlanes(reader, Lumps[(int)LumpType.Planes]);
      LeesHoekpunten(reader, Lumps[(int)LumpType.Vertices]);
      LeesKnooppunten(reader, Lumps[(int)LumpType.Nodes]);
      LeesVlakken(reader, Lumps[(int)LumpType.Faces]);
      LeesBladeren(reader, Lumps[(int)LumpType.Leaves]);
      LeesRanden(reader, Lumps[(int)LumpType.Edges]);
      LeesSurfEdges(reader, Lumps[(int)LumpType.SurfEdges]);
      LeesEntiteiten(reader, Lumps[(int)LumpType.Entities]);
    }
  }
  private void LeesPlanes(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 20;
    for (int i = 0; i < aantal; i++) {
      Planes.Add(new BspPlane {
        Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
        Afstand = reader.ReadSingle(),
        Type = reader.ReadInt32()
      });
    }
  }
  private void LeesHoekpunten(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 12;
    for (int i = 0; i < aantal; i++) {
      Hoekpunten.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
    }
  }
  private void LeesKnooppunten(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 28;
    for (int i = 0; i < aantal; i++) {
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
  private void LeesBladeren(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 28;
    for (int i = 0; i < aantal; i++) {
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
  private void LeesVlakken(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 20;
    for (int i = 0; i < aantal; i++) {
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
  private void LeesRanden(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 4;
    for (int i = 0; i < aantal; i++) {
      Randen.Add(new BspRand { V1 = reader.ReadUInt16(), V2 = reader.ReadUInt16() });
    }
  }

  private void LeesSurfEdges(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 4;
    for (int i = 0; i < aantal; i++) {
      SurfEdges.Add(reader.ReadInt32());
    }
  }
  private void LeesEntiteiten(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    byte[] bytes = reader.ReadBytes(lump.Lengte);
    EntiteitenTekst = Encoding.ASCII.GetString(bytes);
  }
}