using System;
using SDLForms;
using System.Collections.Generic;
using SDL2;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.Remoting.Channels;
using System.Configuration.Assemblies;

namespace ModelViewer {
public class Viewer {

	private enum Projections {
		Perspective,
		Isometric,
		Cavalier,
		Cabinet,
		Orthogonal}

	;

	private Matrix _rotate = Matrix.Identity();
	private Matrix _scale = Matrix.Identity();
	private Matrix _translate = Matrix.Identity();

	private Matrix _projection;
	private Projections _currentProjection = Projections.Perspective;
	private bool _isPerspectiveProjection = true;
	private double _horizon = 1000;
	private double _pp_alpah = 0.5;
	private double _pp_beta = 63.4;
	private bool _update = true;
	private bool _wireframe = false;
	private bool _fill = true;
	private double[,] zbuffer;
	private double near = -1000;

	/* Lighting model */
	double ka = 0.1;
	double kd = 0.001;
	Vector3D ia = new Vector3D(1, 1, 1);
	Vector3D[,] Lights = {
		{ new Vector3D(50, 50, 150), new Vector3D(0, 0, 1) },
		{ new Vector3D(0, 0, 150), new Vector3D(.8, .8, .8) }
	};

	private Model _model;
	private List<Model> _models = new List<Model>();

	Renderer _renderer;

	Window _wnd;

	public Viewer(Window wnd) {
		wnd.Paint += (sender, e) => Render();

		wnd.KeyDown += delegate(object sender, KeyDownEventArgs e) {
			switch (e.KeySym.scancode) {
			case SDL.SDL_Scancode.SDL_SCANCODE_R:
				_rotate = Matrix.Identity();
				_translate = Matrix.Identity();
				_scale = Matrix.Identity();
				break;
			case SDL.SDL_Scancode.SDL_SCANCODE_LEFTBRACKET:
				_scale = Matrix.Scale(1 / 1.2, 1 / 1.2, 1 / 1.2) * _scale;
				break;
			case SDL.SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET:
				_scale = Matrix.Scale(1.2, 1.2, 1.2) * _scale;
				break;
			case SDL.SDL_Scancode.SDL_SCANCODE_KP_PLUS:
				_horizon *= 1.2;
				RefreshProjection();
				break;
			case SDL.SDL_Scancode.SDL_SCANCODE_KP_MINUS:
				_horizon /= 1.2;
				RefreshProjection();
				break;
			case SDL.SDL_Scancode.SDL_SCANCODE_F:
				_fill = !_fill;
				break;
			case SDL.SDL_Scancode.SDL_SCANCODE_W:
				_wireframe = !_wireframe;
				break;
			case SDL.SDL_Scancode.SDL_SCANCODE_P:
				switch (_currentProjection) {
				case Projections.Perspective:
					_isPerspectiveProjection = false;
					_pp_alpah = 45;
					_pp_beta = 45;
					_currentProjection = Projections.Isometric;
					break;
				case Projections.Isometric:
					_isPerspectiveProjection = false;
					_pp_alpah = 0.5;
					_pp_beta = 66.4;
					_currentProjection = Projections.Cabinet;
					break;
				case Projections.Cabinet:
					_isPerspectiveProjection = false;
					_pp_alpah = 1;
					_pp_beta = 45;
					_currentProjection = Projections.Cavalier;
					break;
				case Projections.Cavalier:
					_isPerspectiveProjection = false;
					_pp_alpah = 0;
					_pp_beta = 90;
					_currentProjection = Projections.Orthogonal;
					break;
				case Projections.Orthogonal:
					_isPerspectiveProjection = true;
					_currentProjection = Projections.Perspective;
					break;
				}
				RefreshProjection();
				break;
			case SDL.SDL_Scancode.SDL_SCANCODE_M:
				_models.RemoveAt(0);
				_models.Add(_model);
				_model = _models[0];
				wnd.Title = string.Format("3D Viewer ({0})", _model.Name);
				break;
			}
			_update = true;
		};
		_wnd = wnd;
		foreach (var path in Directory.GetFiles("Models")) {
			_models.Add(Model.FromFile(path));
		}
		_model = _models[0];
		_renderer = wnd.CreateRenderer();
		wnd.WindowResized += delegate(object sender, WindowResizedEventArgs e) {
			RefreshProjection();
			_update = true;
		};
		RefreshProjection();
		wnd.MouseMotion += delegate(object sender, MouseMotionEventArgs e) {
			if (e.Button == MouseButton.Left) {
				var dist = _translate[2, 3];
				var scale = 1 - (-dist / _horizon);
				_translate *= Matrix.Translation(
					e.RelativeX * scale,
					e.RelativeY * scale,
					0
				);
				_update = true;
			} else if (e.Button == MouseButton.Right) {
				var rotx = Matrix.RotationX(-e.RelativeX * (Math.PI / 180d));
				var roty = Matrix.RotationY(e.RelativeY * (Math.PI / 180d));
				var rot = rotx * roty;
				_rotate = rot * _rotate;
				_update = true;
			} else {
				Lights[0, 0].X = e.X - _wnd.Width / 2;
				Lights[0, 0].Y = e.Y - _wnd.Height / 2;
				_update = true;
			}
		};

		wnd.MouseWheel += delegate(object sender, MouseWheelEventArgs e) {
			_translate = Matrix.Translation(0, 0, 100 * e.Y) * _translate;
			_update = true;
			return;
		};
		Render();
	}

