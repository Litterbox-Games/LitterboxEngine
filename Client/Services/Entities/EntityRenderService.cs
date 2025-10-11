using System.Drawing;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Utils;
using Client.Graphics;
using Client.Services.Resource;
using Common.Components;
using Common.Core;
using Common.Mathematics;
using Common.Services.Entities;
using Common.Services.Entities.Events;
using Common.Services.Events;
using Common.Services.Players;
using Common.Services.Resource;
using Common.Services.World;
using ImGuiNET;

namespace Client.Services.Entities;

public class EntityRenderService: IService, IDrawable, IGuiDrawable
{
    private readonly QueryDescription _movable = new QueryDescription().WithAll<Networked, Position>();
    
    private readonly IEntityService _entityService;
    private readonly IPlayerService _playerService;
    private readonly IResourceService _resourceService;
    private readonly EventService _eventService;
    
    private Entity? _playerEntity;
    private readonly Rectangle _textureSource = new(32, 112, 20, 16);
    
    public EntityRenderService(IEntityService entityService, IPlayerService playerService, IResourceService resourceService, EventService eventService)
    {
        _entityService = entityService;
        _playerService = playerService;
        _resourceService = resourceService;
        _eventService = eventService;

        _eventService.Handle<EntityCreatedEvent>(OnEntityCreated);
        _eventService.Handle<EntityDestroyedEvent>(OnEntityDestroyed);
    }
    
    private void OnEntityCreated(EntityCreatedEvent e)
    {
        if (!e.Entity.Has<Networked, Player>()) return;
        var networked = e.Entity.Get<Networked>();
        if (networked.OwnerId == _playerService.PlayerId) _playerEntity = e.Entity;
    }
    
    private void OnEntityDestroyed(EntityDestroyedEvent e)
    {
        if (!e.Entity.Has<Networked, Player>()) return;
        var networked = e.Entity.Get<Networked>();
        if (networked.OwnerId == _playerService.PlayerId) _playerEntity = null;       
    }
    
    public void Draw(float _, RendererService renderer)
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
            // if (position.Queued.Count > 1)
            // { // this is a player
            //     var queued = position.Queued.ToArray();
            //     renderer.DrawTexture(texture, _textureSource, new RectangleF(queued[0].Position.X, queued[0].Position.Y, 1.25f, 1), Color.Green);
            //     renderer.DrawTexture(texture, _textureSource, new RectangleF(queued[1].Position.X, queued[1].Position.Y, 1.25f, 1), Color.Red);
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
    
    public void DrawGui(float deltaTime, RendererService renderer)
    {
        var font = _resourceService.Get<Font>("Fonts/dogica.otf");
        
        renderer.DrawRectangle(new RectangleF(new Vector4(0,0,800,150)), Color.Coral);
        renderer.DrawText("Hello GUI!", font, new Vector2(16, 16), 16, 16, Color.Black);
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
    
    public void Dispose()
    {
        _eventService.Unhandle<EntityCreatedEvent>(OnEntityCreated);
        _eventService.Unhandle<EntityDestroyedEvent>(OnEntityDestroyed);
        
        GC.SuppressFinalize(this);
    }
}