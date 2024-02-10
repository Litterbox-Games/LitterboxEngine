using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace LitterboxEngine;

public class Renderer : IDisposable
{
    // TODO: Stop passing fields as params and just reference them directly
    
    private const int MaxFramesInFlight = 2;
    
    private readonly Instance _instance;
    private readonly Vk _vk;
    private readonly Window _window;
    private readonly PhysicalDevice _physicalDevice;
    private readonly uint _queueFamilyIndex;
    private readonly Device _logicalDevice;
    private readonly Queue _queue;
    private readonly SurfaceKHR _surface;
    private KhrSwapchain _khrSwapchain;
    private SwapchainKHR _swapchain;
    private readonly KhrSurface _khrSurface;
    private Format _swapchainFormat;
    private ImageView[] _imageViews;
    private Image[] _images;
    private Extent2D _swapchainExtent;
    private PipelineLayout _pipelineLayout;
    private Pipeline _pipeline;
    private RenderPass _renderPass;
    private Framebuffer[] _framebuffers;
    private readonly CommandPool _commandPool;
    private readonly Semaphore[] _imageAvailableSemaphores;
    private readonly Semaphore[] _renderFinishedSemaphores;
    private readonly Fence[] _inFlightFences;
    private Fence[] _imagesInFlight;
    private CommandBuffer[] _commandBuffers;
    private readonly Buffer _vertexBuffer;
    private readonly DeviceMemory _vertexBufferMemory;

    private int _currentFrame;
    private bool _frameBufferResized;
    
    private readonly Vertex[] _vertices =
    {
        new() { pos = new Vector2D<float>(0.0f,-0.5f), color = new Vector3D<float>(1.0f, 0.0f, 0.0f) },
        new() { pos = new Vector2D<float>(0.5f,0.5f), color = new Vector3D<float>(0.0f, 1.0f, 0.0f) },
        new() { pos = new Vector2D<float>(-0.5f,0.5f), color = new Vector3D<float>(0.0f, 0.0f, 1.0f) }
    };

    public unsafe Renderer(Window window, string[] extensions, string[]? validationLayers = null, DebugUtilsMessengerCallbackFunctionEXT? debugCallback = null)
    {
        _vk = Vk.GetApi();
        _window = window;
        _window.SetFrameBufferResizeCallback((_,_,_) => _frameBufferResized = true);
        
        _instance = CreateVulkanInstance(_window.Title, extensions, validationLayers, debugCallback);
        _physicalDevice = SelectPhysicalDevice();
        _queueFamilyIndex = SelectGraphicsQueueFamily();
        (_logicalDevice, _queue) = CreateLogicalDevice();
        (_surface, _khrSurface) = CreateSurface();
        (_swapchain, _khrSwapchain, _swapchainFormat, _swapchainExtent) = CreateSwapchain();    
        (_images, _imageViews) = CreateImageViews(_khrSwapchain, _swapchain, _swapchainFormat); 
        _renderPass = CreateRenderPass(_swapchainFormat);                                       
        (_pipelineLayout, _pipeline) = CreateGraphicsPipeline(_renderPass, _swapchainExtent);   
        _framebuffers = CreateFramebuffers(_swapchainExtent, _imageViews, _renderPass);         
        _commandPool = CreateCommandPool();
        (_vertexBuffer, _vertexBufferMemory) = CreateVertexBuffer();
        _commandBuffers = CreateCommandBuffers(_swapchainExtent, _renderPass, _pipeline, _framebuffers); 
        (_imageAvailableSemaphores, _renderFinishedSemaphores, _inFlightFences, _imagesInFlight) = CreateSyncObjects(_images);
    }
    
    private unsafe Instance CreateVulkanInstance(string applicationName, string[] extensions, string[]? validationLayers = null, DebugUtilsMessengerCallbackFunctionEXT? debugCallback = null)
    {
        validationLayers ??= Array.Empty<string>();
        
        // Create Vulkan Instance
        var applicationCreateInfo = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            ApiVersion = Vk.Version10,
            ApplicationVersion = new Version32(0, 1, 0),
            EngineVersion = new Version32(0, 1, 0),
            PApplicationName = (byte*) Marshal.StringToHGlobalAnsi(applicationName),
            PEngineName = (byte*) Marshal.StringToHGlobalAnsi("Litterbox Engine")
        };
        
