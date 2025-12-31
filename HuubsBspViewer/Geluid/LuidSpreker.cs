using OpenTK.Mathematics;

public class LuidSpreker {
  public string TargetName { get; set; }
  public string NoisePath { get; set; } // bijv. "world/amb1.wav"
  public Vector3 Positie { get; set; }
  public bool IsLooping { get; set; }

  public void SpeelAf() {
    // Hier komt je OpenAL/Audio code om NoisePath te laden en te spelen
    Console.WriteLine($"AFSPELEN: {NoisePath} op positie {Positie}");
  }
}