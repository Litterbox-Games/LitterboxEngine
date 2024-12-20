﻿// From: https://github.com/dotnet/Silk.NET/tree/main/src/Lab/Experiments/ImGuiVulkan
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using Silk.NET.Maths;
using Silk.NET.Windowing;

// ReSharper disable once CheckNamespace
namespace Silk.NET.Vulkan.Extensions.ImGui
{
    public class ImGuiController : IDisposable
    {
        private readonly Vk _vk;
        private readonly IView _view;
        private readonly IInputContext _input;
        private readonly Device _device;
        private readonly PhysicalDevice _physicalDevice;
        private bool _frameBegun;
        private readonly List<char> _pressedChars = new();
        private readonly IKeyboard _keyboard;
        private readonly DescriptorPool _descriptorPool;
        private readonly RenderPass _renderPass;
        private int _windowWidth;
        private int _windowHeight;
        private readonly int _swapChainImageCt;
        private readonly Sampler _fontSampler;
        private DescriptorSetLayout _descriptorSetLayout;
        private readonly DescriptorSet _descriptorSet;
        private readonly PipelineLayout _pipelineLayout;
        private readonly ShaderModule _shaderModuleVert;
        private readonly ShaderModule _shaderModuleFrag;
        private readonly Pipeline _pipeline;
        private WindowRenderBuffers _mainWindowRenderBuffers;
        private GlobalMemory? _frameRenderBuffers;
        private readonly DeviceMemory _fontMemory;
        private readonly Image _fontImage;
        private readonly ImageView _fontView;
        private ulong _bufferMemoryAlignment = 256;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        /// <param name="vk">The vulkan api instance</param>
        /// <param name="view">Window view</param>
        /// <param name="input">Input context</param>
        /// <param name="physicalDevice">The physical device instance in use</param>
        /// <param name="graphicsFamilyIndex">The graphics family index corresponding to the graphics queue</param>
        /// <param name="swapChainImageCt">The number of images used in the swap chain</param>
        /// <param name="swapChainFormat">The image format used by the swap chain</param>
        /// <param name="depthBufferFormat">The image format used by the depth buffer, or null if no depth buffer is used</param>
        /// <param name="imGuiFontConfig">A custom ImGui configuration</param>        
        public unsafe ImGuiController(Vk vk, IView view, IInputContext input, PhysicalDevice physicalDevice,
            uint graphicsFamilyIndex, int swapChainImageCt, Format swapChainFormat, Format? depthBufferFormat, ImGuiFontConfig? imGuiFontConfig)
        {
            var context = ImGuiNET.ImGui.CreateContext();
            ImGuiNET.ImGui.SetCurrentContext(context);
            var io = ImGuiNET.ImGui.GetIO();
            if (imGuiFontConfig.HasValue)
            {
                if (io.Fonts.AddFontFromFileTTF(imGuiFontConfig.Value.FontPath, imGuiFontConfig.Value.FontSize).NativePtr == default)
                    throw new Exception($"Failed to load ImGui font");
            }
            else
            {
                io.Fonts.AddFontDefault();    
            }

            _vk = vk;
            _view = view;
            _input = input;
            _keyboard = _input.Keyboards[0];
            _physicalDevice = physicalDevice;
            _windowWidth = view.Size.X;
            _windowHeight = view.Size.Y;
            _swapChainImageCt = swapChainImageCt;

            if (swapChainImageCt < 2)
            {
                throw new Exception($"Swap chain image count must be >= 2");
            }

            if (!_vk.CurrentDevice.HasValue)
            {
                throw new InvalidOperationException("vk.CurrentDevice is null. _vk.CurrentDevice must be set to the current device.");
            }

            _device = _vk.CurrentDevice.Value;

            // Set default style
            ImGuiNET.ImGui.StyleColorsDark();

            // Create the descriptor pool for ImGui
            Span<DescriptorPoolSize> poolSizes = stackalloc DescriptorPoolSize[] { new DescriptorPoolSize(DescriptorType.CombinedImageSampler, 1) };
            var descriptorPool = new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = (uint)poolSizes.Length,
                PPoolSizes = (DescriptorPoolSize*)Unsafe.AsPointer(ref poolSizes.GetPinnableReference()),
                MaxSets = 1
            };

            if (_vk.CreateDescriptorPool(_device, in descriptorPool, default, out _descriptorPool) != Result.Success)
            {
                throw new Exception($"Unable to create descriptor pool");
            }

            // Create the render pass
            var colorAttachment = new AttachmentDescription
            {
                Format = swapChainFormat,
                Samples = SampleCountFlags.Count1Bit,
                LoadOp = AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.PresentSrcKhr,
                FinalLayout = ImageLayout.PresentSrcKhr
            };

            var colorAttachmentRef = new AttachmentReference
            {
                Attachment = 0,
                Layout = ImageLayout.ColorAttachmentOptimal
            };

            var subpass = new SubpassDescription
            {
                PipelineBindPoint = PipelineBindPoint.Graphics,
                ColorAttachmentCount = 1,
                PColorAttachments = (AttachmentReference*)Unsafe.AsPointer(ref colorAttachmentRef)
            };

