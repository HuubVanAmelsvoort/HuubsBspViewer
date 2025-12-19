using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
/**
 * Met behulp van Gemini AI (https://www.gemini.com/) gegenereerde code.
 */
namespace HuubsBspViewer {
  internal class Program {
    static void Main(string[] args) {
      BspLader lader = new BspLader();
      lader.LaadBestand(@"D:\werkdirectory\SpelData\baseq2uitgepakt\baseq2\maps\base1.bsp");
      BspBuilder bspBouwer = new();
      var driehoeken = bspBouwer.GenereerDriehoeken(lader);
      
      var instellingen = NativeWindowSettings.Default;
      instellingen.Size = new Vector2i(1280, 720);
      instellingen.Title = "Quake 2 C# BSP Viewer";

      using (var venster = new BspVenster(GameWindowSettings.Default, instellingen, driehoeken)) {
        venster.Run();
      }
      Console.WriteLine("Hello, World!");
    }
  }
}
