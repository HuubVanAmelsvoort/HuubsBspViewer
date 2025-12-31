using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

public class GeluidsBron {
  private int _source;

  public GeluidsBron(Vector3 positie) {
    _source = AL.GenSource();
    AL.Source(_source, ALSourcef.Gain, 1.0f);
    AL.Source(_source, ALSource3f.Position, positie.X, positie.Y, positie.Z);
    // Voor 3D geluid:
    AL.Source(_source, ALSourceb.SourceRelative, false);
  }

  public void Speel(int buffer) {
    AL.Source(_source, ALSourcei.Buffer, buffer);
    AL.SourcePlay(_source);
  }
}