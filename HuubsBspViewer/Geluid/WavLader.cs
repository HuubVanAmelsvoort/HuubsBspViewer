using OpenTK.Audio.OpenAL;
using System.Runtime.InteropServices;
public static class WavLader {
  public static int LaadWav(string bestandsNaam) {
    if(!File.Exists(bestandsNaam)) {
      Console.WriteLine($"In LaadWav bestandsNaam: {bestandsNaam} bestaat niet!!!!");
      return -1;
    }
    Console.WriteLine($"In LaadWav bestandsNaam: {bestandsNaam}");

    using(BinaryReader reader = new BinaryReader(File.OpenRead(bestandsNaam))) {
      // RIFF header
      string signature = new string(reader.ReadChars(4));
      if(signature != "RIFF") throw new NotSupportedException("Geen RIFF bestand");

      reader.ReadInt32(); // File size
      string format = new string(reader.ReadChars(4)); // WAVE
      string chunkID = new string(reader.ReadChars(4)); // fmt 
      reader.ReadInt32(); // Chunk size
      short audioFormat = reader.ReadInt16(); // 1 = PCM
      if(audioFormat != 1) { // 1 = PCM, alles anders is gecomprimeerd
        Console.WriteLine($"FOUT: {bestandsNaam} is geen PCM maar formaat {audioFormat}");
      }
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

      Console.WriteLine($"Laden: {bestandsNaam} - Kanalen: {channels}, Bits: {bitsPerSample}, Rate: {sampleRate}");

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