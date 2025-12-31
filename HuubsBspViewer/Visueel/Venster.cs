using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static TriggerZone;
public class Venster : GameWindow {
  private Tekenen _renderer;
  private SkyboxRenderer _skyboxRenderer;

  //======== Triggers ==============================================
  private UnitCube _unitCube;// = new UnitCube();
  private List<TriggerZone> _actieveTriggers = new List<TriggerZone>();
  //private UnitCube _debugCube;
  private List<TriggerZone> _triggers;// = new List<TriggerZone>();
  private List<TriggerZone> _logischetriggers;// = new List<TriggerZone>();
  //======== Triggers ==============================================


  private AlgemeneShader _shader;
  private static Camera _camera;
  private Matrix4 _cameraMatrix;
  private Vector3 _positie;

  private BspBotsing _botsing;
  private BspLader _lader;

  public Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
  List<TextureGroep> _mapData;
  private Texture _texture;
  // Noclip variabele
  bool noclip = false;
  public Venster(GameWindowSettings gws
        , NativeWindowSettings nws
        , BspLader lader
        , List<TextureGroep> mapData
        , Vector3 startPositie) : base(gws, nws) {
    _lader = lader;
    _renderer = new Tekenen();
    _botsing = new BspBotsing(_lader);
    _camera = new Camera(startPositie, Size.X / (float)Size.Y);
    _positie = startPositie;
    _triggers = _lader.triggers;
    _logischetriggers = _lader.logischetriggers;
    // De mapData bewaren we om later in OnLoad naar de GPU te sturen
    _mapData = mapData;
    DiverseHelpFuncties._botsing = _botsing;
    DiverseHelpFuncties._camera = _camera;

    Console.WriteLine("Noclip is de N toets !!");

  }
  protected override void OnLoad() {
    base.OnLoad();

    _unitCube = new UnitCube();
    // Laad hier je triggers in uit de BSP (is al gedaan in de constructor)

    GL.Enable(EnableCap.DebugOutput); // voor debuggen
    GL.DebugMessageCallback(DiverseHelpFuncties.HuubDebugCallback, IntPtr.Zero);

    // 1. Scherm instellingen
    GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f); // Achtergrondkleur (donkergrijs/blauw)
    GL.Enable(EnableCap.DepthTest);        // Zorgt dat muren achter elkaar correct getekend worden

    // 2. Muis vangen (Cruciaal voor rondkijken!)
    // Dit zorgt ervoor dat de muis niet uit het scherm loopt en onzichtbaar wordt
    CursorState = CursorState.Grabbed;

    // 3. Shader initialisatie
    _shader = new AlgemeneShader(DiverseHelpFuncties.vertexCode, DiverseHelpFuncties.fragmentCode);
    _shader.Gebruiken();

    // 4. Renderer voorbereiden
    // _mapData bevat de driehoeken die we in de constructor hebben meegekregen
    _renderer.Voorbereiden(_mapData);

    foreach(var naam in _lader.TextureNamen) {
      string pad = naam;
      if(naam.Contains("trigger")) {
        // Triggers hebben geen texture, sla deze over
        continue;
      }

      if(!_textures.ContainsKey(pad)) {
        // Controleer of de PNG bestaat, anders laad een placeholder
        string volledigePad = $"{DiverseHelpFuncties.basispadQuake2}/textures/{pad}.png";
        if(File.Exists(volledigePad)) {
          _textures[naam] = new Texture(volledigePad);
        } else {
          _texture = new Texture("Quake2.png");
        }
      } // if texture already loaded
    } // eind foreach

    //_camera.Snelheid = 1f;
    //_camera.RechtsLinks = MathHelper.DegreesToRadians(180f); wordt al in camera gezet
    //_camera.UpdateVectoren();

