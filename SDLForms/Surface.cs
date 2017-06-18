using System;
using SDL2;

namespace SDLForms {
public class Surface :IDisposable {
	IntPtr _sfc;

	public Surface(int width, int height, int depth) {
		_sfc = new Surface(width, height, depth);
		_format = SDL.SDL_GetSurfaceColorMod
	}

	Renderer CreateRenderer() {
		return new Renderer(_sfc);
	}

	public


	public void Dispose() {
		SDL.SDL_FreeSurface(_sfc);
	}

	#endregion
}
}

