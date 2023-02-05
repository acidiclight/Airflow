using System.Collections.Generic;
using System.ComponentModel;
using NAudio.CoreAudioApi;

namespace Airflow;

public class AudioSessionSelection : IConsoleMenu
{
    private List<IAudioSession> sessions = new List<IAudioSession>();

    public string Title => "Audio Session Selection";
    public string Description => @"Please select the application you'd like Airflow to control.";
    public bool CanGoBack => true;

    public IEnumerable<ConsoleMenuChoice> Choices
    {
        get
        {
            yield return new ConsoleMenuChoice(
                "Refresh",
                program =>
                {
                    RefreshSources();
                    program.RefreshChoices();
                });

            foreach (var session in this.sessions)
            {
                yield return new ConsoleMenuChoice(
                    session.DisplayName,
                    program => program.SetTargetSession(session)
                );
            }
        }
    }

    public AudioSessionSelection()
    {
        this.RefreshSources();
    }

    private void RefreshSources()
    {

        this.sessions.Clear();

#if WINDOWS
        this.GetWin32Sessions();
#else
        this.GetPulseAudioSessions();
#endif
    }

    private void GetPulseAudioSessions()
    {
        var sinkInputs = PulseAudioClient.ListSinkInputs();

        foreach (PulseAudioSinkInput sinkInput in sinkInputs)
        {
            int index = sinkInput.SinkIndex;

            if (!sinkInput.TryGetPropertyByName("application.name", out string? appName))
                appName = "Unknown application";

            float volume = sinkInput.LoudestVolume;
            this.sessions.Add(new PulseAudioSession(index, appName, volume));
        }
    }

    private void GetWin32Sessions()
    {
        var deviceEnumerator = new MMDeviceEnumerator();
        var defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        var sessions = defaultDevice.AudioSessionManager.Sessions;
        for (var i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            this.sessions.Add(new Win32AudioSession(session));
        }
    }
}