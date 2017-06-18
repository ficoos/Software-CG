using System.Collections.Generic;

namespace PolygonEditor {
public struct BoundingBox {
	public float X1 { get; set; }
	public float Y1 { get; set; }
	public float X2 { get; set; }
	public float Y2 { get; set; }

	public static BoundingBox FromPoints(IEnumerable<Point> points) {
		float minX = float.MaxValue, minY = float.MaxValue;
		float maxX = float.MinValue, maxY = float.MinValue;

		foreach(var p in points) {
			if (p.X < minX) {
				minX = p.X;
			}
			if (p.X > maxX) {
				maxX = p.X;
			}
			if (p.Y < minY) {
				minY = p.Y;
			}
			if (p.Y > maxY) {
				maxY = p.Y;
			}
		}

		return new BoundingBox {
			X1 = minX,
			X2 = maxX,
			Y1 = minY,
			Y2 = maxY,
		};
	}
}
}

