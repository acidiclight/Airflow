using System;
using X11;

namespace Airflow.X11;

class X11KeyGrabber
{
	private static IntPtr _XDisplay;
	private static KeyCode _Keycode1;
	private static KeyCode _Keycode2;
	private static KeyCode _Keycode3;
	private static KeyCode _Keycode4;
	private static KeyCode _Keycode5;
	private static KeyCode _KeycodeLeft;
	private static KeyCode _KeycodeMiddle;

	private static KeyCode _KeycodeControlL;
	private static KeyCode _KeycodeControlR;
	private static KeyCode _KeycodeEnable;

	private static Window _XRootWindow;
	private byte[] _queryReturnArr = new byte[32];
	private uint _pointerMask;

	public static bool IsKeyDown(ReadOnlySpan<byte> arr, KeyCode code) =>
		(arr[(byte)code >> 3] & (1 << ((byte)code & 7))) == 1 << ((byte)code & 7);

	public void Initialize()
	{
		_XDisplay = Xlib.XOpenDisplay(":0");
		_XRootWindow = Xlib.XDefaultRootWindow(_XDisplay);

		_Keycode5 = Xlib.XKeysymToKeycode(_XDisplay,
			(KeySym)KeySyms.i); //right handed: a //left handed: ;
		_Keycode4 = Xlib.XKeysymToKeycode(_XDisplay,
			(KeySym)KeySyms.o); //right handed: s //left handed: l
		_Keycode3 = Xlib.XKeysymToKeycode(_XDisplay,
			(KeySym)KeySyms.e); //right handed: d //left handed: k
		_Keycode2 = Xlib.XKeysymToKeycode(_XDisplay,
			(KeySym)KeySyms.n); //right handed: f //left handed: j
		_Keycode1 = Xlib.XKeysymToKeycode(_XDisplay, (KeySym)KeySyms.space);

		_KeycodeEnable = Xlib.XKeysymToKeycode(_XDisplay, (KeySym)KeySyms.space);

		_KeycodeControlL = Xlib.XKeysymToKeycode(_XDisplay, (KeySym)KeySyms.Control_L);
		_KeycodeControlR = Xlib.XKeysymToKeycode(_XDisplay, (KeySym)KeySyms.Control_R);
	}

	private const KeyButtonMask MASK = KeyButtonMask.Mod2Mask;
	private const KeyButtonMask ENABLE_MASK = MASK | KeyButtonMask.ShiftMask;

	public unsafe void GrabKeys()
	{
		Xlib.XGrabKey(_XDisplay, _Keycode1, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(_XDisplay, _Keycode2, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(_XDisplay, _Keycode3, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(_XDisplay, _Keycode4, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabKey(_XDisplay, _Keycode5, MASK, _XRootWindow, false, GrabMode.Async, GrabMode.Async);

		// Xlib.XGrabKey(Window.X11DisplayPtr, _KeycodeLeft, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		// Xlib.XGrabKey(Window.X11DisplayPtr, _KeycodeMiddle, MASK, _XWindow, false, GrabMode.Async, GrabMode.Async);
		Xlib.XGrabButton(_XDisplay, Button.LEFT, MASK, _XRootWindow, false,
			EventMask.ButtonPressMask | EventMask.ButtonReleaseMask, GrabMode.Async, GrabMode.Async, Window.None,
			FontCursor.None);
		Xlib.XGrabButton(_XDisplay, Button.RIGHT, MASK, _XRootWindow, false,
			EventMask.ButtonPressMask | EventMask.ButtonReleaseMask, GrabMode.Async, GrabMode.Async, Window.None,
			FontCursor.None);

		Xlib.XGrabKey(_XDisplay, _KeycodeEnable, ENABLE_MASK, _XRootWindow, false, GrabMode.Async,
			GrabMode.Async);

		bool test;
		XLibB.XkbSetDetectableAutoRepeat(_XDisplay, true, &test);

		Xlib.XFlush(_XDisplay);
	}

	public void ReleaseKeys()
	{
		Xlib.XUngrabKey(_XDisplay, _Keycode1, MASK, _XRootWindow);
		Xlib.XUngrabKey(_XDisplay, _Keycode2, MASK, _XRootWindow);
		Xlib.XUngrabKey(_XDisplay, _Keycode3, MASK, _XRootWindow);
		Xlib.XUngrabKey(_XDisplay, _Keycode4, MASK, _XRootWindow);
		Xlib.XUngrabKey(_XDisplay, _Keycode5, MASK, _XRootWindow);

		Xlib.XUngrabButton(_XDisplay, Button.LEFT, KeyButtonMask.Mod2Mask, _XRootWindow);
		Xlib.XUngrabButton(_XDisplay, Button.RIGHT, KeyButtonMask.Mod2Mask, _XRootWindow);

		Xlib.XUngrabKey(_XDisplay, _KeycodeEnable, ENABLE_MASK, _XRootWindow);

		Xlib.XFlush(_XDisplay);
	}

	public void Poll()
	{
		Window pointerWindow = new();
		Window pointerChild = new();

		int pointerRootX = 0;
		int pointerRootY = 0;
		int pointerWinX = 0;
		int pointerWinY = 0;

		this._pointerMask = 0;

		XLibB.QueryKeymap(_XDisplay, this._queryReturnArr);
		Xlib.XQueryPointer(_XDisplay, _XRootWindow, ref pointerWindow, ref pointerChild, ref pointerRootX,
			ref pointerRootY, ref pointerWinX, ref pointerWinY, ref this._pointerMask);
	}

	public bool Key1State()
	{
		return IsKeyDown(this._queryReturnArr, _Keycode1);
	}

	public bool Key2State()
	{
		return IsKeyDown(this._queryReturnArr, _Keycode2);
	}

	public bool Key3State()
	{
		return IsKeyDown(this._queryReturnArr, _Keycode3);
	}

	public bool Key4State()
	{
		return IsKeyDown(this._queryReturnArr, _Keycode4);
	}

	public bool Key5State()
	{
		return IsKeyDown(this._queryReturnArr, _Keycode5);
	}

	public bool ControlState()
	{
		return IsKeyDown(this._queryReturnArr, _KeycodeControlL) | IsKeyDown(this._queryReturnArr, _KeycodeControlR);
	}

	public bool LeftState()
	{
		return (this._pointerMask & (1 << 8)) != 0;
	}

	public bool RightState()
	{
		return (this._pointerMask & (1 << 10)) != 0;
	}

	public void Dispose()
	{

	}
}