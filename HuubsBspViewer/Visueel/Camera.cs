using OpenTK.Mathematics;
using System.Drawing;

public class Camera {
  // ===== Instellingen =====
  public float Snelheid = 800.0f;
  public float MuisGevoeligheid = 0.0025f;
  public float GezichtsVeldFieldOfView = 75;

  // ===== Stand =====
  private Vector3 _positie;
  private float _rechtsLinks;
  private float _omhoogOmlaag;

  // Basisvectoren (Quake 2)
  private Vector3 _vooruit;
  private Vector3 _rechts;
  private Vector3 _omhoog;

  public float AspectRatio { get; set; }
  public Vector3 Vooruit {
    get => _vooruit;
    set => _vooruit = value;
  }
  public Vector3 Positie {
    get => _positie;
    set => _positie = value;
  }
  public Vector3 Rechts {
    get => _rechts;
    set => _rechts = value;
  }
  public float RechtsLinks {
    get => _rechtsLinks;
    set => _rechtsLinks = value;
  }
  public float OmhoogOmlaag {
    get => _omhoogOmlaag;
    set => _omhoogOmlaag = value;
  }

  public Camera(Vector3 startPos, float aspectRatio) {
    AspectRatio = aspectRatio;
    _positie = startPos;

    // Quake2: X voorwaarts, Z omhoog
    _rechtsLinks = MathHelper.DegreesToRadians(DiverseHelpFuncties.deStartHoek);
    _omhoogOmlaag = 0.0f;

    Console.WriteLine($"Camera op positie: {_positie} _rechtsLinks: {_rechtsLinks} deStartHoek: {DiverseHelpFuncties.deStartHoek}");

    UpdateVectoren(); // !!! cruciaal bij start
  }

  // ===============================
  // ===== Vector update logic =====
  // ===============================
  public void UpdateVectoren() {
    // Forward (X = forward, Y = right, Z = up)
    _vooruit.X = MathF.Cos(_omhoogOmlaag) * MathF.Cos(_rechtsLinks);
    _vooruit.Y = MathF.Cos(_omhoogOmlaag) * MathF.Sin(_rechtsLinks);
    _vooruit.Z = MathF.Sin(_omhoogOmlaag);
    
    _vooruit = Vector3.Normalize(_vooruit);

    // Right & Up
    _rechts = Vector3.Normalize(Vector3.Cross(_vooruit, Vector3.UnitZ));
    _omhoog = Vector3.Normalize(Vector3.Cross(_rechts, _vooruit));
  }
  public void VerwerkMuis(float deltaX, float deltaY) {
    _rechtsLinks -= deltaX * MuisGevoeligheid;
    _omhoogOmlaag -= deltaY * MuisGevoeligheid;

    _omhoogOmlaag = MathHelper.Clamp(_omhoogOmlaag,
        -MathHelper.PiOver2 + 0.01f,
         MathHelper.PiOver2 - 0.01f);

    UpdateVectoren();
  }

  // ===============================
  // ===== Matrices =====
  // ===============================
  public Matrix4 GetViewMatrix() {
    return Matrix4.LookAt(
        _positie,
        _positie + _vooruit,
        _omhoog
    );
  }

  public Matrix4 GetProjectionMatrix() {
    //var x = Matrix4.CreatePerspectiveFieldOfView(
    //    MathHelper.DegreesToRadians( GezichtsVeldFieldOfView),
    //    AspectRatio,
    //    0.1f,
    //    20000.0f);

    var x = Matrix4.CreatePerspectiveFieldOfView(
        MathHelper.DegreesToRadians(90f),
        //Size.X / (float)Size.Y
        AspectRatio
       // 1.0f
        , 0.1f, 1000f);
    
        return x;
  }
}
