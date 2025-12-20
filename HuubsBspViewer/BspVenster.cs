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
  public BspVenster(GameWindowSettings gws, NativeWindowSettings nws, List<Vector3> mapData)
      : base(gws, nws) {
    _renderer = new BspRenderer();
    _mapData = mapData;
  }

  private List<Vector3> _mapData;

  protected override void OnLoad() {
    base.OnLoad();
    GL.ClearColor(0.1f, 0.1f, 0.2f, 1.0f); // Donkerblauwe achtergrond
    GL.Enable(EnableCap.DepthTest); // Zorgt dat muren achter elkaar correct getoond worden
    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

    _renderer.Voorbereiden(_mapData);

    // Simpele shaders (normaal laad je deze uit een bestand)
    //string vCode = @"#version 330 core
    //                    layout (location = 0) in vec3 aPos;
    //                    uniform mat4 mvp;
    //                    void main() { gl_Position = vec4(aPos, 1.0) * mvp; }";
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

    //string fCode = @"#version 330 core
    //                    out vec4 FragColor;
    //void main() { FragColor = vec4(0.7, 0.7, 0.7, 1.0); }
    //";
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

  //protected override void OnUpdateFrame(FrameEventArgs e) {
  //  base.OnUpdateFrame(e);

  //  if (!IsFocused) return; // Alleen bewegen als het venster actief is

  //  var input = KeyboardState;
  //  float snelheid = 500.0f * (float)e.Time; // Quake muren zijn groot!

  //  // Beweeg de camera positie
  //  if (input.IsKeyDown(Keys.W)) _positie += Vector3.UnitZ * snelheid;
  //  if (input.IsKeyDown(Keys.S)) _positie -= Vector3.UnitZ * snelheid;
  //  if (input.IsKeyDown(Keys.A)) _positie += Vector3.UnitX * snelheid;
  //  if (input.IsKeyDown(Keys.D)) _positie -= Vector3.UnitX * snelheid;
  //  if (input.IsKeyDown(Keys.Space)) _positie += Vector3.UnitY * snelheid;
  //  if (input.IsKeyDown(Keys.LeftControl)) _positie -= Vector3.UnitY * snelheid;
  //}

  //protected override void OnRenderFrame(FrameEventArgs e) {
  //  base.OnRenderFrame(e);
  //  GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

  //  _shader.Gebruiken();

  //  // Bereken Camera (Projection * View)
  //  Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), Size.X / (float)Size.Y, 1.0f, 10000.0f);
  //  Matrix4 view = Matrix4.LookAt(_positie, _positie + Vector3.UnitZ, Vector3.UnitY);
  //  Matrix4 mvp = view * projection;

  //  int loc = GL.GetUniformLocation(_shader.Handle, "mvp");
  //  GL.UniformMatrix4(loc, false, ref mvp);

  //  _renderer.Tekenen();
  //  SwapBuffers();
  //}

  protected override void OnUpdateFrame(FrameEventArgs e) {
    base.OnUpdateFrame(e);

    var input = KeyboardState;
    float snelheid = 400.0f * (float)e.Time;
    float gevoeligheid = 0.2f;

    // Toetsenbord (bewegen relatief aan waar je kijkt)
    if (input.IsKeyDown(Keys.W)) _camera.Positie += _camera.Vooruit * snelheid;
    if (input.IsKeyDown(Keys.S)) _camera.Positie -= _camera.Vooruit * snelheid;
    if (input.IsKeyDown(Keys.A)) _camera.Positie -= Vector3.Normalize(Vector3.Cross(_camera.Vooruit, Vector3.UnitY)) * snelheid;
    if (input.IsKeyDown(Keys.D)) _camera.Positie += Vector3.Normalize(Vector3.Cross(_camera.Vooruit, Vector3.UnitY)) * snelheid;
    if (input.IsKeyDown(Keys.Escape)) Close();

    // Muis (rondkijken)
    var muis = MouseState;
    if (_vorigeMuisPositie == Vector2.Zero) {
      _vorigeMuisPositie = new Vector2(muis.X, muis.Y);
    } else {
      float deltaX = muis.X - _vorigeMuisPositie.X;
      float deltaY = muis.Y - _vorigeMuisPositie.Y;
      _vorigeMuisPositie = new Vector2(muis.X, muis.Y);

      _camera.Yaw += deltaX * gevoeligheid;
      _camera.Pitch -= deltaY * gevoeligheid; // Min omdat Y-as omhoog gaat
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