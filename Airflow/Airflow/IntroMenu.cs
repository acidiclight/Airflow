using System;
using System.Collections.Generic;

namespace Airflow;

public class IntroMenu : IConsoleMenu
{
    public string Title => "Airflow";

    public string Description =>
        @"Airflow is a program that monitors your keypresses and controls your computer's audio volume based on how consistently you type.

This is a program based on the Air mechanic from Sonic Riders. The more consistently you type, the more flow you get. Flow steadily decreases over time, and decreases more when deleting text.

When you have maximum flow, your music will be turned up. It will stay turned up until you lose your flow. A warning sound will play when you're about to lose your flow.

To get started, you'll need to select an application whose audio level you would like Airflow to control.";

    public bool CanGoBack => false;

    public IEnumerable<ConsoleMenuChoice> Choices
    {
        get
        {
            yield return new ConsoleMenuChoice(
                "Set up Airflow!",
                "Press [ENTER] to start configuring Airflow.",
                program => program.PushMenu(new AudioSessionSelection())
            );

            yield return new ConsoleMenuChoice(
                "Close",
                "Choose this to exit Airflow.",
                _ => Environment.Exit(0)
            );
        }
    }
}