	public void RefreshProjection() {
		if (_isPerspectiveProjection) {
			_projection = Matrix.PerspectiveProjection(
				_wnd.Width / 2d, _wnd.Height / 2d,
				near, _horizon
			);
		} else {
			_projection = Matrix.ParallelProjection(
				_wnd.Width / 2d, _wnd.Height / 2d,
				_pp_alpah * Math.PI / 180,
				_pp_beta * Math.PI / 180
			);
		}
	}

	public static bool IsCulled(Vector3D p1, Vector3D p2, Vector3D p3) {
		var a = p2.X - p1.X;
		var b = p3.X - p1.X;
		var c = p2.Y - p1.Y;
		var d = p3.Y - p1.Y;
		var det = (a * d) - (b * c);
		return det > 0;
	}

	public bool FilterFace(Vector3D[] vertices, Vector3D[] normals, int[,] face) {
		var v1 = vertices[face[0, 0]];
		var v2 = vertices[face[1, 0]];
		var v3 = vertices[face[2, 0]];
		return (v1.Z > near && v2.Z > near && v3.Z > near);
	}

	public void RenderFace(Vector3D[] vertices, Vector3D[] normals, Vector3D[] colors, int[,] face) {
		var v1 = vertices[face[0, 0]];
		var v2 = vertices[face[1, 0]];
		var v3 = vertices[face[2, 0]];

		var n1 = normals[face[0, 1]];
		var n2 = normals[face[1, 1]];
		var n3 = normals[face[2, 1]];

		var c1 = colors[face[0, 1]];
		var c2 = colors[face[1, 1]];
		var c3 = colors[face[2, 1]];

		if (Viewer.IsCulled(v1, v2, v3)) {
			return;
		}
		var w = _wnd.Width;
		var h = _wnd.Height;

		if (
			(v1.X < 0 || v1.Y < 0 || v1.X > w || v1.Y > h)
			&& (v2.X < 0 || v2.Y < 0 || v2.X > w || v2.Y > h)
			&& (v3.X < 0 || v3.Y < 0 || v3.X > w || v3.Y > h)) {
			return;
		}

		if (_fill) {
			SetColor(c1);
			FillTirangle(v1, v2, v3, n1, n2, n3, c1, c2, c3);
		}
		if (_wireframe) {
			_renderer.SetColor(255, 255, 255, 255);
			DrawTriangle(v1, v2, v3);
		}
	}

	public void DrawTriangle(Vector3D v1, Vector3D v2, Vector3D v3) {
		DrawLine(v1, v2);
		DrawLine(v2, v3);
		DrawLine(v3, v1);
	}

	public void DrawLine(Vector3D v1, Vector3D v2) {
		_renderer.DrawLine(
			(int)(v1.X),
			(int)(v1.Y),
			(int)(v2.X),
			(int)(v2.Y)
		);
	}

