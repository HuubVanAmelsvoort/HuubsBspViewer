using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
/**
 * Met behulp van Gemini AI (https://www.gemini.com/) gegenereerde code.
 */
namespace HuubsBspViewer {
  internal class Program {
    static void Main(string[] args) {
      BspLader lader = new BspLader();
      lader.LaadBestand(args[0]);

      BspBuilder bspBouwer = new();
      List<TextureGroep> driehoeken = bspBouwer.GenereerDriehoeken(lader);

      // Als je de lader hebt uitgebreid met de Entities lump:
      DiverseHelpFuncties.ZoekStartPositieEnLuchtNaam(lader.EntiteitenTekst);

      // 4. Start het venster met de lader erbij
      var GameWindowSettings = OpenTK.Windowing.Desktop.GameWindowSettings.Default;
      var NativeWindowSettings = OpenTK.Windowing.Desktop.NativeWindowSettings.Default;
      NativeWindowSettings.Size = new Vector2i(1280, 720);
      var instellingen = OpenTK.Windowing.Desktop.NativeWindowSettings.Default;
      instellingen.Size = new Vector2i(1280, 720);
      instellingen.Title = "Huubs BSP Viewer";

      using (Venster venster = new Venster(
        GameWindowSettings
      , NativeWindowSettings
      , lader
      , driehoeken
      , DiverseHelpFuncties.startPos
      )) {
        venster.Run();
      }
    }
  }
}
