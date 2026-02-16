using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

public class TekenLijnBlokken {
  // Houd een referentie naar de delegate vast om te voorkomen dat de Garbage Collector hem opruimt
  private static DebugProc _debugProcCallback = DiverseHelpFuncties.HuubDebugCallback;

  private int _vaoBlokje;
  private int _vboBlokje;

  AlgemeneShader _shader;
  /**
   * Constructor gemaakt in venster waar ook de shader is geladen
   */
  public TekenLijnBlokken(AlgemeneShader shader) {
    _shader = shader;

    //// 2. Setup Debug Blokjes (VAO & VBO)
    _vaoBlokje = GL.GenVertexArray();
    _vboBlokje = GL.GenBuffer();

    GL.Enable(EnableCap.DebugOutput);
    GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);

    GL.BindVertexArray(_vaoBlokje);
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vboBlokje);

    DiverseHelpFuncties.ControleerGLFout("TekenLijnBlokken constructor");

    // Reserveer ruimte voor 24 Vector3 punten (12 lijnen voor een kubus)
    GL.BufferData(BufferTarget.ArrayBuffer, 24 * Vector3.SizeInBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);

    // Koppel de shader-attribuut 'aPos' aan deze VAO
    int posLoc = GL.GetAttribLocation(_shader.Handle, "aPos");
    if(posLoc != -1) {
      GL.EnableVertexAttribArray(posLoc);
      GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 0, 0);
    }

    // Ontkoppel de VAO zodat andere teken-functies er geen last van hebben
    GL.BindVertexArray(0);
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
  }
  /**
   * Teken lijnen voor een blokje rond de entiteit
   */
  public void TekenLijnBlokjes(AlgemeneShader _shader, List<EntiteitAlgemeen> entiteiten, Camera camera) {
    // TEKEN HIER DE TRIGGERS
    // Optie A: Teken ALTIJD alle triggers (handig voor debuggen)

    Vector3 actieveTrigger = camera.Positie;

    foreach(EntiteitAlgemeen entiteit in entiteiten.Where(x => x.ClassNaam.StartsWith("trigger"))) {

      Vector3 kleurLijn = new Vector3(0, 1, 1);
      kleurLijn = DiverseHelpFuncties.KleurVanClassNaam(entiteit.ClassNaam);

      if(entiteit.Positie == actieveTrigger) {
        kleurLijn = new Vector3(1, 1, 1); // Wit als we erin staan
      }
      TekenLijnBlokje(entiteit, kleurLijn);
    }
  }
  /**
   * Teken 1 blokje in verschillende kleuren afhankelijk van de ClassNaam:
   *  target_speaker    : Rood
      trigger_always    : Groen
      trigger_multiple  : Blauw
      trigger_once      : Geel
      trigger_relay     : Magenta
   */
  public void TekenLijnBlokje(EntiteitAlgemeen entiteit, Vector3 kleur) {
    _shader.Gebruiken();

    // 1. Update de data (BufferData dwingt een 'verse' buffer af bij de driver)
    GL.BindBuffer(BufferTarget.ArrayBuffer, _vboBlokje);

    Vector3 offset = (0, 0, 0); //entiteit.Positie;
    Vector3 min = entiteit.MinimumCoordinaat + offset;
    Vector3 max = entiteit.MaximumCoordinaat + offset;

    DiverseHelpFuncties.ControleerGLFout("TekenLijnBlokje");

    // Reserveer ruimte voor 24 Vector3 punten (12 lijnen voor een kubus)
    GL.BufferData(BufferTarget.ArrayBuffer, 24 * Vector3.SizeInBytes, IntPtr.Zero, BufferUsageHint.DynamicDraw);

    // Maak kleine punten zichtbaar als een 16x16x16 box
    if(min == max) {
      min -= Vector3.One * 8;
      max += Vector3.One * 8;
    } else {
      // Voeg een kleine marge toe zodat de lijnen niet precies op de wanden zitten
      Vector3 marge = new Vector3(1.0f, 1.0f, 1.0f);
      min -= marge;
      max += marge;
    }

    Vector3[] lijnen = {
        new(min.X, min.Y, min.Z), new(max.X, min.Y, min.Z),
        new(max.X, min.Y, min.Z), new(max.X, max.Y, min.Z),
        new(max.X, max.Y, min.Z), new(min.X, max.Y, min.Z),
        new(min.X, max.Y, min.Z), new(min.X, min.Y, min.Z),
        new(min.X, min.Y, max.Z), new(max.X, min.Y, max.Z),
        new(max.X, min.Y, max.Z), new(max.X, max.Y, max.Z),
        new(max.X, max.Y, max.Z), new(min.X, max.Y, max.Z),
        new(min.X, max.Y, max.Z), new(min.X, min.Y, max.Z),
        new(min.X, min.Y, min.Z), new(min.X, min.Y, max.Z),
        new(max.X, min.Y, min.Z), new(max.X, min.Y, max.Z),
        new(max.X, max.Y, min.Z), new(max.X, max.Y, max.Z),
        new(min.X, max.Y, min.Z), new(min.X, max.Y, max.Z)
    };

    DiverseHelpFuncties.ControleerGLFout("TekenLijnBlokje");

    GL.BufferData(BufferTarget.ArrayBuffer, lijnen.Length * Vector3.SizeInBytes, lijnen, BufferUsageHint.StreamDraw);

    // 2. Bind de VAO (hiermee staan de pointers naar aPos direct goed)
    GL.BindVertexArray(_vaoBlokje);

    // 3. Matrices
    //_shader.SetMatrix4("projection", _camera.GetProjectionMatrix()); // TODO: nodig?
    //_shader.SetMatrix4("view", _camera.GetViewMatrix());
    //_shader.SetMatrix4("model", Matrix4.Identity); // TODO gaf een foutmelding toen dit er stond

    // 4. Shader uniforms
    Color4 kleur4 = new Color4(kleur.X, kleur.Y, kleur.Z, 1.0f); // RGB en transparantie

    GL.Uniform1(GL.GetUniformLocation(_shader.Handle, "debugMode"), 1);
    GL.Uniform4(GL.GetUniformLocation(_shader.Handle, "debugColor"), kleur4);
    GL.Uniform1(GL.GetUniformLocation(_shader.Handle, "isMarker"), 0);

    // 5. Tekenen
    GL.LineWidth(1.0f);
    //GL.Disable(EnableCap.DepthTest); // X-Ray om ze sowieso te vinden
    GL.DrawArrays(PrimitiveType.Lines, 0, 24);
    //GL.Enable(EnableCap.DepthTest);

    // 6. Reset
    // Ontkoppel de VAO zodat andere teken-functies er geen last van hebben
    GL.BindVertexArray(0);
    GL.Uniform1(GL.GetUniformLocation(_shader.Handle, "debugMode"), 0);
  }
}