            Span<AttachmentDescription> attachments = stackalloc AttachmentDescription[] { colorAttachment };
            var depthAttachment = new AttachmentDescription();
            var depthAttachmentRef = new AttachmentReference();
            if (depthBufferFormat.HasValue)
            {
                depthAttachment.Format = depthBufferFormat.Value;
                depthAttachment.Samples = SampleCountFlags.Count1Bit;
                depthAttachment.LoadOp = AttachmentLoadOp.Load;
                depthAttachment.StoreOp = AttachmentStoreOp.DontCare;
                depthAttachment.StencilLoadOp = AttachmentLoadOp.DontCare;
                depthAttachment.StencilStoreOp = AttachmentStoreOp.DontCare;
                depthAttachment.InitialLayout = ImageLayout.DepthStencilAttachmentOptimal;
                depthAttachment.FinalLayout = ImageLayout.DepthStencilAttachmentOptimal;

                depthAttachmentRef.Attachment = 1;
                depthAttachmentRef.Layout = ImageLayout.DepthStencilAttachmentOptimal;

                subpass.PDepthStencilAttachment = (AttachmentReference*)Unsafe.AsPointer(ref depthAttachmentRef);

                attachments = stackalloc AttachmentDescription[] { colorAttachment, depthAttachment };
            }

            var dependency = new SubpassDependency
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                SrcAccessMask = 0,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                DstAccessMask = AccessFlags.ColorAttachmentReadBit | AccessFlags.ColorAttachmentWriteBit
            };

            var renderPassInfo = new RenderPassCreateInfo
            {
                SType = StructureType.RenderPassCreateInfo,
                AttachmentCount = (uint)attachments.Length,
                PAttachments = (AttachmentDescription*)Unsafe.AsPointer(ref attachments.GetPinnableReference()),
                SubpassCount = 1,
                PSubpasses = (SubpassDescription*)Unsafe.AsPointer(ref subpass),
                DependencyCount = 1,
                PDependencies = (SubpassDependency*)Unsafe.AsPointer(ref dependency)
            };

            if (_vk.CreateRenderPass(_device, in renderPassInfo, default, out _renderPass) != Result.Success)
            {
                throw new Exception($"Failed to create render pass");
            }

            var info = new SamplerCreateInfo
            {
                SType = StructureType.SamplerCreateInfo,
                MagFilter = Filter.Linear,
                MinFilter = Filter.Linear,
                MipmapMode = SamplerMipmapMode.Linear,
                AddressModeU = SamplerAddressMode.Repeat,
                AddressModeV = SamplerAddressMode.Repeat,
                AddressModeW = SamplerAddressMode.Repeat,
                MinLod = -1000,
                MaxLod = 1000,
                MaxAnisotropy = 1.0f
            };

            if (vk.CreateSampler(_device, in info, default, out _fontSampler) != Result.Success)
            {
                throw new Exception($"Unable to create sampler");
            }

            var sampler = _fontSampler;

            var binding = new DescriptorSetLayoutBinding
            {
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                StageFlags = ShaderStageFlags.FragmentBit,
                PImmutableSamplers = (Sampler*)Unsafe.AsPointer(ref sampler)
            };

