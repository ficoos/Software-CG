using System;
using SDL2;

namespace SDLForms {
public class Renderer : IDisposable {
	readonly IntPtr _renderer;

	internal Renderer(IntPtr wnd) {
		_renderer = SDL.SDL_CreateRenderer(wnd, -1, 0);
	}

	~Renderer() {
		Application.GetInstance()._queuedActions.Add(
			() => SDL.SDL_DestroyRenderer(_renderer)
		);
	}

	public void SetColor(byte r, byte g, byte b, byte a) {
		SDL.SDL_SetRenderDrawColor(_renderer, r, g, b, a);
	}

	public void Clear() {
		SDL.SDL_RenderClear(_renderer);
	}

	public void Present() {
		SDL.SDL_RenderPresent(_renderer);
	}

	public void DrawPoint(int x, int y) {
		SDL.SDL_RenderDrawPoint(_renderer, x, y);
	}

	public void DrawLine(int x1, int y1, int x2, int y2) {
		SDL.SDL_RenderDrawLine(_renderer, x1, y1, x2, y2);
	}

	public void DrawRectangle(int x, int y, int w, int h) {
		var rect = new SDL.SDL_Rect();
		rect.x = x;
		rect.y = y;
		rect.w = w;
		rect.h = h;
		SDL.SDL_RenderDrawRect(_renderer, ref rect);
	}

	public void Dispose() {
		SDL.SDL_DestroyRenderer(_renderer);
	}
}
}

