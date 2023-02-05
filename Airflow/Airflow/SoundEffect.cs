using System;
using System.IO;

namespace Airflow;

public abstract class SoundEffect : IDisposable
{
    private readonly string path;
    private readonly string id;

    public string Path => path;
    public string Id => id;
    
    protected SoundEffect(string path)
    {
        this.path = path;
        this.id = Guid.NewGuid().ToString();
    }

    public abstract void Dispose();

    public abstract void Play(float volume = 1);

    public static SoundEffect FromFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(path);

        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            return new WindowsSoundEffect(path);
        else if (Environment.OSVersion.Platform == PlatformID.Unix)
            return new PulseAudioSoundEffect(path);

        throw new NotSupportedException();
    }
}