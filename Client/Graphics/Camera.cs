using System.Numerics;

namespace Client.Graphics;

public class Camera
{
    public Matrix4x4 ViewMatrix { get; private set; }

    public Vector2 Position;
    public Vector2 Size;
    public float Zoom = 1f;

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
        return
            Matrix4x4.CreateTranslation(-Position.X + Size.X / 2f, -Position.Y + Size.Y / 2f, 0) *
            Matrix4x4.CreateOrthographic(Size.X, Size.Y, -1, 1) * 
            Matrix4x4.CreateScale(new Vector3(Zoom, Zoom, 0), Vector3.Zero);
    }
}