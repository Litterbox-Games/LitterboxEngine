using System.Numerics;

namespace Client.Graphics;

/*
public class Camera
{
    public Matrix4x4 ViewMatrix { get; private set; }

    public Vector2 Position;
    
    public void Update()
    {
        CalculateViewMatrix();
    }

    private void CalculateViewMatrix()
    {
        ViewMatrix = Matrix4x4.CreateTranslation(-Position.X, -Position.Y, 0);
    }
}
*/

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
        return Matrix4x4.CreateScale(1, 1, 1) *
               Matrix4x4.CreateTranslation(-Position.X - Size.X / 2f, -Position.Y - Size.Y / 2f, 0f) *
               Matrix4x4.CreateOrthographic(Size.X * Zoom, Size.Y * Zoom, -1, 1);
    }
}