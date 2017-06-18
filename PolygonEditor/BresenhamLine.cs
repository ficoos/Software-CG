
namespace PolygonEditor {
public static class BresenhamLine {
	static void Transform(int oct, int x, int y, out int ox, out int oy) {
		switch (oct) {
		case 0:
			ox = x;
			oy = y;
			break;
		case 1:
			ox = y;
			oy = x;
			break;
		case 2:
			ox = -y;
			oy = x;
			break;
		case 3:
			ox = -x;
			oy = y;
			break;
		case 4:
			ox = -x;
			oy = -y;
			break;
		case 5:
			ox = -y;
			oy = -x;
			break;
		case 6:
			ox = y;
			oy = -x;
			break;
		case 7:
			ox = x;
			oy = -y;
			break;
		default:
			ox = x;
			oy = y;
			break;
		}
	}

	static void BresnhamOctet(
		Graphics g,
		Color c,
		int oct,
		int x1, 
		int y1,
		int x2,
		int y2
	) {
		int tx, ty;

		if (oct == 2) {
			Transform(6, x1, y1, out x1, out y1);
			Transform(6, x2, y2, out x2, out y2);
		} else if (oct == 6) {
			Transform(2, x1, y1, out x1, out y1);
			Transform(2, x2, y2, out x2, out y2);
		} else {
			Transform(oct, x1, y1, out x1, out y1);
			Transform(oct, x2, y2, out x2, out y2);
		}

		int dx = x2 - x1;
		int dy = y2 - y1;

		int tdy = 2 * dy;
		int tdx = 2 * dx;
		int D = tdy - dx;

		Transform(oct, x1, y1, out tx, out ty);
		g.SetPixel(c, tx, ty);

		int y = y1;
		for (int x = (x1 + 1); x <= x2; x++) {
			if (D > 0) {
				y++;
				D += tdy - tdx;
			} else {
				D += tdy;
			}
			Transform(oct, x, y, out tx, out ty);
			g.SetPixel(c, tx, ty);
		}
	}

	static int getOctant(int x1, int y1, int x2, int y2) {
		int dx = x2 - x1;
		int dy = y2 - y1;
		double m = (dy / (float)(dx));
		if (m <= -1) {
			return (dy < 0) ? 6 : 2;
		} else if (m < 0) {
			return (dy < 0) ? 7 : 3;
		} else if (m >= 1) {
			return (dx < 0) ? 5 : 1;
		} else { //if (m > 0) {
			return (dx < 0) ? 4 : 0;
		}
	}

	public static void DrawLine(Graphics g, Color c, int x1, int y1, int x2, int y2) {
		int oct = getOctant(x1, y1, x2, y2);

		BresnhamOctet(
			g, c, oct,
			x1,	y1,
			x2, y2
		);

	}

}
}

