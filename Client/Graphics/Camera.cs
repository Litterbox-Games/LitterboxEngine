using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Client.Graphics;

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