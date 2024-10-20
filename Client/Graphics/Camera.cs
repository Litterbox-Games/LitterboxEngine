using System.Numerics;

namespace Client.Graphics;

public class Camera
{
    public Matrix4x4 ViewMatrix { get; private set; }

    public Vector2 Position;
    public Vector2 Size;
    public float Zoom = 1f;
    public readonly float NearPlane = -1;
    public readonly float FarPlane = 1;
    
    public Camera(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;

        ViewMatrix = CalculateViewMatrix();
    }

    public void Update()
    {
        ViewMatrix = CalculateViewMatrix();
    }


    private Matrix4x4 CalculateViewMatrix()
    {
        // Order: Scale -> Translate -> Orthographic
        // This ensures proper scaling around the camera position
        return 
            Matrix4x4.CreateScale(new Vector3(Zoom, Zoom, 1.0f)) *
            Matrix4x4.CreateTranslation(-Position.X + Size.X / 2f, -Position.Y + Size.Y / 2f, 0) *
            Matrix4x4.CreateOrthographic(Size.X, Size.Y, NearPlane, FarPlane);
    }
}