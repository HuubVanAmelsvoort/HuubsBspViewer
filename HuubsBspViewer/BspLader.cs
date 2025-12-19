using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class BspLader {
  public List<Vector3> Hoekpunten = new List<Vector3>();
  public List<BspRand> Randen = new List<BspRand>();
  public List<BspVlak> Vlakken = new List<BspVlak>();
  public List<int> SurfEdges = new List<int>();

  public void LaadBestand(string pad) {
    using (BinaryReader reader = new BinaryReader(File.Open(pad, FileMode.Open))) {
      // 1. Lees de Magic Number (moet "IBSP" zijn)
      string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
      int versie = reader.ReadInt32();

      if (magic != "IBSP" || versie != 38) {
        throw new Exception("Dit is geen geldig Quake 2 BSP bestand!");
      }

      // 2. Lees de 19 Lumps (de inhoudsopgave)
      Lump[] lumps = new Lump[19];
      for (int i = 0; i < 19; i++) {
        lumps[i] = new Lump {
          Offset = reader.ReadInt32(),
          Lengte = reader.ReadInt32()
        };
      }

      // 3. Data inlezen met behulp van de offsets
      LeesHoekpunten(reader, lumps[(int)LumpType.Vertices]);
      LeesRanden(reader, lumps[(int)LumpType.Edges]);
      LeesSurfEdges(reader, lumps[(int)LumpType.SurfEdges]);
      LeesVlakken(reader, lumps[(int)LumpType.Faces]);
    }
  }

  private void LeesHoekpunten(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 12; // Elke Vector3 is 3 * 4 bytes float
    for (int i = 0; i < aantal; i++) {
      // Lees de floats en maak direct een OpenTK Vector3 aan
      float x = reader.ReadSingle();
      float y = reader.ReadSingle();
      float z = reader.ReadSingle();
      Hoekpunten.Add(new Vector3(x, y, z));
    }
  }

  private void LeesRanden(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 4; // Elke Edge is 2 * 2 bytes ushort
    for (int i = 0; i < aantal; i++) {
      Randen.Add(new BspRand {
        V1 = reader.ReadUInt16(),
        V2 = reader.ReadUInt16()
      });
    }
  }

  private void LeesSurfEdges(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 4; // Elke SurfEdge is een 4-byte integer
    for (int i = 0; i < aantal; i++) {
      SurfEdges.Add(reader.ReadInt32());
    }
  }

  private void LeesVlakken(BinaryReader reader, Lump lump) {
    reader.BaseStream.Seek(lump.Offset, SeekOrigin.Begin);
    int aantal = lump.Lengte / 20; // Een Quake 2 Face is exact 20 bytes
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
}