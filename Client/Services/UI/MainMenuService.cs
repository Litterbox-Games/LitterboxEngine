using Client.Graphics;
using Client.Host;
using Common.Core;
using ImGuiNET;

namespace Client.Services.UI;

public class MainMenuService: IService, IDrawable
{

    private readonly GameLoopService _gameLoop;  
    
    public MainMenuService(GameLoopService gameLoop)
    {
        _gameLoop = gameLoop;
    }

    public void Draw(float deltaTime, RendererService renderer)
    {
        ImGui.Begin("Main Menu");
                
        if (ImGui.Button("Single Player"))
        {
            _gameLoop.StartGame(new SinglePlayerHost());
        }
                
        if (ImGui.Button("Local Host"))
        {
            _gameLoop.StartGame(new LocalHost());
        }
                
        if (ImGui.Button("Client"))
        {
            _gameLoop.StartGame(new ClientHost());
        }
                
        ImGui.End();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}