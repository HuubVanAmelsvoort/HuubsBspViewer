using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static Trigger;
public class Venster : GameWindow {
  private Tekenen _renderer;
  private SkyboxRenderer _skyboxRenderer;
  private MarkerRenderer _markerRenderer;

  //======== Triggers ==============================================
  private UnitCube _unitCube;// = new UnitCube();
  private List<Trigger> _actieveTriggers = new List<Trigger>();
  //private UnitCube _debugCube;
  private List<Trigger> _triggers;// = new List<TriggerZone>();
  private List<DoelClass> _doelen;
  private List<Trigger> _logischetriggers;// = new List<TriggerZone>();
  //======== Triggers ==============================================

  private AlgemeneShader _shader;
  private static Camera _camera;
  GeluidHelper geluidHelper = new GeluidHelper();

  private BspBotsing _botsing;
  private BspLader _lader;

  public Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
  List<TextureGroep> _mapData;
  private Texture _texture;
  // Noclip variabele
  bool noclip = false;

  //======== Geluid ==============================================
  private GeluidHelper _geluidHelper;// = new GeluidHelper();
  //======== Eind Geluid =========================================

  public Venster(GameWindowSettings gws
        , NativeWindowSettings nws
        , BspLader lader
        , List<TextureGroep> mapData
        , Vector3 startPositie) : base(gws, nws) {

    _lader = lader;
    _renderer = new Tekenen();
    _botsing = new BspBotsing(_lader);
    _camera = new Camera(startPositie, Size.X / (float)Size.Y);
    //_positie = startPositie;
    _triggers = _lader.triggers;
    _doelen = _lader.doelen;
    _logischetriggers = _lader.logischetriggers;
    // De mapData bewaren we om later in OnLoad naar de GPU te sturen
    _mapData = mapData;
    DiverseHelpFuncties._botsing = _botsing;
    DiverseHelpFuncties._camera = _camera;

    Console.WriteLine("Noclip is de N toets !!");
  }
  protected override void OnLoad() {
    base.OnLoad();

    AlleGeluidenDoen();

    int bron = AL.GenSource();

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
    _markerRenderer = new();
    // In de BspRenderer: Zorg dat je de "internal_sky" groep niet tekent met de gewone shader
    // Bijvoorbeeld door deze over te slaan in je BspRenderer.Tekenen loop
  }

