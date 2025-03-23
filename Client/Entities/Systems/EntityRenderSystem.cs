using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Utils;
using Client.Graphics;
using Client.Resource;
using Common.Components;
using Common.Entities;
using Common.Entities.Components;
using Common.Mathematics;
using Common.Players;
using Common.Resource;
using Common.Systems;
using Common.World;
using ImGuiNET;

namespace Client.Entities.Systems;

public class EntityRenderSystem: ISystem, IDrawable
{
    private readonly QueryDescription _movable = new QueryDescription().WithAll<Networked, Position>();
    
    private readonly IEntityService _entityService;
    private readonly IPlayerService _playerService;
    private readonly IResourceService _resourceService;
    
    private Entity? _playerEntity;
    private readonly Rectangle _textureSource = new(32, 112, 20, 16);
    
    public EntityRenderSystem(IEntityService entityService, IPlayerService playerService, IResourceService resourceService)
    {
        _entityService = entityService;
        _playerService = playerService;
        _resourceService = resourceService;
        entityService.EventOnEntitySpawn += OnEntitySpawn;
        entityService.EventOnEntityDespawn += OnEntityDespawn;
    }

    private void OnEntitySpawn(Entity entity)
    {
        if (!entity.Has<Networked, Player>()) return;
        var networked = entity.Get<Networked>();
        if (networked.OwnerId == _playerService.PlayerId) _playerEntity = entity;
    }
    
    private void OnEntityDespawn(Entity entity)
    {
        if (!entity.Has<Networked, Player>()) return;
        var networked = entity.Get<Networked>();
        if (networked.OwnerId == _playerService.PlayerId) _playerEntity = null;       
    }
    
    public void Draw(Renderer renderer)
    {
        if (_playerEntity == null) return;

        var texture = _resourceService.Get<Aseprite>("Aseprites/Player.aseprite").Texture;

        var player = _playerEntity.Value.Get<Position>();
        
        _entityService.Entities.Query(in _movable, ( 
            ref Networked network, 
            ref Position position
        ) => {
            const int worldSize = IWorldService.WorldSize * ChunkData.ChunkSize;
            
            var renderPosition = (position.Current.Modulus(worldSize) -  player.Current + new Vector2(worldSize / 2f)).Modulus(worldSize) - new Vector2(worldSize / 2f) + player.Current;
            
            // Debug draw for showing network positions vs render position (not world wrapping atm)
            // if (entity.EntityType == 0 && entity.QueuedMovements.Count > 1)
            // { // this is a player
            //     var firstMovement = entity.QueuedMovements.ToArray()[0];
            //     _rendererService.DrawTexture(_texture, _textureSource, new RectangleF(firstMovement.Position.X, firstMovement.Position.Y, 1.25f, 1), Color.Green);
            //     
            //     var secondMovement = entity.QueuedMovements.ToArray()[1];
            //     _rendererService.DrawTexture(_texture, _textureSource, new RectangleF(secondMovement.Position.X, secondMovement.Position.Y, 1.25f, 1), Color.Red);
            // }
            
            renderer.DrawTexture(texture, _textureSource, new RectangleF(renderPosition.X, renderPosition.Y, 1.25f, 1), Color.White);
        });
     
        
        ImGui.SetNextWindowSizeConstraints(new Vector2(400, 200), new Vector2(400, 700));
        ImGui.Begin("EntityService");

        _entityService.Entities.Query(new QueryDescription(), (Entity entity) =>
        {
            ImGui.PushID(entity.Id);
            
            if (ImGui.CollapsingHeader($"{entity.Id}"))
            {
                var components = entity.GetAllComponents();
            
                foreach (var component in components)
                {
                    if (component == null) continue;
                
                    ComponentRegistry.TryGet(component.GetType(), out var componentType);
                
                    ImGui.Indent(30);
                    ImGui.PushID(componentType.Id);
                    
                    if (ImGui.CollapsingHeader(componentType.Type.Name))
                    {
                        ImGui.Indent(30);
                        ImGui.PushID("Widget");
                        var componentRef = entity.Get(componentType);
                        if (componentRef == null) continue;
                        DrawComponent(ref componentRef);
                        entity.Set(componentRef);
                        ImGui.PopID();
                        ImGui.Unindent(30);
                    }
                    
                    ImGui.PopID();
                    ImGui.Unindent(30);
                }
            }
            
            ImGui.PopID();
        });
        
        ImGui.End();
    }
    
    void DrawComponent(ref object obj)
    {
        var type = obj.GetType();
        var fields = type.GetFields();

        foreach (var field in fields)
        {
            ImGui.PushID(field.Name);

            var value = field.GetValue(obj);

            if (value == null)
                continue;

            var fieldType = value.GetType();

            if (fieldType.IsEnum)
            {
                var enumValues = Enum.GetValues(fieldType);
                var enumNames = Enum.GetNames(fieldType);

                var selectedIndex = Array.IndexOf(enumNames, value.ToString());

                if (ImGui.Combo(field.Name, ref selectedIndex, enumNames, enumNames.Length))
                {
                    var selectedValue = enumValues.GetValue(selectedIndex);
                    field.SetValue(obj, selectedValue);
                }
            }
            else if (fieldType.IsPrimitive || fieldType == typeof(string))
            {
                switch (value)
                {
                    case nint i:
                        var pointer = i.ToString();
                        ImGui.InputText(field.Name, ref pointer, 64);
                        break;
                    case int i:
                        ImGui.InputInt(field.Name, ref i);
                        field.SetValue(obj, i);
                        break;
                    case float f:
                        ImGui.InputFloat(field.Name, ref f);
                        field.SetValue(obj, f);
                        break;
                    case double d:
                        ImGui.InputDouble(field.Name, ref d);
                        field.SetValue(obj, d);
                        break;
                    case bool b:
                        ImGui.Checkbox(field.Name, ref b);
                        field.SetValue(obj, b);
                        break;
                    case string s:
                        ImGui.InputText(field.Name, ref s, 512);
                        field.SetValue(obj, s);
                        break;
                }
            }
            else if (value is DateTime dt)
            {
                ImGui.Text(field.Name);
                ImGui.Text(dt.ToLongTimeString());
            }
            else
            {
                ImGui.BeginGroup();
                ImGui.Text(field.Name);
                DrawComponent(ref value);
                field.SetValue(obj, value);
                ImGui.EndGroup();
            }

            ImGui.PopID();
        }
    }
}