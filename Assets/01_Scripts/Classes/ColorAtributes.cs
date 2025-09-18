using System.Collections.Generic;
using UnityEngine;


public enum ColorAtribute { Red, Blue, Green, Black, White, Yellow, Purple }

public static class ColorAtributes
{
    // NOTE: use ColorAtribute (same exact spelling as the enum)
    public static readonly Dictionary<ColorAtribute, Color> Predefined = new Dictionary<ColorAtribute, Color>
    {
        { ColorAtribute.Red,   From255(255,   0,   0) },
        { ColorAtribute.Blue,  From255(0,   120, 255) },
        { ColorAtribute.Green, From255(0,   200,  80) },
        { ColorAtribute.Black, Color.black },
        { ColorAtribute.White, Color.white },
        { ColorAtribute.Yellow,From255(255, 220,   0) },
        { ColorAtribute.Purple, From255(106, 13, 173) },
    };

    // safe getter (avoids KeyNotFoundException)
    public static Color Get(ColorAtribute attr)
    {
        if (Predefined.TryGetValue(attr, out var c)) return c;
        return Color.white; // fallback
    }

    // helper: create Color from 0-255 ints
    public static Color From255(int r, int g, int b, int a = 255)
        => new Color(r / 255f, g / 255f, b / 255f, a / 255f);
}

// extension so you can do ColorAtribute.Yellow.ToColor()
public static class ColorAtributeExtensions
{
    public static Color ToColor(this ColorAtribute attr) => ColorAtributes.Get(attr);
}