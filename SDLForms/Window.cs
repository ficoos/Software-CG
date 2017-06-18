using System;
using SDL2;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Reflection.Emit;

namespace SDLForms {
public class KeyDownEventArgs : EventArgs {
	SDL.SDL_KeyboardEvent _evt;

	public KeyDownEventArgs(SDL.SDL_KeyboardEvent evt) {
		_evt = evt;
	}

	public bool IsRepeat {
		get {
			return _evt.repeat > 0;
		}
	}

	public SDL.SDL_Keysym KeySym {
		get {
			return _evt.keysym;
		}
	}
}

public class PaintEventArgs : EventArgs {
}

public class WindowResizedEventArgs : EventArgs {
	SDL.SDL_WindowEvent _evt;

	public WindowResizedEventArgs(SDL.SDL_WindowEvent evt) {
		_evt = evt;
	}

	public int Width {
		get {
			return _evt.data1;
		}
	}

	public int Height {
		get {
			return _evt.data2;
		}
	}
}

public class WindowClosingEventArgs : EventArgs {

	public bool Cancel {
		get;
		set;
	}

	public WindowClosingEventArgs(SDL.SDL_WindowEvent evt) {
		Cancel = false;
	}
}

public enum MouseButton {
	Left,
	Right,
	Middle,
	None,
}

public class MouseWheelEventArgs : EventArgs {
	SDL.SDL_MouseWheelEvent _evt;

	public MouseWheelEventArgs(SDL.SDL_MouseWheelEvent evt) {
		_evt = evt;
	}

	public int X {
		get {
			return _evt.x;
		}
	}

	public int Y {
		get {
			return _evt.y;
		}
	}

}

public class MouseMotionEventArgs : EventArgs {
	SDL.SDL_MouseMotionEvent _evt;

	public MouseMotionEventArgs(SDL.SDL_MouseMotionEvent evt) {
		_evt = evt;
	}

	public MouseButton Button {
		get {
			if ((_evt.state & SDL.SDL_BUTTON_LMASK) != 0) {
				return MouseButton.Left;
			} else if ((_evt.state & SDL.SDL_BUTTON_RMASK) != 0) {
				return MouseButton.Right;
			} else if ((_evt.state & SDL.SDL_BUTTON_MMASK) != 0) {
				return MouseButton.Middle;
			} else {
				return MouseButton.None;
			}
		}
	}

	public bool State {
		get {
			return _evt.state == SDL.SDL_PRESSED;
		}
	}

	public int X {
		get {
			return _evt.x;
		}
	}

	public int Y {
		get {
			return _evt.y;
		}
	}

	public int RelativeX {
		get {
			return _evt.xrel;
		}
	}

	public int RelativeY {
		get {
			return _evt.yrel;
		}
	}
}

public class MouseButtonDownEventArgs : EventArgs {
	SDL.SDL_MouseButtonEvent _evt;

	public MouseButtonDownEventArgs(SDL.SDL_MouseButtonEvent evt) {
		_evt = evt;
	}

	public MouseButton Button {
		get {
			switch ((uint)(_evt.button)) {
			case SDL.SDL_BUTTON_LEFT:
				return MouseButton.Left;
			case SDL.SDL_BUTTON_RIGHT:
				return MouseButton.Right;
			default:
				return MouseButton.Middle;
			}
		}
	}

	public bool State {
		get {
			return _evt.state == SDL.SDL_PRESSED;
		}
	}

	public int X {
		get {
			return _evt.x;
		}
	}

	public int Y {
		get {
			return _evt.y;
		}
	}
}

public class Window {
	readonly IntPtr _wnd;

	public event EventHandler<WindowResizedEventArgs> WindowResized;
	public event EventHandler<WindowClosingEventArgs> WindowClosing;
	public event EventHandler<MouseButtonDownEventArgs> MouseButtonDown;
	public event EventHandler<MouseMotionEventArgs> MouseMotion;
	public event EventHandler<MouseWheelEventArgs> MouseWheel;
	public event EventHandler<KeyDownEventArgs> KeyDown;
	public event EventHandler<PaintEventArgs> Paint;

