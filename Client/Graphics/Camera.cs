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
            // Offset by the camera's position
            Matrix4x4.CreateTranslation(-Position.X, -Position.Y, 0)
            // Center the camera (Vulkan's origin is in the center of the screen)
            * Matrix4x4.CreateTranslation(-0.5f, -0.5f, 0f)
            // Convert from screen coords to world coords
            * Matrix4x4.CreateScale(new Vector3(2.0f / Size.X * 96, 2.0f / Size.Y * 96, 0), Vector3.Zero)
            // Zooms out camera
            * Matrix4x4.CreateScale(new Vector3(Zoom, Zoom, 0), Vector3.Zero);
    }
}