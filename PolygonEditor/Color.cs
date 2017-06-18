using System;
using System.Threading;

namespace PolygonEditor {
public static class Colors {
	public static readonly Color White = new Color(255, 255, 255, 255);
	public static readonly Color Black = new Color(0, 0, 0, 255);
	public static readonly Color Red = new Color(255, 0, 0, 255);
	public static readonly Color Blue = new Color(0, 255, 0, 255);
	public static readonly Color Green = new Color(0, 0, 255, 255);
	public static readonly Color Silver = new Color(0xc0, 0xc0, 0xc0, 255);
}

public struct Color {
	public byte R { get; set; }
	public byte G { get; set; }
	public byte B { get; set; }
	public byte A { get; set; }

	public Color(
		byte r = 255,
		byte b = 255,
		byte g = 255,
		byte a = 255
	) : this() {
		R = r;
		G = g;
		B = b;
		A = a;
	}

}
}

