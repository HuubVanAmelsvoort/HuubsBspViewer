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
}