	public void Render() {
		if (!_update) {
			return;
		}
		zbuffer = new double[_wnd.Width, _wnd.Height];
		for (int x = 0; x < _wnd.Width; x++) {
			for (int y = 0; y < _wnd.Height; y++) {
				zbuffer[x, y] = double.MinValue;
			}
		}

		var st = DateTime.Now;

		_update = false;
		_renderer.SetColor(0, 0, 0, 255);
		_renderer.Clear();

		var verts = new Vector3D[_model.Vertices.Count];
		var normals = new Vector3D[_model.Normals.Count];
		var transform = _translate * _rotate * _scale;

		// Transform model
		for (int i = 0; i < verts.Length; i++) {
			var nv = transform * _model.Vertices[i];
			verts[i] = nv;
		}

		for (int i = 0; i < normals.Length; i++) {
			var nv = transform * _model.Normals[i];
			nv.Normalize();
			normals[i] = nv;
		}

		var transform_duration = DateTime.Now - st;
		st = DateTime.Now;

		// Pan out faces that will not be drawn
		var faces = new int[_model.Faces.Count];
		var colors = new Vector3D[_model.Normals.Count];
		int nfaces = 0;

		for (int i = 0; i < _model.Faces.Count; i++) {
			if (FilterFace(verts, normals, _model.Faces[i])) {
				faces[nfaces] = i;
				nfaces++;
			}
		}

		for (int i = 0; i < nfaces; i++) {
			var face = _model.Faces[faces[i]];
			for (int j = 0; j < 3; j++) {
				var v = verts[face[j, 0]];
				var n = normals[face[j, 1]];
				var color = ka * ia;
				for (int li = 0; li < Lights.Length / 2; li++) {
					var lpos = Lights[li, 0];
					var ldi = Lights[li, 1];
					var vd = (lpos - v);
					var d = vd.DotProduct(vd) / 100000;
					color += Math.Max(0, (kd * (1 / d) * (lpos.DotProduct(n)))) * ldi;
				}
				colors[face[j, 1]] = color;
			}
		}

		for (int i = 0; i < verts.Length; i++) {
			var nv = _projection * verts[i];
			nv.PerspectiveDevide();
			nv.Z = verts[i].Z;
			verts[i] = nv;
		}


		foreach (var i in faces) {
			RenderFace(verts, normals, colors, _model.Faces[i]);
		}

		for (int i = 0; i < Lights.Length / 2; i++) {
			SetColor(Lights[i, 1]);
			var v = _projection * Lights[i, 0];
			v.PerspectiveDevide();
			_renderer.DrawPoint((int)v.X, (int)v.Y);
		}

		_renderer.Present();
		var render_duration = DateTime.Now - st;
		Console.WriteLine(
			"Transform: {0}, Render {1}",
			transform_duration,
			render_duration
		);
		_wnd.Title = string.Format(
			"3D Viewer ({0}, {1} projection)",
			_model.Name,
			_currentProjection
		);
	}

	public static void Swap(ref Vector3D a, ref Vector3D b) {
		var tmp = a;
		a = b;
		b = tmp;
	}

	public double Clamp(double v, double min, double max) {
		return Math.Max(Math.Min(v, max), min);
	}

	public void SetColor(Vector3D c) {
		_renderer.SetColor(
			(byte)(Clamp(c.X * 255, 0, 255)),
			(byte)(Clamp(c.Y * 255, 0, 255)),
			(byte)(Clamp(c.Z * 255, 0, 255)),
			255
		);
	}

