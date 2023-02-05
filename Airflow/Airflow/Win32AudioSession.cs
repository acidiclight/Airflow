using NAudio.CoreAudioApi;

namespace Airflow;

public class Win32AudioSession : IAudioSession
{
    private AudioSessionControl control;

    public Win32AudioSession(AudioSessionControl control)
    {
        this.control = control;
    }

    public float Volume
    {
        get => control.SimpleAudioVolume.Volume;
        set => control.SimpleAudioVolume.Volume = value;
    }

    public string DisplayName => GetProcessName(control.GetProcessID);

    private string GetProcessName(uint pid)
    {
        var process = System.Diagnostics.Process.GetProcessById((int) pid);
        if (!string.IsNullOrWhiteSpace(process.MainWindowTitle))
            return process.MainWindowTitle;

        return process.ProcessName;
    }
}