using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection.Emit;
using System.Security.AccessControl;

namespace PolygonEditor {
public class ComparisonComparer<T> : IComparer<T> {
	private readonly Comparison<T> comparison;

	public ComparisonComparer(Comparison<T> comparison) {
		this.comparison = comparison;
	}

	int IComparer<T>.Compare(T x, T y) {
		return comparison(x, y);
	}
}

public class Polygon : IDrawable {
	private readonly List<Point> _points;

	private struct LineInfo {
		public float m;
		public float b;
		public int ymin;
		public int ymax;
		public bool localpeak;
		public Point pt;
		public bool active;
	}

	public Color Color { get; set; }

	public Polygon(Color color) {
		_points = new List<Point>();
		Color = color;
	}


	void _DrawLine(Graphics g, Point a, Point b) {
		int x1 = (int)(a.X + .5);
		int y1 = (int)(a.Y + .5);

		int x2 = (int)(b.X + .5);
		int y2 = (int)(b.Y + .5);

		g.DrawLine(Color, x1, y1, x2, y2);
	}

	public void AddPoint(Point p) {
		_points.Add(p);
	}

	public void RemovePoint() {
		_points.RemoveAt(_points.Count - 1);
	}

	public int Count {
		get {
			return _points.Count;
		}
	}

	private BoundingBox getBoundingBox() {
		return BoundingBox.FromPoints(_points);
	}

	private LineInfo[] getLines(out int n) {
		var lines = new LineInfo[_points.Count];
		var p1 = _points[_points.Count - 1];
		int i = 0;
		foreach (var p2 in _points) {
			int y1 = (int)(p1.Y + .5);
			int y2 = (int)(p2.Y + .5);

			if (y1 == y2) {
				p1 = p2;
				continue;
			}
			if (y1 < y2) {
				lines[i].ymin = y1;
				lines[i].ymax = y2;
			} else {
				lines[i].ymin = y2;
				lines[i].ymax = y1;
			}
			float m = (p2.Y - p1.Y) / (p2.X - p1.X);
			lines[i].m = m;
			if (float.IsInfinity(m)) {
				lines[i].b = p1.X;
			} else {
				lines[i].b = p1.Y - (m * p1.X);
			}

			lines[i].active = true;
			lines[i].pt = p1;
			p1 = p2;
			i++;
		}

		n = i;
		if (n > 1) {
			for(i = 1; i <= n; i++) {
				var pp = lines[(i - 1) % n].pt;
				p1 = lines[i % n].pt;
				var p2 = lines[(i + 1) % n].pt;

				lines[i % n].localpeak = ((p2.Y - p1.Y) * (pp.Y - p1.Y)) > 0;
			}
		}

		Array.Sort(lines, 0, n, new ComparisonComparer<LineInfo>(
			delegate (LineInfo a, LineInfo b) {
				var res = a.ymin.CompareTo(b.ymin);
				return res != 0 ? res : a.ymax.CompareTo(b.ymax);
			}));

		return lines;
	}

	public void Fill(Graphics g) {
		var bb = getBoundingBox();

		var ystart = (int)(bb.Y1);
		var yend = (int)(bb.Y2) + 1;
		var xstart = (int)(bb.X1);
		var xend = (int)(bb.X2) + 1;

		int nlines;
		var lines = getLines(out nlines);
		var intersections = new int[_points.Count];

		var top = 0;

		for (int y = ystart; y < yend; y++) {
			int n = 0;
			for (int lineidx = top; lineidx < nlines; lineidx++) {
				var li = lines[lineidx];
				if (!li.active) {
					continue;
				}
				if (y < li.ymin) {
					break;
				} else if (y <= li.ymax) {
					if (y == li.ymin || y == li.ymax) {
						if (!li.localpeak && y == (int)(li.pt.Y + .5)) {
							continue;
						}
					}
					intersections[n] = (int)(
					    (
					        float.IsInfinity(li.m) ? li.b : (y - li.b) / li.m
					    ) + .5
					);

					n++;
				} else if (lineidx == top) {
					top++;
				} else {
					li.active = false;
				}
			}

			Array.Sort(intersections, 0, n);
			int i = 0;
			var penDown = false;
			for (int x = xstart; x < xend; x++) {
				while (i < n && x == intersections[i]) {
					penDown = !penDown;
					i++;
				}

				if (penDown) {
					g.SetPixel(Colors.Red, x, y);
				}
			}
		}
	}

	public void Draw(Graphics g) {
		if (_points.Count == 2) {
			_DrawLine(g, _points[0], _points[1]);
		} else {
			Fill(g);

			for (int i = 0; i < _points.Count; i++) {
				_DrawLine(
					g,
					_points[i],
					_points[(i + 1) % _points.Count]
				);
			}
		}
	}

}
}

