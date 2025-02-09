﻿// From: https://github.com/dotnet/Silk.NET/tree/main/src/Lab/Experiments/ImGuiVulkan
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Silk.NET.Vulkan.Extensions.ImGui
{
    public readonly struct ImGuiFontConfig
    {
        public ImGuiFontConfig(string fontPath, int fontSize)
        {
            if (fontSize <= 0) throw new ArgumentOutOfRangeException(nameof(fontSize));
            FontPath = fontPath ?? throw new ArgumentNullException(nameof(fontPath));
            FontSize = fontSize;
        }

        public string FontPath { get; }
        public int FontSize { get; }
    }
}