namespace PolygonEditor {
public class Dot : IDrawable {
	public Point Position { get; set; }
	public Color Color { get; set; }

	public Dot() : this(new Point(0, 0)) {
	}

	public Dot(Point pos) : this(pos, Colors.Black) {
	}

	public Dot(Point pos, Color color) {
		Color = color;
		Position = pos;
	}

	public Dot(float x = 0, float y = 0) : this(new Point(x, y)) {
	}

	public Dot(float x, float y, Color color) : this(new Point(x, y), color) {
	}

	public void Draw(Graphics g) {
		g.SetPixel(
			Color,
			(int)(Position.X + .5),
			(int)(Position.Y + .5)
		);

	}
}
}

