using System;
using System.Diagnostics;

namespace Airflow;

public class PulseAudioSoundEffect : SoundEffect
{
    private string pactlPath;
    
    public PulseAudioSoundEffect(string path) : base(path)
    {
        LocatePactl();
        UploadSample();
    }

    public override void Dispose()
    {
        var startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.FileName = pactlPath;
        startInfo.ArgumentList.Add("remove-sample");
        startInfo.ArgumentList.Add(Id);

        Process.Start(startInfo);
    }

    public override void Play(float volume = 1)
    {
        // TODO: Support for sample volumes. Likely requires using a different technique to play samples.
        
        var startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.FileName = pactlPath;
        startInfo.ArgumentList.Add("play-sample");
        startInfo.ArgumentList.Add(Id);

        Process.Start(startInfo);   
    }

    private void UploadSample()
    {
        var startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.FileName = pactlPath;
        startInfo.ArgumentList.Add("upload-sample");
        startInfo.ArgumentList.Add(Path);
        startInfo.ArgumentList.Add(Id);

        Process.Start(startInfo);
    }
    
    private void LocatePactl()
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

    private void ExitWithNoSupport()
    {
        Console.WriteLine(@"Airflow cannot run on this system!

Your computer lacks the PulseAudio control program (/usr/bin/pactl). Airflow uses pactl
on Linux systems to control the volume of applications and to play sound effects.

You should set up PulseAudio or pipewire-pulse on your system, and ensure that you have
pactl installed. Consult your distro's documentation.");
        Environment.Exit(0);
    }
}