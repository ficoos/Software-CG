using System;

namespace ModelViewer {
public struct Vector3D {
	public double X, Y, Z, W;

	public override string ToString() {
		return string.Format("({0}, {1}, {2}, {3})", X, Y, Z, W);
	}

	public Vector3D(double x, double y, double z) {
		X = x;
		Y = y;
		Z = z;
		W = 1;
	}

	public static Vector3D operator +(Vector3D a, Vector3D b) {
		return new Vector3D(
			a.X + b.X,
			a.Y + b.Y,
			a.Z + b.Z
		);
	}

	public override int GetHashCode() {
		return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
	}

	public override bool Equals(object obj) {
		if (!(obj is Vector3D))
			return false;

		var b = (Vector3D)obj;
		return this == b;
	}

	public static bool operator ==(Vector3D a, Vector3D b) {
		return (a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W);
	}

	public static bool operator !=(Vector3D a, Vector3D b) {
		return (a.X != b.X || a.Y != b.Y | a.Z != b.Z || a.W != b.W);
	}

	public void Normalize() {
		var length = 1 / Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
		X *= length;
		Y *= length;
		Z *= length;
	}

	public double DotProduct(Vector3D b) {
		return X * b.X + Y * b.Y + Z * b.Z;
	}

	public static Vector3D operator *(Vector3D a, double b) {
		return new Vector3D(
			a.X *= b,
			a.Y *= b,
			a.Z *= b
		);
	}

	public static Vector3D operator *(double a, Vector3D b) {
		return new Vector3D(
			b.X *= a,
			b.Y *= a,
			b.Z *= a
		);
	}

	public static Vector3D operator /(Vector3D a, double b) {
		return new Vector3D(
			a.X / b,
			a.Y / b,
			a.Z / b
		);
	}

	public static Vector3D operator /(double a, Vector3D b) {
		return new Vector3D(
			a / b.X,
			a / b.Y,
			a / b.Z
		);
	}

	public static Vector3D operator -(Vector3D a, Vector3D b) {
		return new Vector3D(
			a.X - b.X,
			a.Y - b.Y,
			a.Z - b.Z
		);
	}

	public void PerspectiveDevide() {
		double comp = 1d / W;
		X = X * comp;
		Y = Y * comp;
		Z = Z * comp;
		W = 1;
	}
}
}

