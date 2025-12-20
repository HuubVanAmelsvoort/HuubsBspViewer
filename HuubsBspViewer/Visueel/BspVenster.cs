using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class BspVenster : GameWindow {
  private BspRenderer _renderer;
  private BspShader _shader;
  private Matrix4 _cameraMatrix;
  private Vector3 _positie = new Vector3(0, 50, 0); // Start positie

  private Camera _camera;
  private Vector2 _vorigeMuisPositie;

  private BspBotsing _botsing;
  private BspLader _lader;

  public BspVenster(GameWindowSettings gws
  , NativeWindowSettings nws
  , BspLader lader
  , List<Vector3> mapData
  , Vector3 startPositie)
      : base(gws, nws) {
    _lader = lader;
    _renderer = new BspRenderer();
    _botsing = new BspBotsing(_lader); 

    // We gebruiken de startpositie uit de entities (of handmatig)
    _camera = new Camera(startPositie, Size.X / (float)Size.Y);

    // De mapData bewaren we om later in OnLoad naar de GPU te sturen
    _mapData = mapData;
  }

  private List<Vector3> _mapData;

  protected override void OnLoad() {
    base.OnLoad();
    GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f); // Donkerblauwe achtergrond
    GL.Enable(EnableCap.DepthTest); // Zorgt dat muren achter elkaar correct getoond worden
    
    //GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

    CursorState = CursorState.Grabbed; // TODO dit nog eventueel uit om het verschil van de muis te zien

    _renderer.Voorbereiden(_mapData);

    string vCode = @"
    #version 330 core
    layout(location = 0) in vec3 aPos;
    layout(location = 1) in vec3 aColor; // Nieuw: kleur input

    out vec3 vColor;
    uniform mat4 mvp;

    void main() {
      // FIX: De volgorde MOET mvp * vec4 zijn
      gl_Position = mvp * vec4(aPos, 1.0);
      vColor = aColor;
    }
    ";

    string fCode = @"
    #version 330 core
    in vec3 vColor;
    out vec4 FragColor;

    void main() {
        FragColor = vec4(vColor, 1.0);
    }";

    _shader = new BspShader(vCode, fCode);

    _camera = new Camera(new Vector3(0, 100, 0), Size.X / (float)Size.Y);
    CursorState = CursorState.Grabbed; // Verberg muis en zet vast in venster
  }

  protected override void OnUpdateFrame(FrameEventArgs e) {
    base.OnUpdateFrame(e);

    var input = KeyboardState;
    float snelheid = 500.0f * (float)e.Time;
    float gevoeligheid = 0.2f;

    if (input.IsKeyDown(Keys.Escape)) Close();

    // 1. Muis (rondkijken) - Dit mag altijd, want draaien verandert je positie niet
    var muis = MouseState;
    if (_vorigeMuisPositie == Vector2.Zero) {
      _vorigeMuisPositie = new Vector2(muis.X, muis.Y);
    } else {
      float deltaX = muis.X - _vorigeMuisPositie.X;
      float deltaY = muis.Y - _vorigeMuisPositie.Y;
      _vorigeMuisPositie = new Vector2(muis.X, muis.Y);

      _camera.Yaw += deltaX * gevoeligheid;
      _camera.Pitch -= deltaY * gevoeligheid;
    }

    // 2. Bewegingsrichting bepalen (we verplaatsen de camera nog NIET)
    Vector3 beweging = Vector3.Zero;

    // Vooruit / Achteruit
    if (input.IsKeyDown(Keys.W)) beweging += _camera.Vooruit;
    if (input.IsKeyDown(Keys.S)) beweging -= _camera.Vooruit;

    // Links / Rechts (Strafe)
    Vector3 rechts = Vector3.Normalize(Vector3.Cross(_camera.Vooruit, Vector3.UnitY));
    if (input.IsKeyDown(Keys.A)) beweging -= rechts;
    if (input.IsKeyDown(Keys.D)) beweging += rechts;

    // 3. De botsingstest uitvoeren
    if (beweging != Vector3.Zero) {
      // Zorg dat je niet sneller gaat als je twee toetsen tegelijk indrukt (diagonaal)
      beweging = Vector3.Normalize(beweging);

      // Bereken waar we zouden landen
      Vector3 gewenstePositie = _camera.Positie + (beweging * snelheid);

      // Vraag aan de BSP-boom: Is dit een 'leeg' blad (Leaf)?
      if (!_botsing.IsPuntInSolideRuimte(gewenstePositie, 0)) {
        // Alleen als het pad vrij is, updaten we de echte camera positie
        _camera.Positie = gewenstePositie;
      } else {
        // Console.WriteLine("Botsing gedetecteerd!");
        // Je zit tegen een muur! 
        // Hier zou je later 'sliding' logica kunnen toevoegen.
      }
    }
  }

  protected override void OnRenderFrame(FrameEventArgs e) {
    base.OnRenderFrame(e);
    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    _shader.Gebruiken();

    // De Matrix berekening is nu veel simpeler via de Camera klasse
    Matrix4 mvp = _camera.GetViewMatrix() * _camera.GetProjectionMatrix();

    int loc = GL.GetUniformLocation(_shader.Handle, "mvp");
    GL.UniformMatrix4(loc, false, ref mvp);

    _renderer.Tekenen();
    SwapBuffers();
  }
}