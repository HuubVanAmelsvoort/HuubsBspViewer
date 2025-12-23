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
      Vector3 startPos = DiverseHelpFuncties.ZoekStartPositie(lader.EntiteitenTekst);

      // 4. Start het venster met de lader erbij
      var gws = GameWindowSettings.Default;
      var nws = NativeWindowSettings.Default;
      nws.Size = new Vector2i(1280, 720);
      var instellingen = NativeWindowSettings.Default;
      instellingen.Size = new Vector2i(1280, 720);
      instellingen.Title = "Huubs BSP Viewer";

      using (var venster = new Venster(gws
      , nws
      , lader
      , driehoeken
      , startPos
      )) {
        venster.Run();
      }
    }
  }
}
