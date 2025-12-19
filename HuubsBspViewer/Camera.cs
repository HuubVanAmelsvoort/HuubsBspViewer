using OpenTK.Mathematics;
using System;

public class Camera {
  public Vector3 Positie;
  public float AspectRatio;

  private Vector3 _vooruit = -Vector3.UnitZ;
  private Vector3 _omhoog = Vector3.UnitY;
  private Vector3 _rechts = Vector3.UnitX;

  private float _pitch;
  private float _yaw = -MathHelper.PiOver2; // Kijk standaard naar voren

  public Camera(Vector3 startPositie, float aspect) {
    Positie = startPositie;
    AspectRatio = aspect;
  }

  public float Pitch {
    get => MathHelper.RadiansToDegrees(_pitch);
    set {
      var hoek = MathHelper.Clamp(value, -89f, 89f);
      _pitch = MathHelper.DegreesToRadians(hoek);
      UpdateVectoren();
    }
  }

  public float Yaw {
    get => MathHelper.RadiansToDegrees(_yaw);
    set {
      _yaw = MathHelper.DegreesToRadians(value);
      UpdateVectoren();
    }
  }

  public Vector3 Vooruit => _vooruit;

  public Matrix4 GetViewMatrix() => Matrix4.LookAt(Positie, Positie + _vooruit, _omhoog);
  public Matrix4 GetProjectionMatrix() => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60), AspectRatio, 1.0f, 10000.0f);

  private void UpdateVectoren() {
    _vooruit.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
    _vooruit.Y = MathF.Sin(_pitch);
    _vooruit.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
    _vooruit = Vector3.Normalize(_vooruit);

    _rechts = Vector3.Normalize(Vector3.Cross(_vooruit, Vector3.UnitY));
    _omhoog = Vector3.Normalize(Vector3.Cross(_rechts, _vooruit));
  }
}