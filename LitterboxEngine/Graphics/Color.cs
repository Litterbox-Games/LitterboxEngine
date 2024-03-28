using System.Runtime.InteropServices;

namespace LitterboxEngine.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RgbaFloat
{
    public float R;
    public float G;
    public float B;
    public float A;

    public RgbaFloat(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public RgbaFloat(byte r, byte g, byte b, byte a) : this(r / 255f, g / 255f, b / 255f, a / 255f) { }
    
    public static implicit operator RgbaFloat(System.Drawing.Color color)
    {
        return new RgbaFloat(color.R, color.G, color.B, color.A);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RgbaByte
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public RgbaByte(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
    
    public static implicit operator RgbaByte(System.Drawing.Color color)
    {
        return new RgbaByte(color.R, color.G, color.B, color.A);
    }
}