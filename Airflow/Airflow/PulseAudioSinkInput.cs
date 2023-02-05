using System.Collections.Generic;
using System.Linq;

namespace Airflow;

public class PulseAudioSinkInput
{
    public int index;
    public string channel_map;
    public Dictionary<string, PulseAudioChannelVolume> volume = new Dictionary<string, PulseAudioChannelVolume>();
    public Dictionary<string, string> properties = new Dictionary<string, string>();

    public float LoudestVolume => volume.Values.OrderByDescending(x => x.Volume).First().Volume;
    public int SinkIndex => index;

    public bool TryGetPropertyByName(string name, out string? property)
    {
        return this.properties.TryGetValue(name, out property);
    }
}

public class PulseAudioChannelVolume
{
    public int volume;
    public string volume_percent;
    public string db;

    public float Volume => volume / 65536f;
}