  private void AlleGeluidenDoen() {
    List<DoelClass> omgevingsGeluiden = _doelen.Where(e =>
        e.ClassNaam == "target_speaker" &&
        string.IsNullOrEmpty(e.DoelNaam) && // Heeft geen targetname, dus wordt niet aangestuurd door iets anders
        e.GedragVlaggen == 1                 // Moet direct starten en loopen
    ).ToList();

    foreach(DoelClass geluidEntiteit in omgevingsGeluiden) {

      if(!geluidEntiteit.WasAlGeactiveerd) {
        geluidEntiteit.WasAlGeactiveerd = true;
        string hetGeluid = $"{DiverseHelpFuncties.basispadQuake2}/sound/{geluidEntiteit.GeluidPad}";
        geluidHelper.HaalBuffer(hetGeluid);
        Console.WriteLine($"Omgevingsgeluid starten: {hetGeluid}");
        geluidHelper.Speel(hetGeluid, geluidEntiteit);
      } else {
        Console.WriteLine($"Omgevingsgeluid al actief: {geluidEntiteit.GeluidPad}");
      }
    } // eind foreach

    var deb = 1;
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
      Trigger triggerVoorBotsing = new(); // TODO: gebruik deze variabele om te weten in welke trigger we zitten
      _botsing.ControleerBotsing(_camera.Positie, triggerVoorBotsing);

      //Console.WriteLine(_camera.Positie);

      //========================voor geluid ==================================
      AL.Listener(ALListener3f.Position, _camera.Positie.X, _camera.Positie.Y, _camera.Positie.Z);

      // Update de richting waarin je kijkt (zodat links/rechts werkt)
      Vector3 forward = _camera.Vooruit;
      Vector3 up = Vector3.UnitY; // Omdat je geen up in je camera hebt
      float[] oriëntatie = { forward.X, forward.Y, forward.Z, up.X, up.Y, up.Z };
      AL.Listener(ALListenerfv.Orientation, oriëntatie);

      //==========================================================
      // In je Update loop
      foreach(Trigger trigger in _triggers) {
        bool spelerInBox = trigger.IsPuntInTrigger(_camera.Positie);

        float afstand = Vector3.Distance(_camera.Positie, trigger.Positie);
        if(afstand < 100 && !trigger.WasAlGeactiveerd) { // Als je heel dichtbij het gele blokje bent
          string tekst = $"Dichtbij speaker! Afstand: {afstand}. Zou nu moeten spelen: ";
          tekst = $"{tekst}{trigger.GeluidPad} _camera.Positie: ";
          tekst = $"{tekst} {_camera.Positie.X},{_camera.Positie.Y},{_camera.Positie.Z} trigger.Positie: ";
          Console.WriteLine($"{tekst} {trigger.Positie.X},{trigger.Positie.Y},{trigger.Positie.Z}");
          var deb1 = 1;
        }

        if(spelerInBox) {
          //trigger.DebugColor = new Vector4(1, 0, 0, 0.4f); // Rood

          if(!trigger.WasAlGeactiveerd) {
            trigger.WasAlGeactiveerd = true;

            ActiveerTarget(trigger);
          }
        } else {
          //trigger.DebugColor = new Vector4(0, 1, 0, 0.4f); // Groen
          trigger.WasAlGeactiveerd = false; // Reset als je de zone verlaat
          // Optioneel: reset WasAlGeactiveerd als het een 'trigger_multiple' is
        }
      }
    }
  }

  private void ActiveerTarget(Trigger triggerTarget) {

    Console.Write($"In ActiveerTarget: {triggerTarget.Boodschap} {triggerTarget.Doel} ");

    DoelClass doel = _lader.doelen.Find(s => s.DoelNaam == triggerTarget.Doel);

    //if(Vector3.Distance(_camera.Positie, doel.Positie) < 200) {
    //  Console.WriteLine($"In bereik van speaker: {doel.GeluidPad}");
    //}

    if(doel != null
    && doel.ClassNaam == "target_speaker"
    && doel.DoelNaam != null
    && triggerTarget.Doel != null) {
      string hetGeluid = $"{DiverseHelpFuncties.basispadQuake2}/sound/{doel.GeluidPad}";
      geluidHelper.HaalBuffer(hetGeluid);
      Console.WriteLine($" - Geluid gevonden: {hetGeluid}");
      geluidHelper.Speel(hetGeluid, doel);
    } else {
      Console.WriteLine($" - Geen geluid gevonden. {triggerTarget.Doel} / {triggerTarget.DoelNaam}");
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
    //foreach(Trigger trigger in _triggers) {
    //  TekenTriggerBox(trigger);
    //}
    // Klaar met tekenen van triggers

    GL.DepthMask(false);
    _skyboxRenderer.Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix());
    GL.DepthMask(true);

    _markerRenderer.TekenMarkers(_shader, _lader.doelen);

    SwapBuffers();
  }
  private void TekenTriggerBox(Trigger triggerZone) {
    // Zet debug mode aan in de shader, zodat groen of rood getekend wordt
    _shader.SetInt("debugMode", 1);
    GL.Enable(EnableCap.Blend);
    GL.DepthMask(false); // Triggers blokkeren de muren erachter niet

    // Bereken grootte en positie
    Vector3 grootte = triggerZone.MaximumCoordinaat - triggerZone.MinimumCoordinaat;
    Vector3 midden = (triggerZone.MinimumCoordinaat + triggerZone.MaximumCoordinaat) / 2.0f;

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