using System;
using System.Threading;
using Airflow.X11;
using X11;

#if WINDOWS
using System.Windows.Forms;
#endif

namespace Airflow;

public class KeyListener : IDisposable
{
    public event Action DestructiveKeyPressed;
    public event Action KeyPressed;

    public KeyListener()
    {
#if WINDOWS
        SetupWindows();
#endif
    }

    public unsafe void STartX11Loop()
    {
        int pollingRate = 1000 / 125;

        IntPtr display = Xlib.XOpenDisplay(":0");
        
        Span<byte> current = stackalloc byte[32];

        KeyCode backspaceKeyCode = Xlib.XKeysymToKeycode(display, (KeySym)KeySyms.BackSpace);
        KeyCode metaBackspaceKeyCode = Xlib.XKeysymToKeycode(display, (KeySym)KeySyms.Meta_BackSpace);
        KeyCode deleteKeyCode = Xlib.XKeysymToKeycode(display, (KeySym)KeySyms.Delete);
        KeyCode metaDeleteKeyCode = Xlib.XKeysymToKeycode(display, (KeySym)KeySyms.Meta_Delete);
        
        
        while (true)
        {
            XLibB.QueryKeymap(display, current);
            
            // Loop through each byte in the keymap, to determine if any key is pressed
            var isAnyKeyPressed = false;
            for (var i = 0; i < 32; i++)
            {
                byte bCurrent = current[i];
                // We know a key is pressed if any byte is > 0
                if (bCurrent > 0)
                {
                    isAnyKeyPressed = true;
                    break;
                }
            }
            
            // If any key is pressed at all, check for backspace or delete
            if (isAnyKeyPressed)
            {
                if (X11KeyGrabber.IsKeyDown(current, backspaceKeyCode)
                    || X11KeyGrabber.IsKeyDown(current, metaBackspaceKeyCode)
                    || X11KeyGrabber.IsKeyDown(current, deleteKeyCode)
                    || X11KeyGrabber.IsKeyDown(current, metaDeleteKeyCode))
                    DestructiveKeyPressed?.Invoke();
                else
                    KeyPressed?.Invoke();
            }
            
            Thread.Sleep(pollingRate);
        }
    }

    public void Dispose()
    {
#if WINDOWS
        DisposeWindows();
#endif
    }

    public void StartListening()
    {
#if WINDOWS
        win32hook.HookKeyboard();
#endif
    }

    public void StopListening()
    {
#if WINDOWS
        win32hook.UnHookKeyboard();
#endif
    }
    
#if WINDOWS
    private LowLevelKeyboardHook win32hook;
    
    private void SetupWindows()
    {
        win32hook = new LowLevelKeyboardHook();

        win32hook.OnKeyPressed += Win32KeyPressed;
    }

    private void DisposeWindows()
    {
        win32hook.OnKeyPressed -= Win32KeyPressed;
        win32hook = null;
    }

    private void Win32KeyPressed(object sender, Keys e)
    {
        if (e == Keys.Delete || e == Keys.Back)
        {
            DestructiveKeyPressed?.Invoke();
        }
        else
        {
            KeyPressed?.Invoke();
        }
    }
#endif
}