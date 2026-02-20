using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using GR = OpenTK.Windowing.GraphicsLibraryFramework;
public class Venster : GameWindow {
  private TekenLevel _tekenLevel;

  private EntiteitAlgemeen _geactiveerdeTrigger = null;

  private TekenLucht _luchtBoxTekenen;
  private TekenBlokjes _tekenBlokjes;
  private TekenGltfModel _tekenModel;
  private TekenLijnBlokken _tekenLijnBlokken;

  //======== Triggers ==============================================
  private List<EntiteitAlgemeen> _wereldObjecten;
  //======== Triggers ==============================================

  private AlgemeneShader _shader;
  private static Camera _camera;
  GeluidHelper geluidHelper = new GeluidHelper();

  private Botsing _botsing;
  private Lader _lader;

  public Dictionary<string, Texture> _texturesTabel = new Dictionary<string, Texture>();

  List<TextureGroep> _mapData;
  
  // Noclip variabele
  bool noclip = false;

  //======== Voor bericht ==============================================
  private string _huidigBericht = "";
  private float _berichtTimer = 0.0f;
  //======== bericht ==============================================
  public Venster(GameWindowSettings gws
        , NativeWindowSettings nws
        , Lader lader
        , List<TextureGroep> mapData
        , Vector3 startPositie) : base(gws, nws) {

    _lader = lader;
    _tekenLevel = new TekenLevel();
    _botsing = new Botsing(_lader);
    _camera = new Camera(startPositie, Size.X / (float)Size.Y);
    _wereldObjecten = _lader.EntiteitenAlgemeen;
    // De mapData bewaren we om later in OnLoad naar de GPU te sturen
    _mapData = mapData;
    DiverseHelpFuncties._camera = _camera;

    Console.WriteLine("Noclip is de N toets !!");
  }
  /**
   * Tijdens het laden van het venster
   */
  protected override void OnLoad() {
    base.OnLoad();

    // 1. Initialiseer de Shader (Cruciaal: dit moet eerst!)
    // We maken de shader aan zodat we later GetAttribLocation kunnen aanroepen
    _shader = new AlgemeneShader(DiverseHelpFuncties.vertexCode, DiverseHelpFuncties.fragmentCode);
    _shader.Gebruiken();

    // 3. Audio & Systeem Instellingen
    AlleGeluidenDoen();
    int bron = AL.GenSource();

    // 4. Scherm & Rendering Instellingen
    GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
    GL.Enable(EnableCap.DepthTest);

    // Muis vangen
    CursorState = CursorState.Grabbed;

    // 5. BSP Data Voorbereiden
    _tekenLevel.Voorbereiden(_mapData);

    // 6. Textures Laden

    Texture texture = new Texture("Quake2.png");

    foreach(var naam in _lader.TextureNamen) {
      if(naam.Contains("trigger")) continue;

      if(!_texturesTabel.ContainsKey(naam)) {
        string volledigePad = $"{DiverseHelpFuncties.basispadQuake2}/textures/{naam}.png";
        if(File.Exists(volledigePad)) {
          _texturesTabel[naam] = new Texture(volledigePad);
        } else {
          // Gebruik placeholder als PNG niet bestaat
          if(!_texturesTabel.ContainsKey("placeholder"))
            _texturesTabel["placeholder"] = new Texture("Quake2.png");

          _texturesTabel[naam] = _texturesTabel["placeholder"];
        }
      }
    }

    // 7. Skybox & Overige Renderers
    string luchtNaam = DiverseHelpFuncties.luchtNaam;
    List<string> skyboxPaden = new List<string> {
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}rt.png",
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}lf.png",
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}up.png",
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}dn.png",
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}bk.png",
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}ft.png",
    };

    _luchtBoxTekenen = new TekenLucht(skyboxPaden);
    _tekenBlokjes = new();
    _tekenLijnBlokken = new(_shader);

    _tekenModel = new(DiverseHelpFuncties.ModelPad);
  }

  /**
   * Hier vindt de logica plaats, dus nog niet het tekenen, maar wel waar moet de speler naartoe bewegen etc.
   */
  protected override void OnUpdateFrame(FrameEventArgs e) {
    string titel = "";
    base.OnUpdateFrame(e);

    if(!IsFocused) {
      return; // Alleen bewegen als het venster actief is
    }

    DiverseHelpFuncties.VerwerkMuis(MouseState, _camera);

    DiverseHelpFuncties.BerekenBeweging((float)e.Time);
    if(DiverseHelpFuncties.beweging != Vector3.Zero) {
      AfhandelenBeweging();
    }

    _geactiveerdeTrigger = null; // Reset elke frame

    if(_berichtTimer > 0) {
      _berichtTimer -= (float)e.Time;
      if(_berichtTimer <= 0) _huidigBericht = "";
    }

    UpdateTitel(e.Time, titel);
  }
  /**
 * Hier vindt het tekenen plaats
*/
  protected override void OnRenderFrame(FrameEventArgs e) {
    base.OnRenderFrame(e);
    
    GL.Viewport(0, 0, Size.X, Size.Y);

    GL.BindVertexArray(0); // zorg dat er geen VAO actief is
    GL.UseProgram(_shader.Handle); // zorg dat onze shader actief is
    int debugLoc = GL.GetUniformLocation(_shader.Handle, "debugMode");
    GL.Uniform1(debugLoc, 0); // geen debug mode

    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    _shader.Gebruiken();

    // 2. NU pas de uniform zetten, nadat de shader 'Bound' is
    // Dit voorkomt de "Preview Context" fout omdat de state nu actief is

    _shader.SetMatrix4("model", Matrix4.Identity);
    _shader.SetMatrix4("view", _camera.GetViewMatrix());
    _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

    _tekenLevel.TekenenVanHetLevel(_texturesTabel, _shader);

    //_tekenLijnBlokken.TekenLijnBlokjes(_shader, _lader.EntiteitenAlgemeen, _camera);
    //_tekenBlokjes.TekenBlokjesDiverseKleuren(_shader, _lader.EntiteitenAlgemeen);

    //var deSchaal = new Vector3(32.0f);
    var deSchaal = 32;
    var verderZetten = new Vector3(0, -100, 0);
    _tekenModel.Teken(_shader, DiverseHelpFuncties.startPos + verderZetten, deSchaal);
    
    _tekenBlokjes.TekenModel(_shader, _lader.EntiteitenAlgemeen, _tekenModel);

    // Volgens mij moet hier het doel van de entiteit bepaald worden
    // en dan gehaald uit het model "*2" of "*36" etc.

    LoopDoorDeWereldCheckTrigger(); 

    if(!string.IsNullOrEmpty(_huidigBericht)) {
      TekenTekstOverlay(_huidigBericht);
    }

    GL.DepthMask(false);
    _luchtBoxTekenen.Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix());
    GL.DepthMask(true);

    SwapBuffers();
  }
  private void AlleGeluidenDoen() {
    List<EntiteitAlgemeen> omgevingsGeluiden = _wereldObjecten.Where(e =>
        e.ClassNaam == "target_speaker" &&
        string.IsNullOrEmpty(e.DoelNaam) && // Heeft geen targetname, dus wordt niet aangestuurd door iets anders
        (e.GedragVlaggen & 1) != 0                 // Moet direct starten en loopen
    ).ToList();

    foreach(EntiteitAlgemeen geluidEntiteit in omgevingsGeluiden) {

      if(!geluidEntiteit.WasAlGeactiveerd) {
        geluidEntiteit.WasAlGeactiveerd = true;
        string hetGeluid = $"{DiverseHelpFuncties.basispadQuake2}/sound/{geluidEntiteit.GeluidPad}";
        geluidHelper.HaalBuffer(hetGeluid);
        //Console.WriteLine($"Omgevingsgeluid starten: {hetGeluid}");
        geluidHelper.SpeelGeluid(hetGeluid, geluidEntiteit);
      } else {
        Console.WriteLine($"Omgevingsgeluid al actief: {geluidEntiteit.GeluidPad}");
      }
    } // eind foreach

    Console.WriteLine($"Totaal omgevingsgeluiden gestart: {omgevingsGeluiden.Count}\n\n\n");
  }
  /**
   * Hier afhankelijk van de beweging de nieuwe positie berekenen
   */
  private void AfhandelenBeweging() {
    // 2. Bereken de gewenste nieuwe positie
    Vector3 gewenstePositie = _camera.Positie + DiverseHelpFuncties.beweging;
    if(noclip) {
      _camera.Positie = gewenstePositie; // Beweeg dwars door muren
    } else {
      // 3. Botsing checken
      _camera.Positie = _botsing.ValideerVerplaatsing(_camera.Positie, gewenstePositie);
    }

    // 4. Update luisteraar positie voor 3D geluid
    AL.Listener(ALListener3f.Position, _camera.Positie.X, _camera.Positie.Y, _camera.Positie.Z);
    Vector3 front = _camera.Vooruit;
    Vector3 up = _camera.Omhoog;
    float[] orientation = new[] { front.X, front.Y, front.Z, up.X, up.Y, up.Z };
    AL.Listener(ALListenerfv.Orientation, orientation);
  }
  /**
   * Loop door alle wereldobjecten en check of de speler een trigger activeert
   */
  private void LoopDoorDeWereldCheckTrigger() {
    string titel = "basisTitel";
    float afstand = 0;

    foreach(EntiteitAlgemeen entiteitAlgemeen in _wereldObjecten) {
      bool spelerRaaktObject = false;
      // A. Als het een box-trigger is (Min/Max zijn gevuld)
      if(entiteitAlgemeen.MinimumCoordinaat != Vector3.Zero || entiteitAlgemeen.MaximumCoordinaat != Vector3.Zero) {
        spelerRaaktObject = entiteitAlgemeen.IsPuntInTrigger(_camera.Positie, 1.0f);
      }
      // B. Als het een punt-object is (zoals een target_speaker of monster)
      else if(entiteitAlgemeen.Positie != Vector3.Zero) {
        // Gebruik afstand in plaats van een box
        afstand = Vector3.Distance(_camera.Positie, entiteitAlgemeen.Positie);
        spelerRaaktObject = (afstand < 64.0f); // 64 units is ongeveer 'dichtbij'
      }

      if(spelerRaaktObject) {
        _geactiveerdeTrigger = entiteitAlgemeen;
        if(!entiteitAlgemeen.WasAlGeactiveerd) {
          entiteitAlgemeen.WasAlGeactiveerd = true;
          ActiveerHetDoel(entiteitAlgemeen, _camera);
        }
      }

      titel = $"{entiteitAlgemeen.ClassNaam}";
      if(!string.IsNullOrEmpty(entiteitAlgemeen.DoelNaam)) titel += $" (ID: {entiteitAlgemeen.DoelNaam})";
      if(!string.IsNullOrEmpty(entiteitAlgemeen.Doel)) titel += $" -> Target: {entiteitAlgemeen.Doel}";
      if(string.IsNullOrEmpty(titel)) {
        titel = "Entiteit in trigger box";
      }
    }
  }
  protected override void OnResize(ResizeEventArgs e) {
    base.OnResize(e);
    GL.Viewport(0, 0, e.Width, e.Height);
  }

  protected override void OnKeyDown(KeyboardKeyEventArgs e) {
    //Console.WriteLine("Key down: " + e.Key.ToString());

    if(e.Key == GR.Keys.Escape) Close();

    // 'N' ingedrukt voor noclip wisselen
    if(e.Key == GR.Keys.N) {
      noclip = !noclip;
    }

    if(e.Key == GR.Keys.W) DiverseHelpFuncties.wIngedrukt = true;
    if(e.Key == GR.Keys.S) DiverseHelpFuncties.sIngedrukt = true;
    if(e.Key == GR.Keys.D) DiverseHelpFuncties.dIngedrukt = true;
    if(e.Key == GR.Keys.A) DiverseHelpFuncties.aIngedrukt = true;
  }
  protected override void OnKeyUp(KeyboardKeyEventArgs e) {
    //Console.WriteLine("Key up: " + e.Key.ToString());

    if(e.Key == GR.Keys.W) DiverseHelpFuncties.wIngedrukt = false;
    if(e.Key == GR.Keys.S) DiverseHelpFuncties.sIngedrukt = false;
    if(e.Key == GR.Keys.D) DiverseHelpFuncties.dIngedrukt = false;
    if(e.Key == GR.Keys.A) DiverseHelpFuncties.aIngedrukt = false;

    DiverseHelpFuncties.beweging = Vector3.Zero;
  }
  public void ToonBericht(string bericht, float duur = 3.0f) {
    _huidigBericht = bericht;
    _berichtTimer = duur;
  }
  private void TekenTekstOverlay(string tekst) {
    // 1. Schakel diepte-test uit (tekst moet altijd bovenop)
    GL.Disable(EnableCap.DepthTest);

    // 2. Zet de projectie op 2D (pixels in plaats van 3D coördinaten)
    var ortho = Matrix4.CreateOrthographicOffCenter(0, Size.X, Size.Y, 0, -1, 1);
    // Stuur deze matrix naar een simpele tekst-shader of gebruik GL.WindowByProperty als je die hebt

    //int location = GL.GetUniformLocation(tekstShaderHandle, "uProjection");
    //GL.UniformMatrix4(location, false, ref orthoMatrix);

    // Gebruik je een bibliotheek zoals QuickFont of ImGui? 
    // Zo niet, dan is de console-titel nog steeds de 'cleanste' manier zonder shaders te schrijven:
    Title = $"Dit is de tekst uit het Venster - {tekst}";
    GL.Enable(EnableCap.DepthTest);
  }
  private void UpdateTitel(double frameTijd, string entiteitInfo = "") {
    double fps = 1.0 / frameTijd;

    string fpsInfo = $"FPS: {fps:F1}";
    string posInfo = $"Pos: {_camera.Positie.X:0}, {_camera.Positie.Y:0}, {_camera.Positie.Z:0}";

    if(!string.IsNullOrEmpty(entiteitInfo)) {
      Title = $"{fpsInfo} | {posInfo} | [ENTITY] {entiteitInfo}";
    } else {
      Title = $"{fpsInfo} | {posInfo} | Quake 2 Explorer";
    }
  }
  /**
   * Hier als er tegen een doel aangelopen wordt.
   * Bijvoorbeeld een knop. func_button.
   */
  private void ActiveerHetDoel(EntiteitAlgemeen entiteitAlgemeenDoel, Camera camera) {
    string debugTekst = $"LoopDoorDeWereld ActiveerHetDoel:";
    debugTekst = $"{debugTekst} class: {entiteitAlgemeenDoel.ClassNaam} op {entiteitAlgemeenDoel.Positie}";
    debugTekst = $"{debugTekst} doel: {entiteitAlgemeenDoel.Doel} MaximumCoordinaat: {entiteitAlgemeenDoel.MaximumCoordinaat}";
    debugTekst = $"{debugTekst} modelIndex: {entiteitAlgemeenDoel.ModelIndex} MinimumCoordinaat: {entiteitAlgemeenDoel.MinimumCoordinaat}";
    debugTekst = $"{debugTekst} camera.Posistie: {camera.Positie}";

    Console.WriteLine(debugTekst);
    var debugje = _lader.Modellen;

    // Zoek ALLE entiteiten die deze naam als doelnaam hebben
    List<EntiteitAlgemeen> entiteiten = _lader.EntiteitenAlgemeen.Where(s => s.DoelNaam == entiteitAlgemeenDoel.Doel).ToList();

    string monsterPad = "sound/berserk/sight"; // Standaard monster geluid
    bool monsterGevonden = false;

    string hetGeluid = "";

    switch(entiteitAlgemeenDoel.ClassNaam) {
      case "target_speaker":
        hetGeluid = $"{DiverseHelpFuncties.basispadQuake2}/sound/{entiteitAlgemeenDoel.GeluidPad}";
        //Console.WriteLine($"Activeert speaker: {doel.DoelNaam} met geluid: {hetGeluid}");
        break;
      //====================================================================
      case "target_explosion":
        hetGeluid = $"{DiverseHelpFuncties.basispadQuake2}/sound/weapons/rocklx1a.wav"; // Standaard explosie
        Console.WriteLine($"Activeert target_explosion: {entiteitAlgemeenDoel.DoelNaam} met geluid: {hetGeluid}");
        break;
      //====================================================================
      case "monster_gunner":
        monsterPad = "gunner/sight1";
        monsterGevonden = true;
        break;
      case "monster_infantry":
        monsterPad = "infantry/INFSGHT1";
        monsterGevonden = true;
        break;
      case "monster_soldier_light":
        monsterPad = "soldier/SOLSGHT1";
        monsterGevonden = true;
        break;
      case "monster_soldier_ss": 
        monsterPad = "soldier/SOLSRCH1"; // kan verder niet iets vinden
        monsterGevonden = true;
        break;
      case "misc_deadsoldier":
        monsterPad = "soldier/SOLDETH1"; // kan verder niet iets vinden
        monsterGevonden = true;
        break;

      case "func_button":
        monsterPad = "switches/butn2"; 
        monsterGevonden = true;
        break;
      case "func_door":
      case "sp_func_door":
        monsterPad = "doors/dr1_strt"; // moet in het midden dr1_mid zijn en dr1_end aan het eind
        monsterGevonden = true;
        break;
      case "func_train":
      case "func_plat":
      case "func_areaportal":
      case "misc_model":
      case "trigger_multiple":
      case "trigger_once":
      case "info_player_start":
      case "ammo_slugs":
      case "info_player_intermission":
      case "func_wall":
      case "ammo_shells":
      case "light":
      case "ammo_bullets":
      case "info_player_coop":
      case "info_player_deathmatch":
      case "trigger_push":
      case "trigger_hurt":
      case "trigger_relay":
        break;
      case "target_help":
        if(!string.IsNullOrEmpty(entiteitAlgemeenDoel.Boodschap)) {
          ToonBericht(entiteitAlgemeenDoel.Boodschap);
        } else {
          // Als er geen boodschap is, toon dan wat we activeren
          ToonBericht($"Activeren:  {entiteitAlgemeenDoel.ClassNaam} / {entiteitAlgemeenDoel.DoelNaam} / {entiteitAlgemeenDoel.Doel}");
        }
       break;
      ////====================================================================
      default:
        Console.WriteLine($"Default in ActiveerHetDoel: {entiteitAlgemeenDoel.Doel} met naam {entiteitAlgemeenDoel.DoelNaam} heeft classnaam: {entiteitAlgemeenDoel.ClassNaam}");
        break;
    }

    if(monsterGevonden) {
      hetGeluid = $"{DiverseHelpFuncties.basispadQuake2}/sound/{monsterPad}.wav"; // Een 'wakker worden' geluid
      Console.WriteLine($"Activeer monster: {entiteitAlgemeenDoel.DoelNaam} van class {entiteitAlgemeenDoel.ClassNaam} het geluid: {hetGeluid}");
    }

    if(!string.IsNullOrEmpty(hetGeluid)) {
      geluidHelper.SpeelGeluid(hetGeluid, entiteitAlgemeenDoel);
      Console.WriteLine($"[Actie] {entiteitAlgemeenDoel.ClassNaam} geluid gemaakt: {hetGeluid}");
    }
  }

}