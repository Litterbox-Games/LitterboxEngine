using Client.Graphics;
using Common.Core;
using ImGuiNET;

namespace Client.Services.UI;

[Engine]
public class MainMenuService: IService, IDrawable
{

    private readonly GameLoopService _gameLoop;  
    
    public MainMenuService(GameLoopService gameLoop)
    {
        _gameLoop = gameLoop;
    }

    public void Draw(float deltaTime, RendererService renderer)
    {
        if (_gameLoop.IsGameRunning()) return;
        
        ImGui.Begin("Main Menu");
                
        if (ImGui.Button("Single Player"))
            _gameLoop.StartGame(EMode.Host, false);
        
                
        if (ImGui.Button("Local Host"))
            _gameLoop.StartGame(EMode.Host, true);
        
                
        if (ImGui.Button("Client"))
            _gameLoop.StartGame(EMode.Client, true);
        
                
        ImGui.End();
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}