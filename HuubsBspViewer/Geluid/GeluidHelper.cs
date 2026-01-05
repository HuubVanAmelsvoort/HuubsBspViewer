using OpenTK.Audio.OpenAL;

public class GeluidHelper {
  private Dictionary<string, int> _geluidsArchief = new();
  private ALDevice _device;
  private ALContext _context;
  public GeluidHelper() {
    // Initialiseer de hardware
    _device = ALC.OpenDevice(null);
    _context = ALC.CreateContext(_device, (int[])null);
    ALC.MakeContextCurrent(_context);

    Console.WriteLine("OpenAL hardware is opgestart.");

  }
  public int HaalBuffer(string pad) {
    if(!_geluidsArchief.ContainsKey(pad)) {
      // Hier gebruik je de statische WavLoader
      int id = WavLoader.LoadWav(pad);
      _geluidsArchief[pad] = id;
    }
    return _geluidsArchief[pad];
  }
  public void RuimOp() {
    ALC.MakeContextCurrent(ALContext.Null);
    ALC.DestroyContext(_context);
    ALC.CloseDevice(_device);
  }
  internal void Speel(string bestandPad, DoelClass doel) {
    bool isLopen = (doel.GedragVlaggen & 1) != 0;      // Is vlag 1 gezet?
    bool isGlobaal = (doel.GedragVlaggen & 2048) != 0;    // Is vlag 2048 gezet?

    int _bronId = AL.GenSource();

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
    if(buffer > 0 && !isGlobaal) {
      AL.SourceStop(_bronId);
      AL.Source(_bronId, ALSourcei.Buffer, buffer);
      AL.Source(_bronId, ALSourcef.Gain, 1.0f);
      AL.SourcePlay(_bronId);

      var err = AL.GetError();
      if(err != ALError.NoError) {
        Console.WriteLine($"OpenAL Fout bij {bestandPad}: {err}");
      }
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
}