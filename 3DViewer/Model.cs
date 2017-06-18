using System;
using System.Collections.Generic;
using System.IO;

namespace ModelViewer {
public class Model {
	public List<Vector3D> Vertices { get; private set; }

	public List<Vector3D> Normals { get; private set; }

	public List<int[,]> Faces { get; private set; }

	public string Name { get; set; }



	public Model() {
		Vertices = new List<Vector3D>();
		Normals = new List<Vector3D>();
		Faces = new List<int[,]>();
	}

	public static Model FromFile(string path) {
		var model = new Model();
		model.Name = Path.GetFileNameWithoutExtension(path);
		foreach (var line in File.ReadLines(path)) {
			var args = line.Split(' ');
			if (args[0].Equals("v", StringComparison.OrdinalIgnoreCase)) {
				model.Vertices.Add(new Vector3D(
					double.Parse(args[1]) * 30,
					-double.Parse(args[2]) * 30,
					double.Parse(args[3]) * 30
				));
			}
			if (args[0].Equals("vn", StringComparison.OrdinalIgnoreCase)) {
				model.Normals.Add(new Vector3D(
					double.Parse(args[1]) * 30,
					-double.Parse(args[2]) * 30,
					double.Parse(args[3]) * 30
				));
			}
			if (args[0].Equals("f", StringComparison.OrdinalIgnoreCase)) {
				// Triangulate
				if (args.Length == 4) {
					var lst = new int[3, 2];
					for (int i = 1; i < args.Length; i++) {
						var c = args[i].Split('/');
						lst[(i - 1), 0] = (int.Parse(c[0]) - 1);
						lst[(i - 1), 1] = (int.Parse(c[2]) - 1);
					}
					model.Faces.Add(lst);
				}
			}

		}

		return model;
	}
}
}

