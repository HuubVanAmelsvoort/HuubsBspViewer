using OpenTK.Mathematics;
using System.Runtime.InteropServices;

public struct BspHeader {
  public string MoetZijnIBSP;      // Moet "IBSP" zijn
  public int Versie;    // Moet 38 zijn voor Quake 2
  public Datablok[] Lumps;   // 19 lumps
}
public struct BspPlane {
  public Vector3 Normal;  // De richting waar het vlak naartoe wijst
  public float Afstand;   // Afstand van de oorsprong (0,0,0)
  public int Type;        // As-georiënteerd type
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
public struct BspKnooppunt { // BspNode 
  public int VlakIndex;           // Index naar het scheidingsvlak (Plane)
  public int[] Kinderen;          // Kinderen[0] is de voorkant, Kinderen[1] de achterkant
  public short[] MinBbox;         // Minimale hoek van de begrenzingsdoos (Bounding Box)
  public short[] MaxBbox;         // Maximale hoek van de begrenzingsdoos
  public ushort EersteVlakIndex;  // Index naar het eerste vlak (Face) in dit knooppunt
  public ushort AantalVlakken;    // Hoeveel vlakken bij dit knooppunt horen
}

// Een Edge (Rand) verbindt twee hoekpunten
public struct BspRand {
  public ushort V1; // Index naar hoekpunt 1
  public ushort V2; // Index naar hoekpunt 2
}

public struct Datablok {
  public int Offset; // Waar in het bestand begint deze data
  public int Lengte; // Hoe groot is deze data-sectie
}
public struct BspBlad {
  public int Inhoud;          // Wat zit hier? (0 = leeg, 1 = solide, water, etc.)
  public short Cluster;       // Cluster-index voor zichtbaarheid (PVS)
  public short Gebied;        // Gebied-index (voor portals en zones)
  public short[] MinBbox;     // Minimale hoek van de begrenzingsdoos
  public short[] MaxBbox;     // Maximale hoek van de begrenzingsdoos
  public ushort EersteBladVlak;  // Index naar de lijst met vlakken van dit blad
  public ushort AantalBladVlakken;
  public ushort EersteBladBrush; // Gebruikt voor fysieke botsingen
  public ushort AantalBladBrushes;
}
public struct BspTexInfo {
  public Vector3 U_As;      // Richting van de X-as van de textuur
  public float U_Offset;    // Verschuiving op de X-as
  public Vector3 V_As;      // Richting van de Y-as van de textuur
  public float V_Offset;    // Verschuiving op de Y-as
  public int Flags;         // Eigenschappen (bijv. glimmend of doorzichtig)
  public int Waarde;
  public string TextureNaam; // De naam van het bestand (bijv. "walls/brick01")
  public int VolgendeTexInfo;
}
