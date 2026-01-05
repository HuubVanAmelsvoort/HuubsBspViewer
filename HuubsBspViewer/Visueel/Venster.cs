using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
public class Venster : GameWindow {
  private Tekenen _tekenen;
  private SkyboxRenderer _luchtBoxTekenen;
  private MarkerRenderer _blokjesTekenen;

  //======== Triggers ==============================================
  //private UnitCube _unitCube;// = new UnitCube();
  //private UnitCube _debugCube;
  private List<EntiteitAlgemeen> _wereldObjecten;
  //======== Triggers ==============================================

  private AlgemeneShader _shader;
  private static Camera _camera;
  GeluidHelper geluidHelper = new GeluidHelper();

  private Botsing _botsing;
  private Lader _lader;

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
        , Lader lader
        , List<TextureGroep> mapData
        , Vector3 startPositie) : base(gws, nws) {

    _lader = lader;
    _tekenen = new Tekenen();
    _botsing = new Botsing(_lader);
    _camera = new Camera(startPositie, Size.X / (float)Size.Y);
    _wereldObjecten = _lader.EntiteitenAlgemeen;
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

    //_unitCube = new UnitCube();
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
    _tekenen.Voorbereiden(_mapData);

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

    _luchtBoxTekenen = new SkyboxRenderer(skyboxPaden);
    _blokjesTekenen = new();
    // In de BspRenderer: Zorg dat je de "internal_sky" groep niet tekent met de gewone shader
    // Bijvoorbeeld door deze over te slaan in je BspRenderer.Tekenen loop
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

    if(!IsFocused) return; // Alleen bewegen als het venster actief is

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

      // 4. Update luisteraar positie voor 3D geluid
      AL.Listener(ALListener3f.Position, _camera.Positie.X, _camera.Positie.Y, _camera.Positie.Z);
      Vector3 front = _camera.Vooruit;
      Vector3 up = _camera.Omhoog;
      float[] orientation = new[] { front.X, front.Y, front.Z, up.X, up.Y, up.Z };
      AL.Listener(ALListenerfv.Orientation, orientation);

      // 4. Check triggers
      EntiteitAlgemeen triggerVoorBotsing = new(); // TODO: gebruik deze variabele om te weten in welke trigger we zitten
      _botsing.ControleerBotsing(_camera.Positie, triggerVoorBotsing);

      //Console.WriteLine(_camera.Positie);
    }

    foreach(var obj in _wereldObjecten) {
      float afstand = Vector3.Distance(_camera.Positie, obj.Positie);

      // A: Omgevingsgeluiden (Wind/Ambient)
      if(obj.IsLopen && obj.IsActief) {
        if(!obj.WasAlGeactiveerd) {
          obj.WasAlGeactiveerd = true;
          geluidHelper.Speel(obj.GeluidPad, obj); // Start de loop
        }
        // Pas constant het volume aan op basis van waar je vliegt
        geluidHelper.ZetVolume(obj, afstand, 500.0f);
      }

      // B: Triggers (Speler loopt in een gebied)

      if(obj.IsTriggerBox) {
        // Jouw bestaande methode
        bool spelerInBox = obj.IsPuntInTrigger(_camera.Positie);

        if(spelerInBox && !obj.WasAlGeactiveerd) {
          obj.WasAlGeactiveerd = true;
          ActiveerTarget(obj);
        } else if(!spelerInBox) {
          obj.WasAlGeactiveerd = false;
        }
      }
    }
  }
  private void ActiveerTarget(EntiteitAlgemeen triggerTarget) {
    //if(string.IsNullOrEmpty(triggerTarget.Doel)) return;

    // Zoek ALLE entiteiten die deze naam als doelnaam hebben
    var doelen = _lader.EntiteitenAlgemeen.Where(s => s.DoelNaam == triggerTarget.Doel).ToList();

    foreach(var doel in doelen) {
      if(doel.ClassNaam == "target_speaker") {
        string hetGeluid = $"{DiverseHelpFuncties.basispadQuake2}/sound/{doel.GeluidPad}";
        Console.WriteLine($"[Trigger] Activeert speaker: {doel.DoelNaam} met geluid: {doel.GeluidPad}");

        geluidHelper.Speel(hetGeluid, doel);
      } else {
        Console.WriteLine($"{doel.Doel} met naam {doel.DoelNaam} heeft classnaam: {doel.ClassNaam}");
      }
    }
  }
  protected override void OnRenderFrame(FrameEventArgs e) {
    base.OnRenderFrame(e);
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    _shader.Gebruiken();
    _shader.SetMatrix4("model", Matrix4.Identity);
    _shader.SetMatrix4("view", _camera.GetViewMatrix());
    _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

    _tekenen.HetTekenen(_textures, _shader);

    // Teken hier de trigger boxes
    foreach(EntiteitAlgemeen entiteit in _wereldObjecten) {
      TekenTriggerBox(entiteit);
    }
    // Klaar met tekenen van triggers

    GL.DepthMask(false);
    _luchtBoxTekenen.Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix());
    GL.DepthMask(true);

    _blokjesTekenen.TekenMarkers(_shader, _lader.EntiteitenAlgemeen);

    SwapBuffers();
  }
  private void TekenTriggerBox(EntiteitAlgemeen entiteit) {
    // Zet debug mode aan in de shader, zodat groen of rood getekend wordt
    _shader.SetInt("debugMode", 1);
    GL.Enable(EnableCap.Blend);
    GL.DepthMask(false); // Triggers blokkeren de muren erachter niet

    // Bereken grootte en positie
    Vector3 grootte = entiteit.MaximumCoordinaat - entiteit.MinimumCoordinaat;
    Vector3 midden = (entiteit.MinimumCoordinaat + entiteit.MaximumCoordinaat) / 2.0f;

    // Maak een model matrix die de standaard kubus op de juiste plek zet
    Matrix4 model = Matrix4.CreateScale(grootte) * Matrix4.CreateTranslation(midden);
    _shader.SetMatrix4("model", model);

    if(entiteit.IsPuntInTrigger(_camera.Positie)) {
      // Rood als de camera in de trigger zit
      _shader.SetVector4("debugColor", new Vector4(1.0f, 0.0f, 0.0f, 0.3f));
      //Console.WriteLine($"Je staat in trigger: {triggerZone.Classname}!");
    } else {
      // Groen als de camera er niet in zit
      _shader.SetVector4("debugColor", new Vector4(0.0f, 1.0f, 0.0f, 0.3f));
    }
    // Teken hier je standaard kubus-VBO
    //_unitCube.Render();
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