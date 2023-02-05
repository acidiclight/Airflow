using System;
using System.Runtime.InteropServices;
using X11;

namespace Airflow.X11;

public class XLibB
{
	[DllImport("libX11.so.6", CharSet = CharSet.Ansi)]
	private static extern unsafe void XQueryKeymap(IntPtr display, byte* returnArr);

	public static unsafe void QueryKeymap(IntPtr display, ReadOnlySpan<byte> returnArr)
	{
		if (returnArr.Length != 32)
			throw new Exception("returnArr is not the right length!");

		fixed (byte* ptr = returnArr)
			XQueryKeymap(display, ptr);
	}

	[DllImport("libX11.so.6", CharSet = CharSet.Ansi)]
	public static extern unsafe bool XkbSetDetectableAutoRepeat(IntPtr display, bool detectable, bool* supportedRtrn);

	[DllImport("libX11.so.6", CharSet = CharSet.Ansi)]
	public static extern unsafe int XGrabKeyboard(IntPtr display, Window window, bool ownerEvents, GrabMode pointerMode,
		GrabMode keyboardMode, ulong time);

	[DllImport("libX11.so.6", CharSet = CharSet.Ansi)]
	public static extern unsafe void XUngrabKeyboard(IntPtr display, ulong time);

	[DllImport("libXtst.so.6", CharSet = CharSet.Ansi)]
	public static extern unsafe void XTestGrabControl(IntPtr display, bool impervious);

	[DllImport("libXtst.so.6", CharSet = CharSet.Ansi)]
	public static extern unsafe int XTestFakeKeyEvent(IntPtr display, KeyCode code, bool value, ulong delay);
}