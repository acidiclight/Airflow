using System;
using NAudio.Wave;

namespace Airflow;

public class WindowsSoundEffect : SoundEffect
{
    private WaveStream stream;
    private WaveOutEvent player;
    private WaveChannel32 channel;

    public WindowsSoundEffect(string path) : base(path)
    {
        stream = new WaveFileReader(path);
        channel = new WaveChannel32(stream);
        player = new WaveOutEvent();
        player.Init(channel);
    }

    public override void Dispose()
    {
        player?.Dispose();
        channel?.Dispose();
        stream?.Dispose();

        player = null;
        channel = null;
        stream = null;
    }

    public override void Play(float volume = 1)
    {
        if (volume < 0) volume = 0;
        else if (volume > 1) volume = 1;
            
        if (player == null)
            throw new ObjectDisposedException(nameof(SoundEffect));

        player.Stop();
        if (channel != null)
            channel.Position = 0;
        player.Volume = volume;
        player.Play();
    }
}