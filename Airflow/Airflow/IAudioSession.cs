using System;

namespace Airflow;

public interface IAudioSession
{
    public float Volume { get; set; }
    public string DisplayName { get; }
}

public class PulseAudioSession : IAudioSession
{
    private readonly int sinkIndex;
    private readonly string displayName;
    private float volume;
    
    public PulseAudioSession(int sinkIndex, string displayName, float volume)
    {
        this.sinkIndex = sinkIndex;
        this.displayName = displayName;
        this.volume = volume;
    }

    public float Volume
    {
        get => volume;
        set => UpdateVolume(value);
    }

    public string DisplayName => displayName;

    private void UpdateVolume(float newVolume)
    {
        if (newVolume < 0)
            newVolume = 0;
        if (newVolume > 1)
            newVolume = 1;

        if (Math.Abs(this.volume - newVolume) < (1f / 65536f))
            return;

        volume = newVolume;

        PulseAudioClient.SetSinkInputVolume(sinkIndex, volume);
    }
}