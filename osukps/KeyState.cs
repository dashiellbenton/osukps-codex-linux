using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace osukps {
	static class KeyState {
		private static readonly IKeyStateBackend backend = CreateBackend();

		public static bool IsPressed(int keyCode) {
			return backend.IsPressed(keyCode);
		}

		private static IKeyStateBackend CreateBackend() {
			PlatformID platform = Environment.OSVersion.Platform;
			if (platform == PlatformID.Win32NT || platform == PlatformID.Win32S || platform == PlatformID.Win32Windows || platform == PlatformID.WinCE) {
				return new WindowsKeyStateBackend();
			}
			try {
				return new X11KeyStateBackend();
			} catch (DllNotFoundException ex) {
				Console.Error.WriteLine("osukps: libX11 is not available. Global key polling is disabled. " + ex.Message);
			} catch (EntryPointNotFoundException ex) {
				Console.Error.WriteLine("osukps: required X11 entry point is not available. Global key polling is disabled. " + ex.Message);
			}
			return new DisabledKeyStateBackend();
		}

		private interface IKeyStateBackend {
			bool IsPressed(int keyCode);
		}

		private class DisabledKeyStateBackend : IKeyStateBackend {
			public bool IsPressed(int keyCode) {
				return false;
			}
		}

		private class WindowsKeyStateBackend : IKeyStateBackend {
			[DllImport("user32.dll")]
			private static extern short GetAsyncKeyState(int vkey);

			public bool IsPressed(int keyCode) {
				return (GetAsyncKeyState(keyCode) & 0x8000) == 0x8000;
			}
		}

		private class X11KeyStateBackend : IKeyStateBackend {
			[DllImport("libX11.so.6")]
			private static extern IntPtr XOpenDisplay(IntPtr displayName);

			[DllImport("libX11.so.6")]
			private static extern int XCloseDisplay(IntPtr display);

			[DllImport("libX11.so.6")]
			private static extern int XQueryKeymap(IntPtr display, byte[] keys);

			[DllImport("libX11.so.6")]
			private static extern byte XKeysymToKeycode(IntPtr display, IntPtr keysym);

			private readonly IntPtr display;
			private readonly Dictionary<int, byte> keycodeCache = new Dictionary<int, byte>();

			public X11KeyStateBackend() {
				display = XOpenDisplay(IntPtr.Zero);
				if (display == IntPtr.Zero) {
					Console.Error.WriteLine("osukps: unable to open the X11 display. Global key polling is disabled.");
				}
			}

			~X11KeyStateBackend() {
				if (display != IntPtr.Zero) {
					XCloseDisplay(display);
				}
			}

			public bool IsPressed(int keyCode) {
				if (display == IntPtr.Zero) {
					return false;
				}

				byte xkeycode = GetXKeycode(keyCode);
				if (xkeycode == 0) {
					return false;
				}

				byte[] keys = new byte[32];
				if (XQueryKeymap(display, keys) == 0) {
					return false;
				}

				return (keys[xkeycode >> 3] & (1 << (xkeycode & 7))) != 0;
			}

			private byte GetXKeycode(int keyCode) {
				byte xkeycode;
				if (keycodeCache.TryGetValue(keyCode, out xkeycode)) {
					return xkeycode;
				}

				IntPtr keysym = KeysymFromWinFormsKey((Keys) keyCode);
				xkeycode = keysym == IntPtr.Zero ? (byte) 0 : XKeysymToKeycode(display, keysym);
				keycodeCache[keyCode] = xkeycode;
				return xkeycode;
			}

			private static IntPtr KeysymFromWinFormsKey(Keys key) {
				key &= Keys.KeyCode;

				if (key >= Keys.A && key <= Keys.Z) {
					return new IntPtr(0x0061 + (key - Keys.A));
				}
				if (key >= Keys.D0 && key <= Keys.D9) {
					return new IntPtr(0x0030 + (key - Keys.D0));
				}
				if (key >= Keys.NumPad0 && key <= Keys.NumPad9) {
					return new IntPtr(0xFFB0 + (key - Keys.NumPad0));
				}
				if (key >= Keys.F1 && key <= Keys.F12) {
					return new IntPtr(0xFFBE + (key - Keys.F1));
				}

				switch (key) {
				case Keys.Back: return new IntPtr(0xFF08);
				case Keys.Tab: return new IntPtr(0xFF09);
				case Keys.Enter: return new IntPtr(0xFF0D);
				case Keys.ShiftKey:
				case Keys.LShiftKey: return new IntPtr(0xFFE1);
				case Keys.RShiftKey: return new IntPtr(0xFFE2);
				case Keys.ControlKey:
				case Keys.LControlKey: return new IntPtr(0xFFE3);
				case Keys.RControlKey: return new IntPtr(0xFFE4);
				case Keys.Menu:
				case Keys.LMenu: return new IntPtr(0xFFE9);
				case Keys.RMenu: return new IntPtr(0xFFEA);
				case Keys.Pause: return new IntPtr(0xFF13);
				case Keys.Capital: return new IntPtr(0xFFE5);
				case Keys.Escape: return new IntPtr(0xFF1B);
				case Keys.Space: return new IntPtr(0x0020);
				case Keys.PageUp: return new IntPtr(0xFF55);
				case Keys.PageDown: return new IntPtr(0xFF56);
				case Keys.End: return new IntPtr(0xFF57);
				case Keys.Home: return new IntPtr(0xFF50);
				case Keys.Left: return new IntPtr(0xFF51);
				case Keys.Up: return new IntPtr(0xFF52);
				case Keys.Right: return new IntPtr(0xFF53);
				case Keys.Down: return new IntPtr(0xFF54);
				case Keys.PrintScreen: return new IntPtr(0xFF61);
				case Keys.Insert: return new IntPtr(0xFF63);
				case Keys.Delete: return new IntPtr(0xFFFF);
				case Keys.LWin: return new IntPtr(0xFFEB);
				case Keys.RWin: return new IntPtr(0xFFEC);
				case Keys.Multiply: return new IntPtr(0xFFAA);
				case Keys.Add: return new IntPtr(0xFFAB);
				case Keys.Subtract: return new IntPtr(0xFFAD);
				case Keys.Decimal: return new IntPtr(0xFFAE);
				case Keys.Divide: return new IntPtr(0xFFAF);
				case Keys.NumLock: return new IntPtr(0xFF7F);
				case Keys.Scroll: return new IntPtr(0xFF14);
				case Keys.OemSemicolon: return new IntPtr(0x003B);
				case Keys.Oemplus: return new IntPtr(0x003D);
				case Keys.Oemcomma: return new IntPtr(0x002C);
				case Keys.OemMinus: return new IntPtr(0x002D);
				case Keys.OemPeriod: return new IntPtr(0x002E);
				case Keys.OemQuestion: return new IntPtr(0x002F);
				case Keys.Oemtilde: return new IntPtr(0x0060);
				case Keys.OemOpenBrackets: return new IntPtr(0x005B);
				case Keys.OemPipe: return new IntPtr(0x005C);
				case Keys.OemCloseBrackets: return new IntPtr(0x005D);
				case Keys.OemQuotes: return new IntPtr(0x0027);
				}

				return IntPtr.Zero;
			}
		}
	}
}
