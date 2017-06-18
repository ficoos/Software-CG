using System;

namespace PolygonEditor {
public static class DDALine {
	public static void DrawLine(Graphics g, Color c, int x1, int y1, int x2, int y2) {
		double dx = x2 - x1;
		double dy = y2 - y1;

		double dmax = Math.Max(
			              Math.Abs(dx),
			              Math.Abs(dy)
		              );

		double sx = dx / dmax;
		double sy = dy / dmax;

		double x = x1;
		double y = y1;

		for (int i = 0; i < dmax; i++) {
			g.SetPixel(
				c,
				(int)(x + .5),
				(int)(y + .5)
			);

			x += sx;
			y += sy;
		}
	}
}
}

