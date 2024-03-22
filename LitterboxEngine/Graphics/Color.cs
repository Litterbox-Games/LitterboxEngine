using System.Runtime.InteropServices;

namespace LitterboxEngine.Graphics;

// TODO: Find a better name? Maybe RgbaFloat?
// I feel like it might be confusing when someone does Color.R = x because they may use 255 instead of 1
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Color
{
    public float R;
    public float G;
    public float B;
    public float A;

    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(byte r, byte g, byte b, byte a) : this(r / 255f, g / 255f, b / 255f, a / 255f) { }
    
    // Implicit conversion from System.Drawing.Color to your custom Color struct
    public static implicit operator Color(System.Drawing.Color color)
    {
        return new Color(color.R, color.G, color.B, color.A);
    }
}