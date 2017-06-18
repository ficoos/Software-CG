using System;
using System.Collections.Generic;
using SDL2;
using SDLForms;

namespace PolygonEditor {
public class Editor {
	Graphics _gfx;
	List<IDrawable> _objects;
	Polygon _l;

	public Editor(Window wnd) {
		_gfx = new Graphics(wnd.CreateRenderer());
		_objects = new List<IDrawable>();
		_l = null;
		_gfx.Clear(Colors.White);
		_gfx.Commit();
		wnd.WindowResized += (
			object sender,
			WindowResizedEventArgs e
		) => Render();


		wnd.MouseButtonDown += delegate(
			object sender,
			MouseButtonDownEventArgs e
		) {
			switch (e.Button) {
			case MouseButton.Left:
				if (_l == null) {
					_l = new Polygon(Colors.Black);
					_l.AddPoint(new Point(e.X, e.Y));
					this._objects.Add(_l);
				}
				_l.AddPoint(new Point(e.X, e.Y));
				break;
			case MouseButton.Right:
				_l = null;
				break;

			}

			Render();
		};

		wnd.MouseMotion += delegate(object sender, MouseMotionEventArgs e) {
			if (_l == null || _l.Count == 0) {
				return;
			}

			_l.RemovePoint();
			_l.AddPoint(new Point(e.X, e.Y));

			Render();
		};

		wnd.KeyDown += delegate(object sender, KeyDownEventArgs e) {
			switch (e.KeySym.scancode) {
			case SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE:
				_objects.Clear();
				_l = null;
				break;
			case SDL.SDL_Scancode.SDL_SCANCODE_A:
				if (_gfx.LineAlgorithm == BresenhamLine.DrawLine) {
					_gfx.LineAlgorithm = DDALine.DrawLine;
					Console.WriteLine("Line Algorithm: DDALine");
				} else if (_gfx.LineAlgorithm == DDALine.DrawLine) {
					_gfx.LineAlgorithm = null;
					Console.WriteLine("Line Algorithm: Built In");
				} else {
					_gfx.LineAlgorithm = BresenhamLine.DrawLine;
					Console.WriteLine("Line Algorithm: BresnhamLine");
				}
				break;
			}

			Render();
		};
	}

	void Render() {
		_gfx.Clear(Colors.White);

		var start = DateTime.Now;
		foreach (var obj in _objects) {
			obj.Draw(_gfx);
		}
		Console.WriteLine(
			"Render took {0}ms",
			(DateTime.Now - start).TotalMilliseconds
		);

		_gfx.Commit();
	}

	public static void Main() {
		var app = Application.GetInstance();
		var wnd = new Window("Polygon Editor");
		new Editor(wnd);
		app.MainLoop();
	}
}
}