        DebugUtilsMessengerCreateInfoEXT? debugCreateInfo = debugCallback == null ? null : new DebugUtilsMessengerCreateInfoEXT
        {
            SType = StructureType.DebugUtilsMessengerCreateInfoExt,
            MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt | 
                              DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
            MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                          DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                          DebugUtilsMessageTypeFlagsEXT.ValidationBitExt,
            PfnUserCallback = debugCallback
        };
        
        var instanceCreateInfo = new InstanceCreateInfo
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &applicationCreateInfo,
            EnabledExtensionCount = (uint) extensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions),
            EnabledLayerCount = 0,
            PpEnabledLayerNames = null
        };
        
        if (debugCreateInfo != null)
        {
            var info = debugCreateInfo.Value;
            instanceCreateInfo.EnabledLayerCount = (uint) validationLayers.Length;
            instanceCreateInfo.PpEnabledLayerNames = (byte**) SilkMarshal.StringArrayToPtr(validationLayers);
            // PNext is used to extend instance creation data for use with VK extensions, like the debug callback extension.
            instanceCreateInfo.PNext = &info;
        }
        
        var result = _vk.CreateInstance(instanceCreateInfo, null, out var instance);
        
        Marshal.FreeHGlobal((nint)applicationCreateInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)applicationCreateInfo.PEngineName);
        SilkMarshal.Free((nint)instanceCreateInfo.PpEnabledExtensionNames);
        
        if (debugCreateInfo != null)
        {
            // Native strings are mem-copied by Vulkan and can be freed after instance creation.
            SilkMarshal.Free((nint)instanceCreateInfo.PpEnabledLayerNames);
        }
        
        if (result != Result.Success)
            throw new Exception($"Failed to create Vulkan instance. Instance creation returned {result.ToString()}.");

        return instance;
    }
    
    private unsafe PhysicalDevice SelectPhysicalDevice()
    {
        uint deviceCount = 0;
        var result = _vk.EnumeratePhysicalDevices(_instance, ref deviceCount, null);
            
        if (result != Result.Success)
            throw new Exception($"Failed to enumerate physical devices with error: {result.ToString()}.");

        if (deviceCount == 0)
            throw new Exception("No available GPUs with Vulkan support were found");
            
        var devices = new PhysicalDevice[deviceCount];
        fixed (PhysicalDevice* devicesPtr = devices)
        {
            result = _vk.EnumeratePhysicalDevices(_instance, ref deviceCount, devicesPtr);
            if (result != Result.Success)
                throw new Exception($"Failed to enumerate physical devices with error: {result.ToString()}.");
        }
        
        foreach (var physicalDevice in devices)
        {
            _vk.GetPhysicalDeviceProperties(physicalDevice, out var properties);

            if (properties.DeviceType != PhysicalDeviceType.DiscreteGpu) continue;
            return physicalDevice;
        }

        throw new Exception("Failed to find a discrete graphics card to use");
    }
    
    private unsafe uint SelectGraphicsQueueFamily()
    {
        uint count = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref count, null);
            
            
        var queues = new QueueFamilyProperties[count];
        fixed (QueueFamilyProperties* queuesPtr = queues)
        {
            _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref count, queuesPtr);
        }

        for (uint i = 0; i < queues.Length; i++)
        {
            if (!queues[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit)) continue;
            return  i;
        }

        throw new Exception("Failed to find a valid graphics queue");
    }

    private unsafe (Device, Queue) CreateLogicalDevice()
    {
        var deviceExtensions = new[] {"VK_KHR_swapchain"};

        const int queueCreateInfoCount = 1;
        using var mem = GlobalMemory.Allocate(queueCreateInfoCount * sizeof(DeviceQueueCreateInfo));
        var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());
            
        var queuePriority = 1.0f;
        queueCreateInfos[0] = new DeviceQueueCreateInfo()
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = _queueFamilyIndex,
            QueueCount = 1,
            PQueuePriorities = &queuePriority
        };
            
        var deviceCreateInfo = new DeviceCreateInfo()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = queueCreateInfoCount,
            PQueueCreateInfos = queueCreateInfos,
            EnabledExtensionCount = (uint) deviceExtensions.Length,
            PpEnabledExtensionNames = (byte**) SilkMarshal.StringArrayToPtr(deviceExtensions),

        };
            
        var result = _vk.CreateDevice(_physicalDevice, deviceCreateInfo, null, out var device);
            
        SilkMarshal.Free((nint) deviceCreateInfo.PpEnabledExtensionNames);
            
        if (result != Result.Success)
            throw new Exception($"Failed to create Vulkan device. Device creation returned {result.ToString()}.");

        // This should probably be moved into CreateSwapchain or CreateSurface after we check for WSI support
        _vk.GetDeviceQueue(device, _queueFamilyIndex, 0, out var queue);
        
        return (device, queue);
    }
    
    private unsafe (SurfaceKHR, KhrSurface) CreateSurface()
    {
        var glfw = _window.Glfw;
        var vkNonDispatchableHandle = stackalloc VkNonDispatchableHandle[1];
        
        var isSurfaceCreated = glfw.CreateWindowSurface(_instance.ToHandle(), _window.WindowHandle, null, vkNonDispatchableHandle);
        if (isSurfaceCreated != 0)
            throw new Exception($"Failed to create window surface. Window surface creation returned error code: {isSurfaceCreated}");

        var surface = vkNonDispatchableHandle[0].ToSurface();
        
        if (!_vk.TryGetInstanceExtension(_instance, out KhrSurface khrSurface))
            throw new Exception("KHR_surface extension was not found or was not be loaded");

        khrSurface.GetPhysicalDeviceSurfaceSupport(_physicalDevice, _queueFamilyIndex, surface, out var iSupported);
        if (!iSupported)
            throw new Exception($"No WSI support on selected physical device");

        return (surface, khrSurface);
    }

    private unsafe (SwapchainKHR, KhrSwapchain, Format, Extent2D) CreateSwapchain()
    {
        var glfw = _window.Glfw;
        
        // Capabilities
        _khrSurface.GetPhysicalDeviceSurfaceCapabilities(_physicalDevice, _surface, out var capabilities);
        
        var surfaceExtent = capabilities.CurrentExtent;
        
        if (capabilities.CurrentExtent.Width == uint.MaxValue)
        {
            glfw.GetFramebufferSize(_window.WindowHandle, out var frameBufferWidth, out var frameBufferHeight);
        
            surfaceExtent = new Extent2D
            {
                Width = (uint) frameBufferWidth,
                Height = (uint) frameBufferHeight
            };
        
            surfaceExtent.Width = Math.Clamp(surfaceExtent.Width, capabilities.MinImageExtent.Width,
                capabilities.MaxImageExtent.Width);
        
            surfaceExtent.Height = Math.Clamp(surfaceExtent.Height, capabilities.MinImageExtent.Height,
                capabilities.MaxImageExtent.Height);
        
        }
        
        // Formats
        uint formatCount = 0;
        var result = _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, ref formatCount, null);
        
        if (result != Result.Success)
            throw new Exception($"Failed to get physical device formats with error: {result.ToString()}.");
        
        if (formatCount == 0)
            throw new Exception("No available formats for selected physical device");
        
        var formats = new SurfaceFormatKHR[formatCount];
        
        fixed (SurfaceFormatKHR* formatsPtr = formats)
        {
            result = _khrSurface.GetPhysicalDeviceSurfaceFormats(_physicalDevice, _surface, ref formatCount,
                formatsPtr);
        
            if (result != Result.Success)
                throw new Exception(
                    $"Failed to get physical device surface formats with error: {result.ToString()}.");
        }
        
        var surfaceFormat = formats[0];
        
        foreach (var availableFormat in formats)
        {
            if (availableFormat.Format == Format.B8G8R8A8Srgb &&
                availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
            {
                surfaceFormat = availableFormat;
                break;
            }
        }
        
        // Present Modes
        uint presentModesCount = 0;
        
        result = _khrSurface.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface, ref presentModesCount,
            null);
        
        if (result != Result.Success)
            throw new Exception($"Failed to get physical device present modes with error: {result.ToString()}.");
        
        if (presentModesCount == 0)
            throw new Exception("No available present modes for selected physical device");
        
        var presentModes = new PresentModeKHR[presentModesCount];
        
        fixed (PresentModeKHR* presentModesPtr = presentModes)
        {
            result = _khrSurface.GetPhysicalDeviceSurfacePresentModes(_physicalDevice, _surface,
                ref presentModesCount,
                presentModesPtr);
        
            if (result != Result.Success)
                throw new Exception(
                    $"Failed to get physical device present modes with error: {result.ToString()}.");
        }
        
        var surfacePresentMode = PresentModeKHR.FifoKhr;
        
        foreach (var presentMode in presentModes)
        {
            if (presentMode == PresentModeKHR.MailboxKhr)
            {
                surfacePresentMode = presentMode;
                break;
            }
        }
        
        // Swap Chain Create Info
        var imageCount = capabilities.MinImageCount + 1;
        
        if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount)
        {
            imageCount = capabilities.MaxImageCount;
        }
        
        var swapChainCreateInfo = new SwapchainCreateInfoKHR()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface,
            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = surfaceExtent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,
            ImageSharingMode = SharingMode.Exclusive,
            PreTransform = capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = surfacePresentMode,
            Clipped = true,
            OldSwapchain = default
        };
        
        
        if (!_vk.TryGetDeviceExtension(_instance, _logicalDevice, out KhrSwapchain khrSwapchain))
            throw new Exception("VK_KHR_swapchain extension was not found or was not be loaded");
        
        result = khrSwapchain.CreateSwapchain(_logicalDevice, swapChainCreateInfo, null, out var swapChain);
        
        if (result != Result.Success)
            throw new Exception($"Failed to create swap chain with error: {result.ToString()}.");

        return (swapChain, khrSwapchain, surfaceFormat.Format, surfaceExtent);
    }

    private unsafe (Image[], ImageView[]) CreateImageViews(KhrSwapchain khrSwapchain, SwapchainKHR swapchain, Format swapchainFormat)
    {
        uint imageCount = 0;
        khrSwapchain.GetSwapchainImages(_logicalDevice, swapchain, ref imageCount, null);
        
        var swapChainImages = new Image[imageCount];
        fixed (Image* swapChainImagesPtr = swapChainImages)
        {
            khrSwapchain.GetSwapchainImages(_logicalDevice, swapchain, ref imageCount, swapChainImagesPtr);
        }
        
        var swapChainImageViews = new ImageView[swapChainImages.Length];

        for (var i = 0; i < swapChainImages.Length; i++)
        {
            ImageViewCreateInfo createInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = swapChainImages[i],
                ViewType = ImageViewType.Type2D,
                Format = swapchainFormat,
                Components =
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity,
                },
                SubresourceRange =
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                }

            };


            var result = _vk.CreateImageView(_logicalDevice, createInfo, null, out swapChainImageViews[i]);
            if (result != Result.Success)
                throw new Exception($"Failed to create image views with error: {result.ToString()}");
        }
        
        return (swapChainImages, swapChainImageViews);
    }

    private unsafe RenderPass CreateRenderPass(Format swapchainFormat)
    {
        AttachmentDescription colorAttachment = new()
        {
            Format = swapchainFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr,
        };

        AttachmentReference colorAttachmentRef = new()
        {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal,
        };

        SubpassDescription subpass = new()
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef,
        };

        RenderPassCreateInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &colorAttachment,
            SubpassCount = 1,
            PSubpasses = &subpass,
        };

        var result = _vk.CreateRenderPass(_logicalDevice, renderPassInfo, null, out var renderPass);
        if (result != Result.Success)
            throw new Exception($"Failed to create render pass with error: {result.ToString()}");

        return renderPass;
    }
    
    private unsafe (PipelineLayout, Pipeline) CreateGraphicsPipeline(RenderPass renderPass, Extent2D swapchainExtent)
    {
        var vertShaderModule = CreateShaderModule("Resources/Shaders/vert.spv");
        var fragShaderModule = CreateShaderModule("Resources/Shaders/frag.spv");

        PipelineShaderStageCreateInfo  vertShaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.VertexBit,
            Module = vertShaderModule,
            PName = (byte*)SilkMarshal.StringToPtr("main")
        };

        PipelineShaderStageCreateInfo fragShaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.FragmentBit,
            Module = fragShaderModule,
            PName = (byte*)SilkMarshal.StringToPtr("main")
        };

        var shaderStages = stackalloc[]
        {
            vertShaderStageInfo,
            fragShaderStageInfo
        };
        
        var bindingDescription = Vertex.GetBindingDescription();
        var attributeDescriptions = Vertex.GetAttributeDescriptions();

        fixed (VertexInputAttributeDescription* attributeDescriptionsPtr = attributeDescriptions)
        {
            
            PipelineVertexInputStateCreateInfo vertexInputInfo = new()
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 1,
                VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length,
                PVertexBindingDescriptions = &bindingDescription,
                PVertexAttributeDescriptions = attributeDescriptionsPtr,
            };

            PipelineInputAssemblyStateCreateInfo inputAssembly = new()
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
                PrimitiveRestartEnable = false,
            };
            
            Viewport viewport = new()
            {
                X = 0,
                Y = 0,
                Width = swapchainExtent.Width,
                Height = swapchainExtent.Height,
                MinDepth = 0,
                MaxDepth = 1,
            };
            
            Rect2D scissor = new()
            {
                Offset = { X = 0, Y = 0 },
                Extent = swapchainExtent,
            };
            
            PipelineViewportStateCreateInfo viewportState = new()
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                PViewports = &viewport,
                ScissorCount = 1,
                PScissors = &scissor,
            };
            
            PipelineRasterizationStateCreateInfo rasterizer = new()
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                PolygonMode = PolygonMode.Fill,
                LineWidth = 1,
                CullMode = CullModeFlags.BackBit,
                FrontFace = FrontFace.Clockwise,
                DepthBiasEnable = false,
            };
            
            PipelineMultisampleStateCreateInfo multisampling = new()
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                SampleShadingEnable = false,
                RasterizationSamples = SampleCountFlags.Count1Bit,
            };
            
            PipelineColorBlendAttachmentState colorBlendAttachment = new()
            {
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
                BlendEnable = false,
            };
            
            PipelineColorBlendStateCreateInfo colorBlending = new()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOpEnable = false,
                LogicOp = LogicOp.Copy,
                AttachmentCount = 1,
                PAttachments = &colorBlendAttachment,
            };
            
            colorBlending.BlendConstants[0] = 0;
            colorBlending.BlendConstants[1] = 0;
            colorBlending.BlendConstants[2] = 0;
            colorBlending.BlendConstants[3] = 0;
            
            PipelineLayoutCreateInfo pipelineLayoutInfo = new()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 0,
                PushConstantRangeCount = 0,
            };
            
            var result = _vk.CreatePipelineLayout(_logicalDevice, pipelineLayoutInfo, null, out var pipelineLayout);
            if (result != Result.Success)
                throw new Exception($"Failed to create graphics pipeline layout with error: {result.ToString()}");
            
            GraphicsPipelineCreateInfo pipelineInfo = new()
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = shaderStages,
                PVertexInputState = &vertexInputInfo,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &rasterizer,
                PMultisampleState = &multisampling,
                PColorBlendState = &colorBlending,
                Layout = pipelineLayout,
                RenderPass = renderPass,
                Subpass = 0,
                BasePipelineHandle = default
            };
            
            result = _vk.CreateGraphicsPipelines(_logicalDevice, default, 1, pipelineInfo, null, out var graphicsPipeline);
            if (result != Result.Success)
                throw new Exception($"Failed to create graphics pipeline with error: {result.ToString()}");
            
            
            _vk.DestroyShaderModule(_logicalDevice, fragShaderModule, null);
            _vk.DestroyShaderModule(_logicalDevice, vertShaderModule, null);
            
            SilkMarshal.Free((nint)vertShaderStageInfo.PName);
            SilkMarshal.Free((nint)fragShaderStageInfo.PName);
            
            return (pipelineLayout, graphicsPipeline);
        }
    }

    private unsafe Framebuffer[] CreateFramebuffers(Extent2D swapchainExtent, ImageView[] imageViews, RenderPass renderPass)
    {
        var swapChainFramebuffers = new Framebuffer[imageViews.Length];

        for (var i = 0; i < imageViews.Length; i++)
        {
            var attachment = imageViews[i];

            FramebufferCreateInfo framebufferInfo = new()
            {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = renderPass,
                AttachmentCount = 1,
                PAttachments = &attachment,
                Width = swapchainExtent.Width,
                Height = swapchainExtent.Height,
                Layers = 1,
            };

            var result = _vk.CreateFramebuffer(_logicalDevice, framebufferInfo, null, out swapChainFramebuffers[i]);
            if (result != Result.Success)
                throw new Exception($"Failed to create framebuffer with error {result.ToString()}");
        }

        return swapChainFramebuffers;
    }
    
    private unsafe ShaderModule CreateShaderModule(string filename)
    {
        var code = File.ReadAllBytes(filename);
        
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (nuint)code.Length,
        };

        ShaderModule shaderModule;

        fixed (byte* codePtr = code)
        {
            createInfo.PCode = (uint*)codePtr;

            var result = _vk.CreateShaderModule(_logicalDevice, createInfo, null, out shaderModule); 
            
            if (result != Result.Success)
                throw new Exception($"Failed to create shader module for shader {filename} with error: {result.ToString()}");
        }

        return shaderModule;
    }

    private unsafe CommandPool CreateCommandPool()
    {
        CommandPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = _queueFamilyIndex,
        };

        var result = _vk.CreateCommandPool(_logicalDevice, poolInfo, null, out var commandPool); 
        if (result != Result.Success)
            throw new Exception($"Failed to create command pool with error: {result.ToString()}");

        return commandPool;
    }
    
    private unsafe (Buffer, DeviceMemory) CreateVertexBuffer()
    {
        BufferCreateInfo bufferInfo = new()
        {
            SType = StructureType.BufferCreateInfo,
            Size = (ulong)(sizeof(Vertex) * _vertices.Length),
            Usage = BufferUsageFlags.VertexBufferBit,
            SharingMode = SharingMode.Exclusive,
        };

        var result = _vk.CreateBuffer(_logicalDevice, bufferInfo, null, out var vertexBuffer);
        if (result != Result.Success)
            throw new Exception($"Failed to create vertex buffer with error: {result.ToString()}");

        _vk.GetBufferMemoryRequirements(_logicalDevice, vertexBuffer, out var memRequirements);

        MemoryAllocateInfo allocateInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit),
        };

        result = _vk.AllocateMemory(_logicalDevice, allocateInfo, null, out var vertexBufferMemory); 
        if (result != Result.Success)
            throw new Exception($"Failed to allocate vertex buffer memory with error: {result.ToString()}");

        _vk.BindBufferMemory(_logicalDevice, vertexBuffer, vertexBufferMemory, 0);

        void* data;
        _vk.MapMemory(_logicalDevice, vertexBufferMemory, 0, bufferInfo.Size, 0, &data);
        _vertices.AsSpan().CopyTo(new Span<Vertex>(data, _vertices.Length));
        _vk.UnmapMemory(_logicalDevice, vertexBufferMemory);

        return (vertexBuffer, vertexBufferMemory);
    }
    
    private uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
    {
        _vk.GetPhysicalDeviceMemoryProperties(_physicalDevice, out var memProperties);

        for (var i = 0; i < memProperties.MemoryTypeCount; i++)
        {
            if ((typeFilter & (1 << i)) != 0 && (memProperties.MemoryTypes[i].PropertyFlags & properties) == properties)
                return (uint)i;
        }

        throw new Exception("Failed to find suitable memory type");
    }
    
    private unsafe CommandBuffer[] CreateCommandBuffers(Extent2D swapchainExtent, RenderPass renderPass, Pipeline pipeline, Framebuffer[] framebuffers)
    {
        var commandBuffers = new CommandBuffer[framebuffers.Length];

        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = (uint)commandBuffers.Length,
        };

        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            var result = _vk.AllocateCommandBuffers(_logicalDevice, allocInfo, commandBuffersPtr);
            if (result != Result.Success)
                throw new Exception($"Failed to allocate command buffers with error: {result.ToString()}");
        }

        for (var i = 0; i < commandBuffers.Length; i++)
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo,
            };

            var result = _vk.BeginCommandBuffer(commandBuffers[i], beginInfo);
            if (result != Result.Success)
                throw new Exception($"Failed to begin recording command buffer with error: {result.ToString()}");

            RenderPassBeginInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = renderPass,
                Framebuffer = framebuffers[i],
                RenderArea =
                {
                    Offset = { X = 0, Y = 0 },
                    Extent = swapchainExtent,
                }
            };

            ClearValue clearColor = new()
            {
                Color = new ClearColorValue
                    { Float32_0 = 0, Float32_1 = 0, Float32_2 = 0, Float32_3 = 1 },
            };

            renderPassInfo.ClearValueCount = 1;
            renderPassInfo.PClearValues = &clearColor;

            _vk.CmdBeginRenderPass(commandBuffers[i], &renderPassInfo, SubpassContents.Inline);

            _vk.CmdBindPipeline(commandBuffers[i], PipelineBindPoint.Graphics, pipeline);

            var vertexBuffers = new[] { _vertexBuffer };
            var offsets = new ulong[] { 0 };

            fixed (ulong* offsetsPtr = offsets)
            fixed (Buffer* vertexBuffersPtr = vertexBuffers)
            {
                _vk.CmdBindVertexBuffers(commandBuffers[i], 0, 1, vertexBuffersPtr, offsetsPtr);
            }
            
            _vk.CmdDraw(commandBuffers[i], 3, 1, 0, 0);

            _vk.CmdEndRenderPass(commandBuffers[i]);

            result = _vk.EndCommandBuffer(commandBuffers[i]); 
            if (result != Result.Success)
                throw new Exception($"Failed to record command buffer with error: {result.ToString()}");
        }

        return commandBuffers;
    }
    
    private unsafe (Semaphore[], Semaphore[], Fence[], Fence[]) CreateSyncObjects(Image[] images)
    {
        var imageAvailableSemaphores = new Semaphore[MaxFramesInFlight];
        var renderFinishedSemaphores = new Semaphore[MaxFramesInFlight];
        var inFlightFences = new Fence[MaxFramesInFlight];
        var imagesInFlight = new Fence[images.Length];

        SemaphoreCreateInfo semaphoreInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo,
        };

        FenceCreateInfo fenceInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit,
        };

        for (var i = 0; i < MaxFramesInFlight; i++)
        {
            var result = _vk.CreateSemaphore(_logicalDevice, semaphoreInfo, null, out imageAvailableSemaphores[i]);
            if (result != Result.Success)
                throw new Exception($"Failed to create image available semaphore with error: {result.ToString()}");
            
            result = _vk.CreateSemaphore(_logicalDevice, semaphoreInfo, null, out renderFinishedSemaphores[i]);
            if (result != Result.Success)
                throw new Exception($"Failed to create render finished semaphore with error: {result.ToString()}");
            
            result = _vk.CreateFence(_logicalDevice, fenceInfo, null, out inFlightFences[i]);
            if (result != Result.Success)
                throw new Exception($"Failed to create in flight fence with error: {result.ToString()}");
        }

        return (imageAvailableSemaphores, renderFinishedSemaphores, inFlightFences, imagesInFlight);
    }
    
    public unsafe void DrawFrame(/*double delta*/)
    {
        _vk.WaitForFences(_logicalDevice, 1, _inFlightFences[_currentFrame], true, ulong.MaxValue);

        uint imageIndex = 0;
        var result = _khrSwapchain.AcquireNextImage(_logicalDevice, _swapchain, ulong.MaxValue, _imageAvailableSemaphores[_currentFrame], default, ref imageIndex);

        if (result == Result.ErrorOutOfDateKhr)
        {
            RecreateSwapChain();
            return;
        }

        if (_imagesInFlight[imageIndex].Handle != default)
            _vk.WaitForFences(_logicalDevice, 1, _imagesInFlight[imageIndex], true, ulong.MaxValue);
        
        _imagesInFlight[imageIndex] = _inFlightFences[_currentFrame];

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo,
        };

        var waitSemaphores = stackalloc[] { _imageAvailableSemaphores[_currentFrame] };
        var waitStages = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };

        var buffer = _commandBuffers[imageIndex];

        submitInfo = submitInfo with
        {
            WaitSemaphoreCount = 1,
            PWaitSemaphores = waitSemaphores,
            PWaitDstStageMask = waitStages,

            CommandBufferCount = 1,
            PCommandBuffers = &buffer
        };

        var signalSemaphores = stackalloc[] { _renderFinishedSemaphores[_currentFrame] };
        submitInfo = submitInfo with
        {
            SignalSemaphoreCount = 1,
            PSignalSemaphores = signalSemaphores,
        };

        _vk.ResetFences(_logicalDevice, 1, _inFlightFences[_currentFrame]);

        result = _vk.QueueSubmit(_queue, 1, submitInfo, _inFlightFences[_currentFrame]);
        if (result != Result.Success)
            throw new Exception($"Failed to submit draw command buffer with error: {result.ToString()}");

        var swapChains = stackalloc[] { _swapchain };
        PresentInfoKHR presentInfo = new()
        {
            SType = StructureType.PresentInfoKhr,

            WaitSemaphoreCount = 1,
            PWaitSemaphores = signalSemaphores,

            SwapchainCount = 1,
            PSwapchains = swapChains,

            PImageIndices = &imageIndex
        };

        result = _khrSwapchain.QueuePresent(_queue, presentInfo);

        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || _frameBufferResized)
        {
            _frameBufferResized = false;
            RecreateSwapChain();
        }
        else if (result != Result.Success)
            throw new Exception($"Failed to present swap chain image with error: ${result.ToString()}");

        _currentFrame = (_currentFrame + 1) % MaxFramesInFlight;
    }

    public void DeviceWaitIdle()
    {
        _vk.DeviceWaitIdle(_logicalDevice);
    }
    
    private void RecreateSwapChain()
    {
        var (width, height) = _window.GetFrameBufferSize();
        while (width == 0 || height == 0)
        {
            (width, height) = _window.GetFrameBufferSize();
            _window.WaitEvents();
        }

        DeviceWaitIdle();

        CleanUpSwapChain();

        (_swapchain, _khrSwapchain, _swapchainFormat, _swapchainExtent) = CreateSwapchain();    
        (_images, _imageViews) = CreateImageViews(_khrSwapchain, _swapchain, _swapchainFormat); 
        _renderPass = CreateRenderPass(_swapchainFormat);                                       
        (_pipelineLayout, _pipeline) = CreateGraphicsPipeline(_renderPass, _swapchainExtent);   
        _framebuffers = CreateFramebuffers(_swapchainExtent, _imageViews, _renderPass);         
        _commandBuffers = CreateCommandBuffers(_swapchainExtent, _renderPass, _pipeline, _framebuffers);

        _imagesInFlight = new Fence[_images.Length];
    }
    
    private unsafe void CleanUpSwapChain()
    {
        foreach (var framebuffer in _framebuffers)
            _vk.DestroyFramebuffer(_logicalDevice, framebuffer, null);

        fixed (CommandBuffer* commandBuffersPtr = _commandBuffers)
        {
            _vk.FreeCommandBuffers(_logicalDevice, _commandPool, (uint)_commandBuffers.Length, commandBuffersPtr);
        }

        _vk.DestroyPipeline(_logicalDevice, _pipeline, null);
        _vk.DestroyPipelineLayout(_logicalDevice, _pipelineLayout, null);
        _vk.DestroyRenderPass(_logicalDevice, _renderPass, null);

        foreach (var imageView in _imageViews)
            _vk.DestroyImageView(_logicalDevice, imageView, null);

        _khrSwapchain.DestroySwapchain(_logicalDevice, _swapchain, null);
    }
    
    public unsafe void Dispose()
    {
        CleanUpSwapChain();
        
        _vk.DestroyBuffer(_logicalDevice, _vertexBuffer, null);
        _vk.FreeMemory(_logicalDevice, _vertexBufferMemory, null);
        
        for (var i = 0; i < MaxFramesInFlight; i++)
        {
            _vk.DestroySemaphore(_logicalDevice, _renderFinishedSemaphores[i], null);
            _vk.DestroySemaphore(_logicalDevice, _imageAvailableSemaphores[i], null);
            _vk.DestroyFence(_logicalDevice, _inFlightFences[i], null);
        }
        
        _vk.DestroyCommandPool(_logicalDevice, _commandPool, null);

        _khrSurface.DestroySurface(_instance, _surface, null);
        
        _vk.DestroyDevice(_logicalDevice, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
        GC.SuppressFinalize(this);
    }
}