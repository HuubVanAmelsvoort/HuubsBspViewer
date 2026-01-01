using OpenTK.Audio.OpenAL;
using System.Runtime.InteropServices;
public static class WavLoader {
  public static int LoadWav(string filename) {
    if(!File.Exists(filename)) {
      return -1;
    }

    using(BinaryReader reader = new BinaryReader(File.OpenRead(filename))) {
      // RIFF header
      string signature = new string(reader.ReadChars(4));
      if(signature != "RIFF") throw new NotSupportedException("Geen RIFF bestand");

      reader.ReadInt32(); // File size
      string format = new string(reader.ReadChars(4)); // WAVE
      string chunkID = new string(reader.ReadChars(4)); // fmt 
      reader.ReadInt32(); // Chunk size
      short audioFormat = reader.ReadInt16(); // 1 = PCM
      short channels = reader.ReadInt16();
      int sampleRate = reader.ReadInt32();
      reader.ReadInt32(); // Byte rate
      reader.ReadInt16(); // Block align
      short bitsPerSample = reader.ReadInt16();

      // Zoek naar de 'data' chunk
      while(new string(reader.ReadChars(4)) != "data") {
        int size = reader.ReadInt32();
        reader.BaseStream.Seek(size, SeekOrigin.Current);
      }

      int dataSize = reader.ReadInt32();
      byte[] data = reader.ReadBytes(dataSize);

      // Bepaal OpenAL formaat
      ALFormat alFormat = GetSoundFormat(channels, bitsPerSample);

      int buffer = AL.GenBuffer();

      GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
      try {
        nint pointer = handle.AddrOfPinnedObject();
        AL.BufferData(buffer, alFormat, pointer, dataSize, sampleRate);
      } finally {
        handle.Free();
      }

      Console.WriteLine($"Laden: {filename} - Kanalen: {channels}, Bits: {bitsPerSample}, Rate: {sampleRate}");

      return buffer;
    }
  }

  private static ALFormat GetSoundFormat(int channels, int bits) {
    switch(channels) {
      case 1: 
      return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
      case 2: 
      return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
      default: throw new NotSupportedException("Kanaalformaat niet ondersteund");
    }
  }
}