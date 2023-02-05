using System;

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