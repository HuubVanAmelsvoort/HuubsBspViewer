using OpenTK.Audio.OpenAL;
public class GeluidHelper {
  private Dictionary<string, int> _geluidsArchief = new();
  private Dictionary<EntiteitAlgemeen, int> _actieveBronnen = new();

  private ALDevice _device;
  private ALContext _context;
  public GeluidHelper() {
    // Initialiseer de hardware
    _device = ALC.OpenDevice(null);
    _context = ALC.CreateContext(_device, (int[])null);
    ALC.MakeContextCurrent(_context);
 
    DiverseHelpFuncties.SchrijfRegel("OpenAL hardware is opgestart.");

  }
  public int HaalBuffer(string pad) {
    if(!_geluidsArchief.ContainsKey(pad)) {
      // Hier gebruik je de statische WavLoader
      int id = WavLader.LaadWav(pad);
      _geluidsArchief[pad] = id;
    }
    return _geluidsArchief[pad];
  }
  public void RuimOp() {
    ALC.MakeContextCurrent(ALContext.Null);
    ALC.DestroyContext(_context);
    ALC.CloseDevice(_device);
  }
  internal void SpeelGeluid(string bestandPad, EntiteitAlgemeen entiteitAlgemeenDoel) {
    // 1. Flush oude fouten
    AL.GetError();

    // 2. Bepaal gedrag op basis van vlaggen
    bool isLopen = (entiteitAlgemeenDoel.ClassNaam == "target_speaker") && ((entiteitAlgemeenDoel.GedragVlaggen & 1) != 0);
    bool isGlobaal = (entiteitAlgemeenDoel.GedragVlaggen & 2048) != 0;

    string deelPad = bestandPad.Replace(DiverseHelpFuncties.basispadQuake2, "");

    string debugTekst = $"Geluid speel: {deelPad} class: {entiteitAlgemeenDoel.ClassNaam} Positie: {entiteitAlgemeenDoel.Positie} ";
    debugTekst = $"{debugTekst} isLopen: {isLopen} isGlobaal: {isGlobaal} IsActief: {entiteitAlgemeenDoel.IsActief}";

    // 3. Haal de buffer (doe dit VOORDAT je een bron claimt)
    DiverseHelpFuncties.SchrijfRegel(debugTekst);

    int buffer = HaalBuffer(bestandPad);
    if(buffer <= 0) {
      DiverseHelpFuncties.SchrijfRegel($"FOUT: Buffer ongeldig voor {bestandPad}.");
      return;
    }

    // 4. Beheer de Bron (Source)
    int _bronId;
    // Check of dit object al een bron heeft, zo niet: maak aan
    if(!_actieveBronnen.TryGetValue(entiteitAlgemeenDoel, out _bronId)) {
      _bronId = AL.GenSource();
      _actieveBronnen[entiteitAlgemeenDoel] = _bronId;
    } else {
      // Als hij al bestond, stop hem dan even voor we hem herstarten
      AL.SourceStop(_bronId);
    }

    // 5. Configureer de bron
    AL.Source(_bronId, ALSourcef.ReferenceDistance, 100.0f);
    AL.Source(_bronId, ALSourcef.MaxDistance, 2000.0f);
    AL.Source(_bronId, ALSourcef.RolloffFactor, 1.0f);
    AL.Source(_bronId, ALSourceb.Looping, isLopen);
    AL.Source(_bronId, ALSourcef.Gain, 0.5f);

    if(isGlobaal) {
      AL.Source(_bronId, ALSourceb.SourceRelative, true);
      AL.Source(_bronId, ALSource3f.Position, 0, 0, 0);
    } else {
      AL.Source(_bronId, ALSourceb.SourceRelative, false);
      AL.Source(_bronId, ALSource3f.Position, entiteitAlgemeenDoel.Positie.X, entiteitAlgemeenDoel.Positie.Y, entiteitAlgemeenDoel.Positie.Z);
    }

    // 6. Koppel buffer en speel af
    AL.Source(_bronId, ALSourcei.Buffer, buffer);
    AL.SourcePlay(_bronId);

    // 7. Laatste foutcontrole
    DiverseHelpFuncties.ControleerGLFout("SpeelGeluid");
  }
  internal void ZetVolume(EntiteitAlgemeen entiteit, float afstand, float maxAfstand) {
    // Bereken het volume: 1.0 (dichtbij) naar 0.0 (ver weg)
    float volume = 1.0f - Math.Clamp(afstand / maxAfstand, 0.0f, 1.0f);

    // Als je OpenAL gebruikt, heb je een SourceId nodig die bij dit doel hoort
    // Zoek de actieve speaker (Source ID) op die bij deze specifieke entiteit hoort
    if(_actieveBronnen.TryGetValue(entiteit, out int sourceId)) {
      //if (volume > 0) DiverseHelpFuncties.SchrijfRegel($"volume: {volume}");
      AL.Source(sourceId, ALSourcef.Gain, volume);
    }
  }
}