using System.Runtime.InteropServices;
using Common.Services.Logging;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Client.Graphics.Backend.Vulkan;

public class VulkanInstance: IDisposable
{
    private readonly ILoggingService? _logger;
    
    private readonly Vk _vk;
    public readonly Instance VkInstance;
    
    private static readonly string[] RequestedExtensions =
    {
        // This extension is required to work with the window surface created by GLFW.
        "VK_KHR_surface",
        "VK_KHR_win32_surface",
#if MACOS
        // Tells vulkan to complain about the use of features non-compliant for portability
        "VK_KHR_portability_enumeration"
#endif
#if DEBUG        
        "VK_EXT_debug_utils"
#endif
    };
    
    private static readonly string[] RequestedValidationLayers = 
    {
#if DEBUG
        "VK_LAYER_KHRONOS_validation"
#endif
    };
    
    public unsafe VulkanInstance(Vk vk, string applicationName, ILoggingService? logger = null)
    {
        _logger = logger;
        _vk = vk;

        ApplicationInfo applicationCreateInfo = new() 
        {
            SType = StructureType.ApplicationInfo,
            ApiVersion = Vk.Version10,
            ApplicationVersion = new Version32(0, 1, 0),
            EngineVersion = new Version32(0, 1, 0),
            PApplicationName = (byte*) Marshal.StringToHGlobalAnsi(applicationName),
            PEngineName = (byte*) Marshal.StringToHGlobalAnsi("Litterbox Engine")
        };

        

#if DEBUG
        var validationLayers = GetSupportedValidationLayers();
        
        if (validationLayers.Length == 0)
        {
            // TODO: should probably throw an exception here instead
            _logger?.Warning("Vulkan validation layers requested but failed to find a supported validation layer");
        }
        _logger?.Debug("Vulkan validation on.");
#endif
        
        var extensions = GetInstanceExtensions();

        var instanceCreateInfo = new InstanceCreateInfo
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &applicationCreateInfo,
            EnabledExtensionCount = (uint) extensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions),
            EnabledLayerCount = 0,
            PpEnabledLayerNames = null
        };
        
#if DEBUG
        DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new()
        {
            SType = StructureType.DebugUtilsMessengerCreateInfoExt,
            MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt | 
                              DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
            MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                          DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                          DebugUtilsMessageTypeFlagsEXT.ValidationBitExt,
            PfnUserCallback = (PfnDebugUtilsMessengerCallbackEXT)DebugCallback
        };
        
        instanceCreateInfo.EnabledLayerCount = (uint) validationLayers.Length;
        instanceCreateInfo.PpEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(validationLayers);
        // PNext is used to extend instance creation data for use with VK extensions, like the debug callback extension.
        instanceCreateInfo.PNext = &debugCreateInfo;
#endif
        
        var result = _vk.CreateInstance(in instanceCreateInfo, null, out VkInstance);
        
        // Native strings are mem-copied by Vulkan and can be freed after instance creation.
        Marshal.FreeHGlobal((nint)applicationCreateInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)applicationCreateInfo.PEngineName);
        SilkMarshal.Free((nint)instanceCreateInfo.PpEnabledExtensionNames);
#if DEBUG
       SilkMarshal.Free((nint)instanceCreateInfo.PpEnabledLayerNames);
#endif
        if (result != Result.Success)
            throw new Exception($"Failed to create Vulkan instance. Instance creation returned {result.ToString()}.");
    }

    private unsafe string[] GetInstanceExtensions()
    {
        uint layersCount = 0;
        
        var result = _vk.EnumerateInstanceExtensionProperties((string)null!, ref layersCount, null);
        if (result != Result.Success)
            throw new Exception($"Failed to enumerate instance extension properties with error: {result.ToString()}");
        
        Span<ExtensionProperties> extensions = new ExtensionProperties[layersCount];
        result = _vk.EnumerateInstanceExtensionProperties((string)null!, &layersCount, extensions); 
        if (result != Result.Success)
            throw new Exception($"Failed to enumerate instance extension properties with error: {result.ToString()}");

        return extensions.ToArray()
            .Select(ext =>
            {
                // ReSharper disable once ConvertToLambdaExpression
                return SilkMarshal.PtrToString((nint) ext.ExtensionName)!;
            })
            .Where(name => RequestedExtensions.Contains(name))
            .ToArray();
    }
    
    private unsafe string[] GetSupportedValidationLayers()
    {
        uint layersCount = 0;
        
        var result = _vk.EnumerateInstanceLayerProperties(ref layersCount, null);
        if (result != Result.Success)
            throw new Exception($"Failed to enumerate instance layer properties with error: {result.ToString()}");

        Span<LayerProperties> layers = new LayerProperties[layersCount];
        result = _vk.EnumerateInstanceLayerProperties(&layersCount, layers); 
        if (result != Result.Success)
            throw new Exception($"Failed to enumerate instance layer properties with error: {result.ToString()}");

        return layers.ToArray()
            .Select(p =>
            {
                // ReSharper disable once ConvertToLambdaExpression
                return SilkMarshal.PtrToString((nint) p.LayerName)!;
            })
            .Where(name => RequestedValidationLayers.Contains(name))
            .ToArray();
    }
    
    private unsafe uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
    {
        // Do not need to release this string like the others as Vulkan will release the memory automatically.
        _logger?.Debug(Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage)!);

        return Vk.False;
    }

    public unsafe void Dispose()
    {
        _vk.DestroyInstance(VkInstance, null);
        GC.SuppressFinalize(this);
    }
}