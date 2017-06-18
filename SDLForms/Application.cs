using SDL2;
using System.Collections.Generic;
using System;

namespace SDLForms {
public class Application {
	bool _isRunning;
	Dictionary<uint, Window> _windows;
	internal List<Action> _queuedActions;

	Application() {
		_isRunning = true;
		SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
		_windows = new Dictionary<uint, Window>();
		_queuedActions = new List<Action>();
	}

	internal void registerWindow(Window wnd) {
		_windows.Add(wnd.ID, wnd);
	}

	internal void unregisterWindow(Window wnd) {
		_windows.Remove(wnd.ID);
	}

	void _PropegateEvent(uint wid, ref SDL.SDL_Event evt) {
		Window wnd;
		try {
			wnd = _windows[wid];
		} catch (KeyNotFoundException) {
			return;
		}

		wnd._HandleEvent(
			ref evt
		);
	}

	void _HandleEvent(ref SDL.SDL_Event evt) {
		switch (evt.type) {
		case SDL.SDL_EventType.SDL_QUIT:
			_isRunning = false;
			break;
		case SDL.SDL_EventType.SDL_WINDOWEVENT:
			_PropegateEvent(evt.window.windowID, ref evt);
			break;
		case SDL.SDL_EventType.SDL_KEYDOWN:
			_PropegateEvent(evt.key.windowID, ref evt);
			break;
		case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
			if (evt.button.which != SDL.SDL_TOUCH_MOUSEID) {
				_PropegateEvent(evt.button.windowID, ref evt);
			}
			break;
		case SDL.SDL_EventType.SDL_MOUSEWHEEL:
			if (evt.wheel.which != SDL.SDL_TOUCH_MOUSEID) {
				_PropegateEvent(evt.wheel.windowID, ref evt);
			}
			break;
		case SDL.SDL_EventType.SDL_MOUSEMOTION:
			if (evt.motion.which != SDL.SDL_TOUCH_MOUSEID) {
				_PropegateEvent(evt.motion.windowID, ref evt);
			}
			break;
		}
	}

	public void MainLoop() {
		SDL.SDL_Event evt;
		while (_isRunning) {
			SDL.SDL_WaitEvent(out evt);
			_HandleEvent(ref evt);
			while (SDL.SDL_PollEvent(out evt) != 0) {
				_HandleEvent(ref evt);
			}

			foreach (var action in _queuedActions) {
				action();
			}

			_queuedActions.Clear();

			foreach (var window in _windows.Values) {
				window._paint();
			}
		}
		SDL.SDL_Quit();
	}

	static Application _instance;

	public static Application GetInstance() {
		if (_instance == null) {
			_instance = new Application();
		}

		return _instance;
	}
}
}