	public void FillTirangle(
		Vector3D p1,
		Vector3D p2,
		Vector3D p3,
		Vector3D n1,
		Vector3D n2,
		Vector3D n3,
		Vector3D c1,
		Vector3D c2,
		Vector3D c3
	) {
		// Sort from top to bottom
		if (p1.Y > p2.Y) {
			Swap(ref p1, ref p2);
			Swap(ref c1, ref c2);
		}

		if (p1.Y > p3.Y) {
			Swap(ref p1, ref p3);
			Swap(ref c1, ref c3);
		}

		if (p2.Y > p3.Y) {
			Swap(ref p2, ref p3);
			Swap(ref c2, ref c3);
		}
			
		var tx = (int)p1.X;
		var ty = (int)p1.Y;
		var tz = p1.Z;
		var tc = c1;

		var my = (int)p2.Y;
		var mx = (int)p2.X;
		var mz = p2.Z;
		var mc = c2;

		var bty = (int)p3.Y;
		var btx = (int)p3.X;
		var btz = p3.Z;
		var btc = c3;

		var dlx = btx - tx;
		var dly = bty - ty;
		var dlz = btz - tz;
		var dlc = btc - tc;
		var slx = (Math.Abs(dly) < .0001) ? 0d : dlx / (double)dly;
		var slz = (Math.Abs(dly) < .0001) ? 0d : dlz / (double)dly;
		var slc = (Math.Abs(dly) < .0001) ? c1 : dlc / (double)dly;

		var dx = mx - tx;
		var dy = my - ty;
		var dz = mz - tz;
		var dc = mc - tc;
		var sx = (Math.Abs(dy) < .0001) ? 0d : dx / (double)dy;
		var sz = (Math.Abs(dy) < .0001) ? 0d : dz / (double)dy;
		var sc = (Math.Abs(dy) < .0001) ? c1 : dc / (double)dy;

		var lx = (double)tx;
		var rx = (double)tx;

		var lz = tz;
		var rz = tz;

		var lc = tc;
		var rc = tc;

		var lsx = sx < slx ? sx : slx;
		var rsx = sx > slx ? sx : slx;

		var lsz = sx < slx ? sz : slz;
		var rsz = sx > slx ? sz : slz;

		var lsc = sx < slx ? sc : slc;
		var rsc = sx > slx ? sc : slc;

		for (var y = ty; y <= my; y++) {
			var z = lz;
			var c = lc;
			var xdz = (rz - lz) / (rx - lx);
			var xdc = (rc - lc) / (rx - lx);
			for (var x = (int)(lx); x <= (int)(rx); x++) {
				try {
					if (zbuffer[x, y] < z) {
						zbuffer[x, y] = z;
						SetColor(c);
						_renderer.DrawPoint(x, y);
					}
				} catch (IndexOutOfRangeException) {
				}
				z += xdz;
				c += xdc;
			}
			lx += lsx;
			lz += lsz;
			lc += lsc;
			rx += rsx;
			rz += rsz;
			rc += rsc;
		}
			
		dx = btx - mx;
		dy = bty - my;
		dz = btz - mz;
		dc = btc - mc;
		sx = (Math.Abs(dy) < .0001) ? 0d : dx / (double)dy;
		sz = (Math.Abs(dy) < .0001) ? 0d : dz / (double)dy;
		sc = (Math.Abs(dy) < .0001) ? c1 : dc / (double)dy;

		lx = btx;
		rx = btx;
		lz = btz;
		rz = btz;
		lc = btc;
		rc = btc;

		lsx = sx > slx ? sx : slx;
		rsx = sx < slx ? sx : slx;

		lsz = sx < slx ? sz : slz;
		rsz = sx > slx ? sz : slz;

		lsc = sx > slx ? sc : slc;
		rsc = sx < slx ? sc : slc;

		for (var y = bty; y > my; y--) {
			var z = lz;
			var c = lc;
			var xdz = (rz - lz) / (rx - lx);
			var xdc = (rc - lc) / (rx - lx);
			for (var x = (int)(lx); x <= (int)(rx); x++) {
				try {
					if (zbuffer[x, y] < z) {
						zbuffer[x, y] = z;
						SetColor(c);
						_renderer.DrawPoint(x, y);
					}
				} catch (IndexOutOfRangeException) {
				}
				z += xdz;
				c += xdc;
			}
			lx -= lsx;
			lz -= lsz;
			lc -= lsc;
			rx -= rsx;
			rz -= rsz;
			rc -= rsc;
		}
	}

	public static void Main() {
		var app = Application.GetInstance();
		var wnd = new Window("3D Model Viewer");
		new Viewer(wnd);
		app.MainLoop();
	}
}
}

