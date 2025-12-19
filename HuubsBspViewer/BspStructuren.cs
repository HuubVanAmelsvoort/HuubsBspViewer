using System.Runtime.InteropServices;

public enum LumpType {
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

// Een Face (Vlak) beschrijft een oppervlak in de wereld
public struct BspVlak {
  public ushort PlaneIndex;      // Welke plane wordt gebruikt
  public int Side;               // Kant van de plane
  public int EersteEdgeIndex;    // Index in de SurfEdge lump
  public ushort AantalEdges;     // Hoeveel randen heeft dit vlak
  public ushort TextureInfoIndex;
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
  public byte[] LightmapStyles;  // 4 bytes
  public int LightmapOffset;
}

// Een Edge (Rand) verbindt twee hoekpunten
public struct BspRand {
  public ushort V1; // Index naar hoekpunt 1
  public ushort V2; // Index naar hoekpunt 2
}

public struct Lump {
  public int Offset; // Waar in het bestand begint deze data
  public int Lengte; // Hoe groot is deze data-sectie
}
