using OpenTK.Audio.OpenAL;

public class AudioManager {
  private ALDevice _device;
  private ALContext _context;

  public AudioManager() {
    _device = ALC.OpenDevice(null); // Gebruik standaard audio device
    _context = ALC.CreateContext(_device, (int[])null);
    ALC.MakeContextCurrent(_context);
  }

  public int LaadGeluid(string path) {
    // Hier moet je een .wav inladen en naar een AL-buffer sturen
    // Voor nu een simpele placeholder
    int buffer = AL.GenBuffer();
    // LaadWavData(path, buffer); 
    return buffer;
  }
}