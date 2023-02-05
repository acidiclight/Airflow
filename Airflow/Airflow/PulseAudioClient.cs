using System;
using System.Diagnostics;
using System.Text.Json;

namespace Airflow;

public static class PulseAudioClient
{
    private static string pactlPath;
    
    static PulseAudioClient()
    {
        LocatePactl();
    }

    public static void SetSinkInputVolume(int index, float volume)
    {
        var startInfo = new ProcessStartInfo();
        startInfo.FileName = pactlPath;
        startInfo.ArgumentList.Add("set-sink-input-volume");
        startInfo.ArgumentList.Add(index.ToString());
        startInfo.ArgumentList.Add($"{Math.Floor(volume * 100)}%");

        Process.Start(startInfo);
    }
    
    public static PulseAudioSinkInput[] ListSinkInputs()
    {
        var startInfo = new ProcessStartInfo();
        startInfo.FileName = pactlPath;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.ArgumentList.Add("-f");
        startInfo.ArgumentList.Add("json");
        startInfo.ArgumentList.Add("list");
        startInfo.ArgumentList.Add("sink-inputs");

        var proc = Process.Start(startInfo);
        string json = proc.StandardOutput.ReadToEnd().Trim();

        return JsonSerializer.Deserialize<PulseAudioSinkInput[]>(json, new JsonSerializerOptions
        {
            IncludeFields = true
        });
    }
    
    public static void LocatePactl()
    {
        var startInfo = new ProcessStartInfo();
        startInfo.FileName = "/usr/bin/whereis";
        startInfo.UseShellExecute = false;
        startInfo.Arguments = "pactl";
        startInfo.RedirectStandardOutput = true;

        var proc = Process.Start(startInfo);
        if (proc == null)
        {
            ExitWithNoSupport();
            return;
        }

        string result = proc.StandardOutput.ReadToEnd();

        int pathsIndex = result.IndexOf(": ", StringComparison.Ordinal);
        if (pathsIndex == -1)
        {
            ExitWithNoSupport();
            return;
        }
        
        string pathsRaw = result.Substring(pathsIndex + 2);

        string[] paths = pathsRaw.Split(" ");

        if (paths.Length < 1)
        {
            ExitWithNoSupport();
            return;
        }

        pactlPath = paths[0];
    }

    private static void ExitWithNoSupport()
    {
        Console.WriteLine(@"Airflow cannot run on this system!

Your computer lacks the PulseAudio control program (/usr/bin/pactl). Airflow uses pactl
on Linux systems to control the volume of applications and to play sound effects.

You should set up PulseAudio or pipewire-pulse on your system, and ensure that you have
pactl installed. Consult your distro's documentation.");
        Environment.Exit(0);
    }
}