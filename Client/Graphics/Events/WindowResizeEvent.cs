using Common.Services.Events;

namespace Client.Graphics.Events;

public struct WindowResizeEvent(int width, int height) : IEvent
{
    public readonly int Width = width;
    public readonly int Height = height;
}