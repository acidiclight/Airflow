using System;

namespace Airflow.X11;

public struct XKeyCode
{
    public int index;
    public byte bit;

    public XKeyCode(int index, byte bit)
    {
        this.index = index;
        this.bit = bit;
    }

    public bool IsKeyDown(ReadOnlySpan<byte> keymap)
    {
        return (keymap[index] & bit) != 0;
    }
}