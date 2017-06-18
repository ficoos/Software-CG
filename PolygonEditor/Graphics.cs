using System;
using SDLForms;

namespace PolygonEditor {
public class Graphics {
	readonly Renderer _renderer;
	ulong _pixelCount;

	public Action<Graphics, Color, int, int ,int, int> LineAlgorithm { get; set; }

	internal Graphics(Renderer renderer) {
		_renderer = renderer;
		LineAlgorithm = BresenhamLine.DrawLine;
		_pixelCount = 0;
	}

	public void Clear(Color? color = null) {
		if (color == null) {
			_SetColor(Colors.Black);
		} else {
			_SetColor((Color)color);
		}
		_renderer.Clear();
		_pixelCount = 0;
	}

	void _SetColor(Color color) {
		_renderer.SetColor(
			color.R,
			color.G,
			color.B,
			color.A
		);
	}

	public void SetPixel(Color color, int x, int y) {
		_SetColor(color);
		_renderer.DrawPoint(x, y);
		_pixelCount++;
	}

	public void DrawLine(Color color, int x1, int y1, int x2, int y2) {
		if (LineAlgorithm == null) {
			_SetColor(color);
			_renderer.DrawLine(x1, y1, x2, y2);
		} else {
			LineAlgorithm(this, color, x1, y1, x2, y2);
		}
	}

	public void DrawRectangle(Color color, int x, int y, int w, int h) {
		_SetColor(color);
		_renderer.DrawRectangle(x, y, w, h);
	}

	public void Commit() {
		_renderer.Present();
		Console.WriteLine("{0} pixels drawn", _pixelCount);
	}
}
}

