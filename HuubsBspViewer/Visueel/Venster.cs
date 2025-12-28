using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
public class Venster : GameWindow {
  private Tekenen _renderer;
  private SkyboxRenderer _skyboxRenderer;
  
  private Shader _shader;
  private static Camera _camera;
  private Matrix4 _cameraMatrix;
  private Vector3 _positie;

  private BspBotsing _botsing;
  private BspLader _lader;

  public Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
  List<TextureGroep> _mapData;

  private Texture _texture;
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

    // De mapData bewaren we om later in OnLoad naar de GPU te sturen
    _mapData = mapData;
    DiverseHelpFuncties._botsing = _botsing;
    DiverseHelpFuncties._camera = _camera;
  }
  protected override void OnLoad() {
    base.OnLoad();
    GL.Enable(EnableCap.DebugOutput); // voor debuggen
    GL.DebugMessageCallback(DiverseHelpFuncties.HuubDebugCallback, IntPtr.Zero);    
    
    // 1. Scherm instellingen
    GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f); // Achtergrondkleur (donkergrijs/blauw)
    GL.Enable(EnableCap.DepthTest);        // Zorgt dat muren achter elkaar correct getekend worden

    // 2. Muis vangen (Cruciaal voor rondkijken!)
    // Dit zorgt ervoor dat de muis niet uit het scherm loopt en onzichtbaar wordt
    CursorState = CursorState.Grabbed;

    // 3. Shader initialisatie
    _shader = new Shader();
    _shader.Gebruiken();

    // 4. Renderer voorbereiden
    // _mapData bevat de driehoeken die we in de constructor hebben meegekregen
    _renderer.Voorbereiden(_mapData);

    foreach (var naam in _lader.TextureNamen) {
      string pad = naam;
      if (!_textures.ContainsKey(pad)) {
        // Controleer of de PNG bestaat, anders laad een placeholder
        string volledigePad = $"{DiverseHelpFuncties.basispadQuake2}/textures/{pad}.png";
        if (File.Exists(volledigePad)) {
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

    var input = KeyboardState;

    DiverseHelpFuncties.VerwerkMuis(MouseState, _camera);

    if (input.IsKeyDown(Keys.Escape)) Close();

    Vector3 beweging = DiverseHelpFuncties.BerekenBeweging(input, (float)e.Time);
    if (beweging != Vector3.Zero) {
      Vector3 gewenstePositie = _camera.Positie + beweging;

      bool noclip = input.IsKeyDown(Keys.N); // Houd 'N' ingedrukt voor noclip

      if (noclip) {
        _camera.Positie = gewenstePositie; // Beweeg dwars door muren
      } else {
        // 3. Botsing checken
        _camera.Positie = _botsing.ValideerVerplaatsing(_camera.Positie, gewenstePositie);
      }
      //Console.WriteLine(_camera.Positie);
    }
  }
  protected override void OnRenderFrame(FrameEventArgs e) {
    base.OnRenderFrame(e);
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    
    GL.DepthMask(false);
    _skyboxRenderer.Render(_camera.GetViewMatrix(), _camera.GetProjectionMatrix());
    GL.DepthMask(true);

    _shader.Gebruiken();
    _shader.SetMatrix4("model", Matrix4.Identity);
    _shader.SetMatrix4("view", _camera.GetViewMatrix());
    _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

    _renderer.HetTekenen(_textures, _shader);

    SwapBuffers();
  }
}