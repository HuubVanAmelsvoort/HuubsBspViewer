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
      var driehoeken = bspBouwer.GenereerDriehoeken(lader);
      // 3. Zoek de startpositie (optioneel, anders gebruik je new Vector3(0, 50, 0))
      BspEntityLader entityLader = new BspEntityLader();
      // Als je de lader hebt uitgebreid met de Entities lump:
      BspHelper.ZoekStartPositie(lader.EntiteitenTekst);

      // 4. Start het venster met de lader erbij
      var gws = GameWindowSettings.Default;
      var nws = NativeWindowSettings.Default;
      nws.Size = new Vector2i(1280, 720);
      var instellingen = NativeWindowSettings.Default;
      instellingen.Size = new Vector2i(1280, 720);
      instellingen.Title = "Huubs BSP Viewer";

      using (var venster = new BspVenster(gws
      , nws
      , lader
      , driehoeken
      , new Vector3(0, 50, 0)
      )) {
        venster.Run();
      }
    }
  }
}
