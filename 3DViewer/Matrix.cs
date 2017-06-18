using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Linq.Expressions;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;

namespace ModelViewer {
public class Matrix {
	private double[,] _mat;

	public Matrix(double[,] data) {
		_mat = new double[,] {
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 }
		};

		for (int x = 0; x < 4; x++) {
			for (int y = 0; y < 4; y++) {
				_mat[x, y] = data[x, y];
			}
		}
	}

	public Matrix() {
		_mat = new double[,] {
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 }
		};
	}

	public double this [int y, int x] {
		get {
			return _mat[y, x];
		}
		set {
			_mat[y, x] = value;
		}
	}

	public static Matrix operator *(Matrix a, Matrix b) {
		var m = new Matrix();
		for (int x = 0; x < 4; x++) {
			for (int y = 0; y < 4; y++) {
				m[y, x] = 0;
				for (int i = 0; i < 4; i++) {
					m[y, x] += a[y, i] * b[i, x];
				}
			}
		}

		return m;
	}

	public static Vector3D operator *(Matrix a, Vector3D b) {
		var v = new Vector3D(0, 0, 0);
		v.X = a[0, 0] * b.X + a[0, 1] * b.Y + a[0, 2] * b.Z + a[0, 3] * b.W;
		v.Y = a[1, 0] * b.X + a[1, 1] * b.Y + a[1, 2] * b.Z + a[1, 3] * b.W;
		v.Z = a[2, 0] * b.X + a[2, 1] * b.Y + a[2, 2] * b.Z + a[2, 3] * b.W;
		v.W = a[3, 0] * b.X + a[3, 1] * b.Y + a[3, 2] * b.Z + a[3, 3] * b.W;

		return v;
	}

	public static Matrix Translation(double x, double y, double z) {
		return new Matrix(new double[,] {
			{ 1, 0, 0, x },
			{ 0, 1, 0, y },
			{ 0, 0, 1, z },
			{ 0, 0, 0, 1 }
		});
	}

	public static Matrix RotationX(double theta) {
		var c = Math.Cos(theta);
		var s = Math.Sin(theta);
		return new Matrix(new double[,] {
			{ c, 0, -s, 0 },
			{ 0, 1, 0, 0 },
			{ s, 0, c, 0 },
			{ 0, 0, 0, 1 }
		});
	}

	public static Matrix RotationY(double theta) {
		var c = Math.Cos(theta);
		var s = Math.Sin(theta);
		return new Matrix(new double[,] {
			{ 1, 0, 0, 0 },
			{ 0, c, s, 0 },
			{ 0, -s, c, 0 },
			{ 0, 0, 0, 1 }
		});
	}

	public static Matrix RotationZ(double theta) {
		var c = Math.Cos(theta);
		var s = Math.Sin(theta);
		return new Matrix(new double[,] {
			{ c, s, 0, 0 },
			{ -s, c, 0, 0 },
			{ 0, 0, 1, 0 },
			{ 0, 0, 0, 1 }
		});
	}

	public static Matrix Scale(double x, double y, double z) {
		return new Matrix(new double[,] {
			{ x, 0, 0, 0 },
			{ 0, y, 0, 0 },
			{ 0, 0, z, 0 },
			{ 0, 0, 0, 1 },
		});
	}

	public override string ToString() {
		string res = "";
		for (int y = 0; y < 4; y++) {
			res += string.Format("{0}", _mat[0, y]);
			for (int x = 1; x < 4; x++) {
				res += string.Format("\t{0}", _mat[y, x]);
			}
			res += "\n";
		}
		return res;
	}

	public static Matrix Identity() {
		return new Matrix(new double[,] {
			{ 1, 0, 0, 0 },
			{ 0, 1, 0, 0 },
			{ 0, 0, 1, 0 },
			{ 0, 0, 0, 1 }
		});
	}

	public static Matrix PerspectiveProjection(double x, double y, double near, double far) {
		var d = far - near;
		var c = Matrix.Translation(x, y, -near);
		var p = new Matrix(new double[,] {
			{ 1, 0, 0, 0 },
			{ 0, 1, 0, 0 },
			{ 0, 0, 1, 0 },
			{ 0, 0, 1 / d, 1 }
		});
		return c * p;
	}

	public static Matrix ParallelProjection(double x, double y, double alpha, double beta) {
		double l1 = 1d / Math.Tan(beta);
		var c = Matrix.Translation(x, y, 0);
		var p = new Matrix(new double[,] {
			{ 1, 0, l1 * Math.Cos(alpha), 0 },
			{ 0, 1, l1 * Math.Sin(alpha), 0 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 1 }
		});

		return c * p;
	}
}
}
