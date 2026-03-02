using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

/**
 * Met behulp van Gemini AI (https://www.gemini.com/) gegenereerde code.
 */
namespace HuubsBspViewer {
  internal class Program {
    [STAThread]
    static void Main(string[] args) {

      if (args.Length < 2) {
        DiverseHelpFuncties.SchrijfRegel("Gebruik: HuubsBspViewer <pad_naar_bsp_bestand> <model>");
        return;
      }
      DiverseHelpFuncties.BspPad = args[0];
      DiverseHelpFuncties.ModelPad = args[1];

      DiverseHelpFuncties.basispadQuake2 = Path.GetDirectoryName(args[0])
          + Path.DirectorySeparatorChar
      + ".."
          + Path.DirectorySeparatorChar;
      LadenEnVerzamelen lader = new LadenEnVerzamelen();

      DiverseHelpFuncties.BspPad = DiverseHelpFuncties.Dialoog("bsp");
      DiverseHelpFuncties.ModelPad = DiverseHelpFuncties.Dialoog("gltf");

      lader.LaadBspBestand(DiverseHelpFuncties.BspPad);

      BspBouwer bspBouwer = new();
      List<PlaatjesGroep> driehoeken = bspBouwer.GenereerDriehoeken(lader, DiverseHelpFuncties.textureAfmetingen);

      DiverseHelpFuncties.ZoekStartPositieEnLuchtNaam(lader.entiteiten);

      // 4. Start het venster met de lader erbij
      GameWindowSettings gameWindowSettings = GameWindowSettings.Default;
      NativeWindowSettings nativeWindowSettings = NativeWindowSettings.Default;

      nativeWindowSettings.ClientSize = new Vector2i(1024, 768);
      nativeWindowSettings.Flags = ContextFlags.Debug; // zororgt voor debug output in de console
      nativeWindowSettings.Title = "Huubs BSP Viewer";
      nativeWindowSettings.StartVisible = true;

      nativeWindowSettings.Location = new Vector2i(50, 50);

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
