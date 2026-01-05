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
    //AL.DistanceModel(ALDistanceModel.InverseDistanceClamped);

    Console.WriteLine("OpenAL hardware is opgestart.");

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
  internal void Speel(string bestandPad, EntiteitAlgemeen doel) {
    bool isLopen = (doel.GedragVlaggen & 1) != 0;      // Is vlag 1 gezet?
    bool isGlobaal = (doel.GedragVlaggen & 2048) != 0;    // Is vlag 2048 gezet?

    int _bronId = AL.GenSource();

    AL.Source(_bronId, ALSourcef.ReferenceDistance, 100.0f); // Tot 100 units op vol volume
    AL.Source(_bronId, ALSourcef.MaxDistance, 2000.0f);     // Boven de 2000 units stilte
    AL.Source(_bronId, ALSourcef.RolloffFactor, 1.0f);      // Hoe snel het geluid afneemt

    if(isGlobaal) {
      // In OpenAL: zet 'Relative' op true en positie op (0,0,0)
      // Dit zorgt ervoor dat de positie van de camera niet uitmaakt.
      AL.Source(_bronId, ALSourceb.SourceRelative, true);
      AL.Source(_bronId, ALSource3f.Position, 0, 0, 0);
    } else {
      // Voor normale 3D speakers (zoals je gele blokjes):
      AL.Source(_bronId, ALSourceb.SourceRelative, false);
      AL.Source(_bronId, ALSource3f.Position, doel.Positie.X, doel.Positie.Y, doel.Positie.Z);
      //AL.Source(_bronId, ALSource3f.Position, 0, 0, 0);
    }
    //AL.Source(_bronId, ALSourceb.SourceRelative, true);
    AL.Source(_bronId, ALSourcef.Gain, 1.0f);

    if(isLopen) {
      AL.Source(_bronId, ALSourceb.Looping, true);
    }

    int buffer = HaalBuffer(bestandPad);
    if(buffer > 0) {
      AL.SourceStop(_bronId);
      AL.Source(_bronId, ALSourcei.Buffer, buffer);
      AL.Source(_bronId, ALSourcef.Gain, 1.0f);
      AL.SourcePlay(_bronId);

      var err = AL.GetError();
      if(err != ALError.NoError) {
        Console.WriteLine($"OpenAL Fout bij {bestandPad}: {err}");
      }
      _actieveBronnen[doel] = _bronId;
    }
    // Controleer of de buffer geldig is
    if(buffer <= 0) {
      Console.WriteLine($"FOUT: Buffer ongeldig voor {bestandPad}. Is het bestand een 8/16-bit WAV?");
      return;
    }
    var error = AL.GetError();
    if(error != ALError.NoError) {
      Console.WriteLine($"OpenAL Fout bij {bestandPad}: {AL.GetErrorString(error)}");
    }
  }

  internal void ZetVolume(EntiteitAlgemeen entiteit, float afstand, float maxAfstand) {
    // Bereken het volume: 1.0 (dichtbij) naar 0.0 (ver weg)
    float volume = 1.0f - Math.Clamp(afstand / maxAfstand, 0.0f, 1.0f);

    // Als je OpenAL gebruikt, heb je een SourceId nodig die bij dit doel hoort
    // Zoek de actieve speaker (Source ID) op die bij deze specifieke entiteit hoort
    if(_actieveBronnen.TryGetValue(entiteit, out int sourceId)) {
      AL.Source(sourceId, ALSourcef.Gain, volume);
    }
  }
}