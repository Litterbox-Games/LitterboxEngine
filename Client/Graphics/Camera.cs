using System.Numerics;

namespace Client.Graphics;

public class Camera
{
    public Matrix4x4 ViewMatrix { get; private set; }

    public Vector2 Position;
    public Vector2 Size;
    public float Zoom = 96f;

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
            Matrix4x4.CreateTranslation(-Position.X, -Position.Y, 0) *
            Matrix4x4.CreateOrthographic( Size.X,  Size.Y, -1, 1);
        
        /*return
            Matrix4x4.CreateTranslation(-Position.X, -Position.Y, 0) *
            Matrix4x4.CreateTranslation(-10f, -5.625f, 0f) *
            Matrix4x4.CreateOrthographic( Size.X / (Size.X / 20),  Size.Y / (Size.X / 20), -1, 1);*/
        return
            // Offset by the camera's position
            // Matrix4x4.CreateTranslation(-Position.X, -Position.Y, 0)
            // Center the camera (Vulkan's origin is in the center of the screen)
            // * Matrix4x4.CreateTranslation(-10f, -5f, 0f)
            Matrix4x4.CreateTranslation(-Position.X - 10, -Position.Y - 5.625f, 0)
            // Convert from screen coords to world coords
            * Matrix4x4.CreateScale(new Vector3(2.0f / Size.X * 96, 2.0f / Size.Y * 96, 0), Vector3.Zero);
        // Zooms out camera
        //* Matrix4x4.CreateScale(new Vector3(Zoom, Zoom, 0), Vector3.Zero);
    }
}