	public Window(
		string windowTitle = "Window",
		int x = SDL.SDL_WINDOWPOS_UNDEFINED,
		int y = SDL.SDL_WINDOWPOS_UNDEFINED,
		int w = 640,
		int h = 480
	) {
		_wnd = SDL.SDL_CreateWindow(
			windowTitle,
			x,
			y,
			w,
			h,
			SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
		);
		Application.GetInstance().registerWindow(this);
	}

	public string Title {
		get {
			return SDL.SDL_GetWindowTitle(_wnd);
		}
		set {
			SDL.SDL_SetWindowTitle(_wnd, value);
		}
	}

	public int Width {
		get {
			int w, h;
			SDL.SDL_GetWindowSize(_wnd, out w, out h);
			return w;
		}
	}

	public int Height {
		get {
			int w, h;
			SDL.SDL_GetWindowSize(_wnd, out w, out h);
			return h;
		}
	}

	public uint ID {
		get {
			return SDL.SDL_GetWindowID(_wnd);
		}
	}

	public SDL.SDL_Surface Surface {
		get {
				return (SDL.SDL_Surface) Marshal.PtrToStructure(
				SDL.SDL_GetWindowSurface(_wnd),
					typeof(SDL.SDL_Surface)
			);
		}
	}

	void OnWindowResized(WindowResizedEventArgs args) {
		var handler = WindowResized;
		if (handler != null) {
			handler(this, args);
		}
	}

	void OnMouseButtonDown(MouseButtonDownEventArgs args) {
		var handler = MouseButtonDown;
		if (handler != null) {
			handler(this, args);
		}
	}

	void OnWindowClosing(WindowClosingEventArgs args) {
		var handler = WindowClosing;
		if (handler != null) {
			handler(this, args);
		}

		if (!args.Cancel) {
			this.Destroy();
		}
	}

	void OnMouseMotion(MouseMotionEventArgs args) {
		var handler = MouseMotion;
		if (handler != null) {
			handler(this, args);
		}
	}

	void OnMouseWheel(MouseWheelEventArgs args) {
		var handler = MouseWheel;
		if (handler != null) {
			handler(this, args);
		}
	}

	void OnKeyDown(KeyDownEventArgs args) {
		var handler = KeyDown;
		if (handler != null) {
			handler(this, args);
		}
	}

	void OnPaint(PaintEventArgs args) {
		var handler = Paint;
		if (handler != null) {
			handler(this, args);
		}
	}

	void Destroy() {
		Application.GetInstance().unregisterWindow(this);
		SDL.SDL_DestroyWindow(_wnd);
	}

	public Renderer CreateRenderer() {
		return new Renderer(_wnd);
	}

	internal void _paint() {
		OnPaint(new PaintEventArgs());
	}

	internal void _HandleEvent(ref SDL.SDL_Event evt) {
		switch (evt.type) {
		case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
			OnMouseButtonDown(new MouseButtonDownEventArgs(evt.button));
			break;
		case SDL.SDL_EventType.SDL_KEYDOWN:
			OnKeyDown(new KeyDownEventArgs(evt.key));
			break;
		case SDL.SDL_EventType.SDL_MOUSEMOTION:
			OnMouseMotion(new MouseMotionEventArgs(evt.motion));
			break;
		case SDL.SDL_EventType.SDL_MOUSEWHEEL:
			OnMouseWheel(new MouseWheelEventArgs(evt.wheel));
			break;
		case SDL.SDL_EventType.SDL_WINDOWEVENT:
			switch (evt.window.windowEvent) {
			case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
				OnWindowResized(new WindowResizedEventArgs(evt.window));
				break;
			case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
				var args = new WindowClosingEventArgs(evt.window);
				OnWindowClosing(args);
				break;
			}
			break;
		}
	}

}
}