            var descriptorInfo = new DescriptorSetLayoutCreateInfo
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1,
                PBindings = (DescriptorSetLayoutBinding*)Unsafe.AsPointer(ref binding)
            };

            if (vk.CreateDescriptorSetLayout(_device, in descriptorInfo, default, out _descriptorSetLayout) != Result.Success)
            {
                throw new Exception($"Unable to create descriptor set layout");
            }

            fixed (DescriptorSetLayout* pgDescriptorSetLayout = &_descriptorSetLayout)
            {
                var allocateInfo = new DescriptorSetAllocateInfo
                {
                    SType = StructureType.DescriptorSetAllocateInfo,
                    DescriptorPool = _descriptorPool,
                    DescriptorSetCount = 1,
                    PSetLayouts = pgDescriptorSetLayout
                };

                if (vk.AllocateDescriptorSets(_device, in allocateInfo, out _descriptorSet) != Result.Success)
                {
                    throw new Exception($"Unable to create descriptor sets");
                }
            }

            var vertPushConst = new PushConstantRange
            {
                StageFlags = ShaderStageFlags.VertexBit,
                Offset = sizeof(float) * 0,
                Size = sizeof(float) * 4
            };

            var setLayout = _descriptorSetLayout;
            var layoutInfo = new PipelineLayoutCreateInfo
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                SetLayoutCount = 1,
                PSetLayouts = (DescriptorSetLayout*)Unsafe.AsPointer(ref setLayout),
                PushConstantRangeCount = 1,
                PPushConstantRanges = (PushConstantRange*)Unsafe.AsPointer(ref vertPushConst)
            };

            if (vk.CreatePipelineLayout(_device, in layoutInfo, default, out _pipelineLayout) != Result.Success)
            {
                throw new Exception($"Unable to create the descriptor set layout");
            }

            // Create the shader modules
            if (_shaderModuleVert.Handle == default)
            {
                fixed (uint* vertShaderBytes = &Shaders.VertexShader[0])
                {
                    var vertInfo = new ShaderModuleCreateInfo
                    {
                        SType = StructureType.ShaderModuleCreateInfo,
                        CodeSize = (nuint)Shaders.VertexShader.Length * sizeof(uint),
                        PCode = vertShaderBytes
                    };

                    if (vk.CreateShaderModule(_device, in vertInfo, default, out _shaderModuleVert) != Result.Success)
                    {
                        throw new Exception($"Unable to create the vertex shader");
                    }
                }
            }
            if (_shaderModuleFrag.Handle == default)
            {
                fixed (uint* fragShaderBytes = &Shaders.FragmentShader[0])
                {
                    var fragInfo = new ShaderModuleCreateInfo
                    {
                        SType = StructureType.ShaderModuleCreateInfo,
                        CodeSize = (nuint)Shaders.FragmentShader.Length * sizeof(uint),
                        PCode = fragShaderBytes
                    };

                    if (vk.CreateShaderModule(_device, in fragInfo, default, out _shaderModuleFrag) != Result.Success)
                    {
                        throw new Exception($"Unable to create the fragment shader");
                    }
                }
            }

            // Create the pipeline
            Span<PipelineShaderStageCreateInfo> stage = stackalloc PipelineShaderStageCreateInfo[2];
            stage[0].SType = StructureType.PipelineShaderStageCreateInfo;
            stage[0].Stage = ShaderStageFlags.VertexBit;
            stage[0].Module = _shaderModuleVert;
            stage[0].PName = (byte*)SilkMarshal.StringToPtr("main");
            stage[1].SType = StructureType.PipelineShaderStageCreateInfo;
            stage[1].Stage = ShaderStageFlags.FragmentBit;
            stage[1].Module = _shaderModuleFrag;
            stage[1].PName = (byte*)SilkMarshal.StringToPtr("main");

            var bindingDesc = new VertexInputBindingDescription
            {
                Stride = (uint)Unsafe.SizeOf<ImDrawVert>(),
                InputRate = VertexInputRate.Vertex
            };

            Span<VertexInputAttributeDescription> attributeDesc = stackalloc VertexInputAttributeDescription[3];
            attributeDesc[0].Location = 0;
            attributeDesc[0].Binding = bindingDesc.Binding;
            attributeDesc[0].Format = Format.R32G32Sfloat;
            attributeDesc[0].Offset = (uint)Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.pos));
            attributeDesc[1].Location = 1;
            attributeDesc[1].Binding = bindingDesc.Binding;
            attributeDesc[1].Format = Format.R32G32Sfloat;
            attributeDesc[1].Offset = (uint)Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.uv));
            attributeDesc[2].Location = 2;
            attributeDesc[2].Binding = bindingDesc.Binding;
            attributeDesc[2].Format = Format.R8G8B8A8Unorm;
            attributeDesc[2].Offset = (uint)Marshal.OffsetOf<ImDrawVert>(nameof(ImDrawVert.col));

            var vertexInfo = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 1,
                PVertexBindingDescriptions = (VertexInputBindingDescription*)Unsafe.AsPointer(ref bindingDesc),
                VertexAttributeDescriptionCount = 3,
                PVertexAttributeDescriptions = (VertexInputAttributeDescription*)Unsafe.AsPointer(ref attributeDesc[0])
            };

            var iaInfo = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList
            };

            var viewportInfo = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount = 1
            };

            var rasterInfo = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = PolygonMode.Fill,
                CullMode = CullModeFlags.None,
                FrontFace = FrontFace.CounterClockwise,
                LineWidth = 1.0f
            };

            var msInfo = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = SampleCountFlags.Count1Bit
            };

            var blendAttachmentState = new PipelineColorBlendAttachmentState
            {
                BlendEnable = new Core.Bool32(true),
                SrcColorBlendFactor = BlendFactor.SrcAlpha,
                DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
                ColorBlendOp = BlendOp.Add,
                SrcAlphaBlendFactor = BlendFactor.One,
                DstAlphaBlendFactor = BlendFactor.OneMinusSrcAlpha,
                AlphaBlendOp = BlendOp.Add,
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit
            };

            var depthInfo = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo
            };

            var blendInfo = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                AttachmentCount = 1,
                PAttachments = (PipelineColorBlendAttachmentState*)Unsafe.AsPointer(ref blendAttachmentState)
            };

            Span<DynamicState> dynamicStates = stackalloc DynamicState[] { DynamicState.Viewport, DynamicState.Scissor };
            var dynamicState = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = (uint)dynamicStates.Length,
                PDynamicStates = (DynamicState*)Unsafe.AsPointer(ref dynamicStates[0])
            };

            var pipelineInfo = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                Flags = default,
                StageCount = 2,
                PStages = (PipelineShaderStageCreateInfo*)Unsafe.AsPointer(ref stage[0]),
                PVertexInputState = (PipelineVertexInputStateCreateInfo*)Unsafe.AsPointer(ref vertexInfo),
                PInputAssemblyState = (PipelineInputAssemblyStateCreateInfo*)Unsafe.AsPointer(ref iaInfo),
                PViewportState = (PipelineViewportStateCreateInfo*)Unsafe.AsPointer(ref viewportInfo),
                PRasterizationState = (PipelineRasterizationStateCreateInfo*)Unsafe.AsPointer(ref rasterInfo),
                PMultisampleState = (PipelineMultisampleStateCreateInfo*)Unsafe.AsPointer(ref msInfo),
                PDepthStencilState = (PipelineDepthStencilStateCreateInfo*)Unsafe.AsPointer(ref depthInfo),
                PColorBlendState = (PipelineColorBlendStateCreateInfo*)Unsafe.AsPointer(ref blendInfo),
                PDynamicState = (PipelineDynamicStateCreateInfo*)Unsafe.AsPointer(ref dynamicState),
                Layout = _pipelineLayout,
                RenderPass = _renderPass,
                Subpass = 0
            };

            if (vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, default, out _pipeline) != Result.Success)
            {
                throw new Exception($"Unable to create the pipeline");
            }

            SilkMarshal.Free((nint)stage[0].PName);
            SilkMarshal.Free((nint)stage[1].PName);

            // Initialise ImGui Vulkan adapter
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height);
            var uploadSize = (ulong)(width * height * 4 * sizeof(byte));

            // Submit one-time command to create the fonts texture
            var poolInfo = new CommandPoolCreateInfo
            {
                SType = StructureType.CommandPoolCreateInfo,
                QueueFamilyIndex = graphicsFamilyIndex
            };

            if (_vk.CreateCommandPool(_device, in poolInfo, null, out var commandPool) != Result.Success)
            {
                throw new Exception("failed to create command pool!");
            }

            var allocInfo = new CommandBufferAllocateInfo
            {
                SType = StructureType.CommandBufferAllocateInfo,
                CommandPool = commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1
            };

            if (_vk.AllocateCommandBuffers(_device, in allocInfo, out var commandBuffer) != Result.Success)
            {
                throw new Exception($"Unable to allocate command buffers");
            }

            var beginInfo = new CommandBufferBeginInfo
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit
            };

            if (_vk.BeginCommandBuffer(commandBuffer, in beginInfo) != Result.Success)
            {
                throw new Exception($"Failed to begin a command buffer");
            }

            var imageInfo = new ImageCreateInfo
            {
                SType = StructureType.ImageCreateInfo,
                ImageType = ImageType.Type2D,
                Format = Format.R8G8B8A8Unorm
            };
            imageInfo.Extent.Width = (uint)width;
            imageInfo.Extent.Height = (uint)height;
            imageInfo.Extent.Depth = 1;
            imageInfo.MipLevels = 1;
            imageInfo.ArrayLayers = 1;
            imageInfo.Samples = SampleCountFlags.Count1Bit;
            imageInfo.Tiling = ImageTiling.Optimal;
            imageInfo.Usage = ImageUsageFlags.SampledBit | ImageUsageFlags.TransferDstBit;
            imageInfo.SharingMode = SharingMode.Exclusive;
            imageInfo.InitialLayout = ImageLayout.Undefined;
            if (_vk.CreateImage(_device, in imageInfo, default, out _fontImage) != Result.Success)
            {
                throw new Exception($"Failed to create font image");
            }
            _vk.GetImageMemoryRequirements(_device, _fontImage, out var fontReq);
            var fontAllocInfo = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = fontReq.Size,
                MemoryTypeIndex = GetMemoryTypeIndex(vk, MemoryPropertyFlags.DeviceLocalBit, fontReq.MemoryTypeBits)
            };

            if (_vk.AllocateMemory(_device, &fontAllocInfo, default, out _fontMemory) != Result.Success)
            {
                throw new Exception($"Failed to allocate device memory");
            }
            if (_vk.BindImageMemory(_device, _fontImage, _fontMemory, 0) != Result.Success)
            {
                throw new Exception($"Failed to bind device memory");
            }

            var imageViewInfo = new ImageViewCreateInfo
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = _fontImage,
                ViewType = ImageViewType.Type2D,
                Format = Format.R8G8B8A8Unorm
            };
            imageViewInfo.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
            imageViewInfo.SubresourceRange.LevelCount = 1;
            imageViewInfo.SubresourceRange.LayerCount = 1;
            if (_vk.CreateImageView(_device, &imageViewInfo, default, out _fontView) != Result.Success)
            {
                throw new Exception($"Failed to create an image view");
            }

            var descImageInfo = new DescriptorImageInfo
            {
                Sampler = _fontSampler,
                ImageView = _fontView,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal
            };
            var writeDescriptors = new WriteDescriptorSet
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = _descriptorSet,
                DescriptorCount = 1,
                DescriptorType = DescriptorType.CombinedImageSampler,
                PImageInfo = (DescriptorImageInfo*)Unsafe.AsPointer(ref descImageInfo)
            };
            _vk.UpdateDescriptorSets(_device, 1, in writeDescriptors, 0, default);

            // Create the Upload Buffer:
            var bufferInfo = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = uploadSize,
                Usage = BufferUsageFlags.TransferSrcBit,
                SharingMode = SharingMode.Exclusive
            };

            if (_vk.CreateBuffer(_device, in bufferInfo, default, out var uploadBuffer) != Result.Success)
            {
                throw new Exception($"Failed to create a device buffer");
            }

            _vk.GetBufferMemoryRequirements(_device, uploadBuffer, out var uploadReq);
            _bufferMemoryAlignment = (_bufferMemoryAlignment > uploadReq.Alignment) ? _bufferMemoryAlignment : uploadReq.Alignment;

            var uploadAllocInfo = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = uploadReq.Size,
                MemoryTypeIndex = GetMemoryTypeIndex(vk, MemoryPropertyFlags.HostVisibleBit, uploadReq.MemoryTypeBits)
            };

            if (_vk.AllocateMemory(_device, in uploadAllocInfo, default, out var uploadBufferMemory) != Result.Success)
            {
                throw new Exception($"Failed to allocate device memory");
            }
            if (_vk.BindBufferMemory(_device, uploadBuffer, uploadBufferMemory, 0) != Result.Success)
            {
                throw new Exception($"Failed to bind device memory");
            }

            void* map = null;
            if (_vk.MapMemory(_device, uploadBufferMemory, 0, uploadSize, 0, &map) != Result.Success)
            {
                throw new Exception($"Failed to map device memory");
            }
            Unsafe.CopyBlock(map, pixels.ToPointer(), (uint)uploadSize);

            var range = new MappedMemoryRange
            {
                SType = StructureType.MappedMemoryRange,
                Memory = uploadBufferMemory,
                Size = uploadSize
            };

            if (_vk.FlushMappedMemoryRanges(_device, 1, in range) != Result.Success)
            {
                throw new Exception($"Failed to flush memory to device");
            }
            _vk.UnmapMemory(_device, uploadBufferMemory);

            const uint vkQueueFamilyIgnored = ~0U;

            var copyBarrier = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                DstAccessMask = AccessFlags.TransferWriteBit,
                OldLayout = ImageLayout.Undefined,
                NewLayout = ImageLayout.TransferDstOptimal,
                SrcQueueFamilyIndex = vkQueueFamilyIgnored,
                DstQueueFamilyIndex = vkQueueFamilyIgnored,
                Image = _fontImage
            };
            copyBarrier.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
            copyBarrier.SubresourceRange.LevelCount = 1;
            copyBarrier.SubresourceRange.LayerCount = 1;
            _vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.HostBit, PipelineStageFlags.TransferBit, 0, 0, default, 0, default, 1, in copyBarrier);

            var region = new BufferImageCopy();
            region.ImageSubresource.AspectMask = ImageAspectFlags.ColorBit;
            region.ImageSubresource.LayerCount = 1;
            region.ImageExtent.Width = (uint)width;
            region.ImageExtent.Height = (uint)height;
            region.ImageExtent.Depth = 1;
            _vk.CmdCopyBufferToImage(commandBuffer, uploadBuffer, _fontImage, ImageLayout.TransferDstOptimal, 1, &region);

            var useBarrier = new ImageMemoryBarrier
            {
                SType = StructureType.ImageMemoryBarrier,
                SrcAccessMask = AccessFlags.TransferWriteBit,
                DstAccessMask = AccessFlags.ShaderReadBit,
                OldLayout = ImageLayout.TransferDstOptimal,
                NewLayout = ImageLayout.ShaderReadOnlyOptimal,
                SrcQueueFamilyIndex = vkQueueFamilyIgnored,
                DstQueueFamilyIndex = vkQueueFamilyIgnored,
                Image = _fontImage
            };
            useBarrier.SubresourceRange.AspectMask = ImageAspectFlags.ColorBit;
            useBarrier.SubresourceRange.LevelCount = 1;
            useBarrier.SubresourceRange.LayerCount = 1;
            _vk.CmdPipelineBarrier(commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit, 0, 0, default, 0, default, 1, in useBarrier);

            // Store our identifier
            io.Fonts.SetTexID((IntPtr)_fontImage.Handle);

            if (_vk.EndCommandBuffer(commandBuffer) != Result.Success)
            {
                throw new Exception($"Failed to begin a command buffer");
            }

            _vk.GetDeviceQueue(_device, graphicsFamilyIndex, 0, out var graphicsQueue);

            var submitInfo = new SubmitInfo
            {
                SType = StructureType.SubmitInfo,
                CommandBufferCount = 1,
                PCommandBuffers = (CommandBuffer*)Unsafe.AsPointer(ref commandBuffer)
            };

            if (_vk.QueueSubmit(graphicsQueue, 1, in submitInfo, default) != Result.Success)
            {
                throw new Exception($"Failed to begin a command buffer");
            }

            if (_vk.QueueWaitIdle(graphicsQueue) != Result.Success)
            {
                throw new Exception($"Failed to begin a command buffer");
            }

            _vk.DestroyBuffer(_device, uploadBuffer, default);
            _vk.FreeMemory(_device, uploadBufferMemory, default);
            _vk.DestroyCommandPool(_device, commandPool, default);
            
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            BeginFrame();
        }

        private uint GetMemoryTypeIndex(Vk vk, MemoryPropertyFlags properties, uint typeBits)
        {
            vk.GetPhysicalDeviceMemoryProperties(_physicalDevice, out var prop);
            for (var i = 0; i < prop.MemoryTypeCount; i++)
            {
                if ((prop.MemoryTypes[i].PropertyFlags & properties) == properties && (typeBits & (1u << i)) != 0)
                {
                    return (uint)i;
                }
            }
            return 0xFFFFFFFF; // Unable to find memoryType
        }

        private void BeginFrame()
        {
            ImGuiNET.ImGui.NewFrame();
            _frameBegun = true;
            _view.Resize += WindowResized;
            _keyboard.KeyChar += OnKeyChar;
        }

        private void OnKeyChar(IKeyboard arg1, char arg2)
        {
            _pressedChars.Add(arg2);
        }

        private void WindowResized(Vector2D<int> size)
        {
            _windowWidth = size.X;
            _windowHeight = size.Y;
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void Render(CommandBuffer commandBuffer, Framebuffer framebuffer, Extent2D swapChainExtent)
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGuiNET.ImGui.Render();
                RenderImDrawData(ImGuiNET.ImGui.GetDrawData(), commandBuffer, framebuffer, swapChainExtent);
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state. Call Update() before drawing and rendering.
        /// </summary>
        public void Update(float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGuiNET.ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput();

            _frameBegun = true;
            ImGuiNET.ImGui.NewFrame();
        }

        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            var io = ImGuiNET.ImGui.GetIO();
            io.DisplaySize = new Vector2(_windowWidth, _windowHeight);

            if (_windowWidth > 0 && _windowHeight > 0)
            {
                // ReSharper disable once PossibleLossOfFraction
                io.DisplayFramebufferScale = new Vector2(_view.FramebufferSize.X / _windowWidth, 
                    // ReSharper disable once PossibleLossOfFraction
                    _view.FramebufferSize.Y / _windowHeight);
            }

            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        private void UpdateImGuiInput()
        {
            var io = ImGuiNET.ImGui.GetIO();

            var mouseState = _input.Mice[0].CaptureState();
            var keyboardState = _input.Keyboards[0];

            io.MouseDown[0] = mouseState.IsButtonPressed(MouseButton.Left);
            io.MouseDown[1] = mouseState.IsButtonPressed(MouseButton.Right);
            io.MouseDown[2] = mouseState.IsButtonPressed(MouseButton.Middle);

            var point = new Point((int)mouseState.Position.X, (int)mouseState.Position.Y);
            io.MousePos = new Vector2(point.X, point.Y);

            var wheel = mouseState.GetScrollWheels()[0];
            io.MouseWheel = wheel.Y;
            io.MouseWheelH = wheel.X;

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (key == Key.Unknown)
                {
                    continue;
                }
                io.KeysDown[(int)key] = keyboardState.IsKeyPressed(key);
            }

            foreach (var c in _pressedChars)
            {
                io.AddInputCharacter(c);
            }

            _pressedChars.Clear();

            io.KeyCtrl = keyboardState.IsKeyPressed(Key.ControlLeft) || keyboardState.IsKeyPressed(Key.ControlRight);
            io.KeyAlt = keyboardState.IsKeyPressed(Key.AltLeft) || keyboardState.IsKeyPressed(Key.AltRight);
            io.KeyShift = keyboardState.IsKeyPressed(Key.ShiftLeft) || keyboardState.IsKeyPressed(Key.ShiftRight);
            io.KeySuper = keyboardState.IsKeyPressed(Key.SuperLeft) || keyboardState.IsKeyPressed(Key.SuperRight);
        }

        internal void PressChar(char keyChar)
        {
            _pressedChars.Add(keyChar);
        }

        private static void SetKeyMappings()
        {
            var io = ImGuiNET.ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Key.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Key.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Key.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Key.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Key.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Key.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Key.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Key.Z;
        }

        private unsafe void RenderImDrawData(in ImDrawDataPtr drawDataPtr, in CommandBuffer commandBuffer, in Framebuffer framebuffer, in Extent2D swapChainExtent)
        {
            var framebufferWidth = (int)(drawDataPtr.DisplaySize.X * drawDataPtr.FramebufferScale.X);
            var framebufferHeight = (int)(drawDataPtr.DisplaySize.Y * drawDataPtr.FramebufferScale.Y);
            if (framebufferWidth <= 0 || framebufferHeight <= 0)
            {
                return;
            }

            var renderPassInfo = new RenderPassBeginInfo
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = _renderPass,
                Framebuffer = framebuffer
            };
            renderPassInfo.RenderArea.Offset = default;
            renderPassInfo.RenderArea.Extent = swapChainExtent;
            renderPassInfo.ClearValueCount = 0;
            renderPassInfo.PClearValues = default;

            _vk.CmdBeginRenderPass(commandBuffer, &renderPassInfo, SubpassContents.Inline);

            var drawData = *drawDataPtr.NativePtr;

            // Avoid rendering when minimized, scale coordinates for retina displays (screen coordinates != framebuffer coordinates)
            var fbWidth = (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X);
            var fbHeight = (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y);
            if (fbWidth <= 0 || fbHeight <= 0)
            {
                return;
            }

            // Allocate array to store enough vertex/index buffers
            if (_mainWindowRenderBuffers.FrameRenderBuffers == null)
            {
                _mainWindowRenderBuffers.Index = 0;
                _mainWindowRenderBuffers.Count = (uint)_swapChainImageCt;
                _frameRenderBuffers = GlobalMemory.Allocate(sizeof(FrameRenderBuffer) * (int)_mainWindowRenderBuffers.Count);
                _mainWindowRenderBuffers.FrameRenderBuffers = _frameRenderBuffers.AsPtr<FrameRenderBuffer>();
                for (var i = 0; i < (int)_mainWindowRenderBuffers.Count; i++)
                {
                    _mainWindowRenderBuffers.FrameRenderBuffers[i].IndexBuffer.Handle = 0;
                    _mainWindowRenderBuffers.FrameRenderBuffers[i].IndexBufferSize = 0;
                    _mainWindowRenderBuffers.FrameRenderBuffers[i].IndexBufferMemory.Handle = 0;
                    _mainWindowRenderBuffers.FrameRenderBuffers[i].VertexBuffer.Handle = 0;
                    _mainWindowRenderBuffers.FrameRenderBuffers[i].VertexBufferSize = 0;
                    _mainWindowRenderBuffers.FrameRenderBuffers[i].VertexBufferMemory.Handle = 0;
                }
            }
            _mainWindowRenderBuffers.Index = (_mainWindowRenderBuffers.Index + 1) % _mainWindowRenderBuffers.Count;

            ref var frameRenderBuffer = ref _mainWindowRenderBuffers.FrameRenderBuffers[_mainWindowRenderBuffers.Index];

            if (drawData.TotalVtxCount > 0)
            {
                // Create or resize the vertex/index buffers
                var vertexSize = (ulong)drawData.TotalVtxCount * (ulong)sizeof(ImDrawVert);
                var indexSize = (ulong)drawData.TotalIdxCount * sizeof(ushort);
                if (frameRenderBuffer.VertexBuffer.Handle == default || frameRenderBuffer.VertexBufferSize < vertexSize)
                {
                    CreateOrResizeBuffer(ref frameRenderBuffer.VertexBuffer, ref frameRenderBuffer.VertexBufferMemory, ref frameRenderBuffer.VertexBufferSize, vertexSize, BufferUsageFlags.VertexBufferBit);
                }
                if (frameRenderBuffer.IndexBuffer.Handle == default || frameRenderBuffer.IndexBufferSize < indexSize)
                {
                    CreateOrResizeBuffer(ref frameRenderBuffer.IndexBuffer, ref frameRenderBuffer.IndexBufferMemory, ref frameRenderBuffer.IndexBufferSize, indexSize, BufferUsageFlags.IndexBufferBit);
                }

                // Upload vertex/index data into a single contiguous GPU buffer
                ImDrawVert* vtxDst = null;
                ushort* idxDst = null;
                if (_vk.MapMemory(_device, frameRenderBuffer.VertexBufferMemory, 0, frameRenderBuffer.VertexBufferSize, 0, (void**)(&vtxDst)) != Result.Success)
                {
                    throw new Exception($"Unable to map device memory");
                }
                if (_vk.MapMemory(_device, frameRenderBuffer.IndexBufferMemory, 0, frameRenderBuffer.IndexBufferSize, 0, (void**)(&idxDst)) != Result.Success)
                {
                    throw new Exception($"Unable to map device memory");
                }
                for (var n = 0; n < drawData.CmdListsCount; n++)
                {
                    ImDrawList* cmdList = drawDataPtr.CmdLists[n];
                    Unsafe.CopyBlock(vtxDst, cmdList->VtxBuffer.Data.ToPointer(), (uint)cmdList->VtxBuffer.Size * (uint)sizeof(ImDrawVert));
                    Unsafe.CopyBlock(idxDst, cmdList->IdxBuffer.Data.ToPointer(), (uint)cmdList->IdxBuffer.Size * sizeof(ushort));
                    vtxDst += cmdList->VtxBuffer.Size;
                    idxDst += cmdList->IdxBuffer.Size;
                }

                Span<MappedMemoryRange> range = stackalloc MappedMemoryRange[2];
                range[0].SType = StructureType.MappedMemoryRange;
                range[0].Memory = frameRenderBuffer.VertexBufferMemory;
                range[0].Size = Vk.WholeSize;
                range[1].SType = StructureType.MappedMemoryRange;
                range[1].Memory = frameRenderBuffer.IndexBufferMemory;
                range[1].Size = Vk.WholeSize;
                if (_vk.FlushMappedMemoryRanges(_device, 2, range) != Result.Success)
                {
                    throw new Exception($"Unable to flush memory to device");
                }
                _vk.UnmapMemory(_device, frameRenderBuffer.VertexBufferMemory);
                _vk.UnmapMemory(_device, frameRenderBuffer.IndexBufferMemory);
            }

            // Setup desired Vulkan state
            _vk.CmdBindPipeline(commandBuffer, PipelineBindPoint.Graphics, _pipeline);
            _vk.CmdBindDescriptorSets(commandBuffer, PipelineBindPoint.Graphics, _pipelineLayout, 0, 1, in _descriptorSet, 0, null);

            // Bind Vertex And Index Buffer:
            if (drawData.TotalVtxCount > 0)
            {
                ReadOnlySpan<Buffer> vertexBuffers = stackalloc Buffer[] { frameRenderBuffer.VertexBuffer };
                ulong bindVertexOffset = 0;
                _vk.CmdBindVertexBuffers(commandBuffer, 0, 1, vertexBuffers, (ulong*)Unsafe.AsPointer(ref bindVertexOffset));
                _vk.CmdBindIndexBuffer(commandBuffer, frameRenderBuffer.IndexBuffer, 0, IndexType.Uint16);
            }

            // Setup viewport:
            Viewport viewport;
            viewport.X = 0;
            viewport.Y = 0;
            viewport.Width = fbWidth;
            viewport.Height = fbHeight;
            viewport.MinDepth = 0.0f;
            viewport.MaxDepth = 1.0f;
            _vk.CmdSetViewport(commandBuffer, 0, 1, &viewport);

            // Setup scale and translation:
            // Our visible imgui space lies from draw_data.DisplayPps (top left) to draw_data.DisplayPos+data_data.DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.
            Span<float> scale = stackalloc float[2];
            scale[0] = 2.0f / drawData.DisplaySize.X;
            scale[1] = 2.0f / drawData.DisplaySize.Y;
            Span<float> translate = stackalloc float[2];
            translate[0] = -1.0f - drawData.DisplayPos.X * scale[0];
            translate[1] = -1.0f - drawData.DisplayPos.Y * scale[1];
            _vk.CmdPushConstants(commandBuffer, _pipelineLayout, ShaderStageFlags.VertexBit, sizeof(float) * 0, sizeof(float) * 2, scale);
            _vk.CmdPushConstants(commandBuffer, _pipelineLayout, ShaderStageFlags.VertexBit, sizeof(float) * 2, sizeof(float) * 2, translate);

            // Will project scissor/clipping rectangles into framebuffer space
            var clipOff = drawData.DisplayPos;         // (0,0) unless using multi-viewports
            var clipScale = drawData.FramebufferScale; // (1,1) unless using retina display which are often (2,2)

            // Render command lists
            // (Because we merged all buffers into a single one, we maintain our own offset into them)
            var vertexOffset = 0;
            var indexOffset = 0;
            for (var n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawList* cmdList = drawDataPtr.CmdLists[n];
                for (var cmdI = 0; cmdI < cmdList->CmdBuffer.Size; cmdI++)
                {
                    ref var pcmd = ref cmdList->CmdBuffer.Ref<ImDrawCmd>(cmdI);

                    // Project scissor/clipping rectangles into framebuffer space
                    Vector4 clipRect;
                    clipRect.X = (pcmd.ClipRect.X - clipOff.X) * clipScale.X;
                    clipRect.Y = (pcmd.ClipRect.Y - clipOff.Y) * clipScale.Y;
                    clipRect.Z = (pcmd.ClipRect.Z - clipOff.X) * clipScale.X;
                    clipRect.W = (pcmd.ClipRect.W - clipOff.Y) * clipScale.Y;

                    if (!(clipRect.X < fbWidth) || !(clipRect.Y < fbHeight) ||
                        clipRect is not {Z: >= 0.0f, W: >= 0.0f}) continue;

                    // Negative offsets are illegal for vkCmdSetScissor
                    if (clipRect.X < 0.0f)
                        clipRect.X = 0.0f;
                    if (clipRect.Y < 0.0f)
                        clipRect.Y = 0.0f;

                    // Apply scissor/clipping rectangle
                    var scissor = new Rect2D();
                    scissor.Offset.X = (int)clipRect.X;
                    scissor.Offset.Y = (int)clipRect.Y;
                    scissor.Extent.Width = (uint)(clipRect.Z - clipRect.X);
                    scissor.Extent.Height = (uint)(clipRect.W - clipRect.Y);
                    _vk.CmdSetScissor(commandBuffer, 0, 1, &scissor);

                    // Draw
                    _vk.CmdDrawIndexed(commandBuffer, pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)indexOffset, (int)pcmd.VtxOffset + vertexOffset, 0);
                }
                indexOffset += cmdList->IdxBuffer.Size;
                vertexOffset += cmdList->VtxBuffer.Size;
            }

            _vk.CmdEndRenderPass(commandBuffer);
        }

        unsafe void CreateOrResizeBuffer(ref Buffer buffer, ref DeviceMemory bufferMemory, ref ulong bufferSize, ulong newSize, BufferUsageFlags usage)
        {
            if (buffer.Handle != default)
            {
                _vk.DestroyBuffer(_device, buffer, default);
            }
            if (bufferMemory.Handle != default)
            {
                _vk.FreeMemory(_device, bufferMemory, default);
            }

            var sizeAlignedVertexBuffer = ((newSize - 1) / _bufferMemoryAlignment + 1) * _bufferMemoryAlignment;
            var bufferInfo = new BufferCreateInfo
            {
                SType = StructureType.BufferCreateInfo,
                Size = sizeAlignedVertexBuffer,
                Usage = usage,
                SharingMode = SharingMode.Exclusive
            };

            if (_vk.CreateBuffer(_device, in bufferInfo, default, out buffer) != Result.Success)
            {
                throw new Exception($"Unable to create a device buffer");
            }

            _vk.GetBufferMemoryRequirements(_device, buffer, out var req);
            _bufferMemoryAlignment = (_bufferMemoryAlignment > req.Alignment) ? _bufferMemoryAlignment : req.Alignment;
            var allocInfo = new MemoryAllocateInfo
            {
                SType = StructureType.MemoryAllocateInfo,
                AllocationSize = req.Size,
                MemoryTypeIndex = GetMemoryTypeIndex(_vk, MemoryPropertyFlags.HostVisibleBit, req.MemoryTypeBits)
            };

            if (_vk.AllocateMemory(_device, &allocInfo, default, out bufferMemory) != Result.Success)
            {
                throw new Exception($"Unable to allocate device memory");
            }

            if (_vk.BindBufferMemory(_device, buffer, bufferMemory, 0) != Result.Success)
            {
                throw new Exception($"Unable to bind device memory");
            }
            bufferSize = req.Size;
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public unsafe void Dispose()
        {
            _view.Resize -= WindowResized;
            _keyboard.KeyChar -= OnKeyChar;

            for (uint n = 0; n < _mainWindowRenderBuffers.Count; n++)
            {
                _vk.DestroyBuffer(_device, _mainWindowRenderBuffers.FrameRenderBuffers[n].VertexBuffer, default);
                _vk.FreeMemory(_device, _mainWindowRenderBuffers.FrameRenderBuffers[n].VertexBufferMemory, default);
                _vk.DestroyBuffer(_device, _mainWindowRenderBuffers.FrameRenderBuffers[n].IndexBuffer, default);
                _vk.FreeMemory(_device, _mainWindowRenderBuffers.FrameRenderBuffers[n].IndexBufferMemory, default);
            }

            _vk.DestroyShaderModule(_device, _shaderModuleVert, default);
            _vk.DestroyShaderModule(_device, _shaderModuleFrag, default);
            _vk.DestroyImageView(_device, _fontView, default);
            _vk.DestroyImage(_device, _fontImage, default);
            _vk.FreeMemory(_device, _fontMemory, default);
            _vk.DestroySampler(_device, _fontSampler, default);
            _vk.DestroyDescriptorSetLayout(_device, _descriptorSetLayout, default);
            _vk.DestroyPipelineLayout(_device, _pipelineLayout, default);
            _vk.DestroyPipeline(_device, _pipeline, default);
            _vk.DestroyDescriptorPool(_vk.CurrentDevice!.Value, _descriptorPool, default);
            _vk.DestroyRenderPass(_vk.CurrentDevice.Value, _renderPass, default);

            ImGuiNET.ImGui.DestroyContext();
        }

        struct FrameRenderBuffer
        {
            public DeviceMemory VertexBufferMemory;
            public DeviceMemory IndexBufferMemory;
            public ulong VertexBufferSize;
            public ulong IndexBufferSize;
            public Buffer VertexBuffer;
            public Buffer IndexBuffer;
        };

        unsafe struct WindowRenderBuffers
        {
            public uint Index;
            public uint Count;
            public FrameRenderBuffer* FrameRenderBuffers;
        };
    }
}
