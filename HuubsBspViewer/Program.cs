using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
/**
 * Met behulp van Gemini AI (https://www.gemini.com/) gegenereerde code.
 */
namespace HuubsBspViewer {
  internal class Program {
    static void Main(string[] args) {
      if (args.Length < 1) {
        Console.WriteLine("Gebruik: HuubsBspViewer <pad_naar_bsp_bestand>");
        return;
      }

      Lader lader = new Lader();

      lader.LaadBestand(args[0]);

      BspBuilder bspBouwer = new();
      List<TextureGroep> driehoeken = bspBouwer.GenereerDriehoeken(lader);

      DiverseHelpFuncties.ZoekStartPositieEnLuchtNaam(lader.entiteiten);

      // 4. Start het venster met de lader erbij
      GameWindowSettings gameWindowSettings = GameWindowSettings.Default;
      NativeWindowSettings nativeWindowSettings = NativeWindowSettings.Default;
      nativeWindowSettings.ClientSize = new Vector2i(1024, 768);
      nativeWindowSettings.Flags = ContextFlags.Debug; // zororgt voor debug output in de console
      nativeWindowSettings.Title = "Huubs BSP Viewer";
      nativeWindowSettings.StartVisible = true;

      using (Venster venster = new Venster(
        gameWindowSettings
      , nativeWindowSettings
      , lader
      , driehoeken
      , DiverseHelpFuncties.startPos
      )) {
        venster.Run();
      }
    }
  }
}