    // hier voor de lucht:
    string luchtNaam = DiverseHelpFuncties.luchtNaam;
    List<string> skyboxPaden = new List<string> {
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}rt.png", // Right (+X)
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}lf.png", // Left (-X)
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}up.png", // Up (+Y)
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}dn.png", // Down (-Y)
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}bk.png", // Back (+Z)
        $"{DiverseHelpFuncties.basispadQuake2}/env/{luchtNaam}ft.png", // Front (-Z)
    };

    _skyboxRenderer = new SkyboxRenderer(skyboxPaden);

    // In de BspRenderer: Zorg dat je de "internal_sky" groep niet tekent met de gewone shader
    // Bijvoorbeeld door deze over te slaan in je BspRenderer.Tekenen loop
  }

  protected override void OnUpdateFrame(FrameEventArgs e) {
    base.OnUpdateFrame(e);

    DiverseHelpFuncties.VerwerkMuis(MouseState, _camera);

    DiverseHelpFuncties.BerekenBeweging((float)e.Time);
    if(DiverseHelpFuncties.beweging != Vector3.Zero) {
      Vector3 gewenstePositie = _camera.Positie + DiverseHelpFuncties.beweging;

      if(noclip) {
        _camera.Positie = gewenstePositie; // Beweeg dwars door muren
      } else {
        // 3. Botsing checken
        _camera.Positie = _botsing.ValideerVerplaatsing(_camera.Positie, gewenstePositie);
      }
      // 4. Check triggers
      TriggerZone t = new(); // TODO: gebruik deze variabele om te weten in welke trigger we zitten
      _botsing.ControleerBotsing(_camera.Positie, t);

      //Console.WriteLine(_camera.Positie);

      //if(staatErin && !trigger.WasAlGeactiveerd) {
      //  trigger.WasAlGeactiveerd = true; // Zorg dat het geluid niet 60x per seconde start

      //  // Zoek de speaker die bij deze trigger hoort
      //  var speaker = _alleSpeakers.Find(s => s.TargetName == trigger.Target);
      //  if(speaker != null) {
      //    SpeelGeluidAf(speaker.Noise);
      //  }
      //}
    }
  }
  protected override void OnRenderFrame(FrameEventArgs e) {
    base.OnRenderFrame(e);
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    _shader.Gebruiken();
    _shader.SetMatrix4("model", Matrix4.Identity);
    _shader.SetMatrix4("view", _camera.GetViewMatrix());
    _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

    _renderer.HetTekenen(_textures, _shader);

    //// Teken hier de trigger boxes
    foreach(TriggerZone trigger in _triggers) {
      //TekenTriggerBox(trigger);
    }
    // Klaar met tekenen van triggers

    GL.DepthMask(false);
    _skyboxRenderer.Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix());
    GL.DepthMask(true);

    SwapBuffers();
  }
  private void TekenTriggerBox(TriggerZone triggerZone) {
    _shader.SetInt("debugMode", 0);

    GL.Enable(EnableCap.Blend);
    GL.DepthMask(false); // Triggers blokkeren de muren erachter niet

    // Bereken grootte en positie
    Vector3 grootte = triggerZone.Maxs - triggerZone.Mins;
    Vector3 midden = (triggerZone.Mins + triggerZone.Maxs) / 2.0f;

    // Maak een model matrix die de standaard kubus op de juiste plek zet
    Matrix4 model = Matrix4.CreateScale(grootte) * Matrix4.CreateTranslation(midden);
    _shader.SetMatrix4("model", model);

    if(triggerZone.IsPuntInTrigger(_camera.Positie)) {
      // Rood als de camera in de trigger zit
      _shader.SetVector4("debugColor", new Vector4(1.0f, 0.0f, 0.0f, 0.3f));
      //Console.WriteLine($"Je staat in trigger: {triggerZone.Classname}!");
    } else {
      // Groen als de camera er niet in zit
      _shader.SetVector4("debugColor", new Vector4(0.0f, 1.0f, 0.0f, 0.3f));
    }
    // Teken hier je standaard kubus-VBO
    _unitCube.Render();
    _shader.SetInt("debugMode", 0);
  }
  protected override void OnKeyDown(KeyboardKeyEventArgs e) {
    //Console.WriteLine("Key down: " + e.Key.ToString());

    if(e.Key == Keys.Escape) Close();

    // 'N' ingedrukt voor noclip wisselen
    if(e.Key == Keys.N) {
      noclip = !noclip;
    }

    if(e.Key == Keys.W) DiverseHelpFuncties.wIngedrukt = true;
    if(e.Key == Keys.S) DiverseHelpFuncties.sIngedrukt = true;
    if(e.Key == Keys.D) DiverseHelpFuncties.dIngedrukt = true;
    if(e.Key == Keys.A) DiverseHelpFuncties.aIngedrukt = true;
  }
  protected override void OnKeyUp(KeyboardKeyEventArgs e) {
    //Console.WriteLine("Key up: " + e.Key.ToString());

    if(e.Key == Keys.W) DiverseHelpFuncties.wIngedrukt = false;
    if(e.Key == Keys.S) DiverseHelpFuncties.sIngedrukt = false;
    if(e.Key == Keys.D) DiverseHelpFuncties.dIngedrukt = false;
    if(e.Key == Keys.A) DiverseHelpFuncties.aIngedrukt = false;

    DiverseHelpFuncties.beweging = Vector3.Zero;
  }
}