using System.Numerics;
using Common.DI;

namespace Client.Graphics.Input;

// TODO: This should be a tickable service with highest priority!!!!!
public interface IMouseService: IService
{
    public Vector2 CurrentPosition { get; }
    public Vector2 PreviousPosition { get; }
    public Vector2 Displacement { get; }
    public bool InWindow { get; }
    public bool IsLeftButtonPressed { get; }
    public bool IsRightButtonPressed { get; }
}