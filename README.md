# Airflow

Sonic Riders air mechanic but it's a C#/Win32 program that controls your music volume based on how well you type.

This program is a demo of some low-level Win32 APIs as well as CoreAudio. It demonstrates how to monitor system-wide keypresses, and how to use CoreAudio to enumerate audio sessions and control them.

Not all of the code is mine, some of it is scavenged from StackOverflow. Really I just wrote this as a tool for my personal use, since having it control my music should help me stay focused.

## How it works

When you run the program, you have to select what audio session it controls. Usually this'll be a program on your computer, but you can also have it control microphones in Listen Mode or even your master audio.

When the program is running, it listens to all keypresses on your system. If you press any key but Backspace or Delete, you gain air. Pressing Backspace or Delete will decrease your air supply, and idling on the keyboard causes it to gradually decrease as well.

You start out at 0% air. When you run out of air, the audio session you select will be smoothly turned down to 25% volume. When max air is achieved, it is turned back up to 100%. When you are below 35% air but above 0, Airflow will play the same warning sound from the actual Sonic Riders game.

## Credit where credit is due

1. I'd like to thank that random guy on StackOverflow whose LowLevelKeyboardHook code I shamelessly stole.
2. Thanks to the Sonic Riders DX/Ungravitified Discord server for helping me source the game audio used in Airflow.
3. Thank you Win32, for being an annoying API to work with...

## Notes

 - Don't close the program Airflow is controlling the volume of. This causes the CoreAudio session to die and the app to crash.

 - Don't select "Airflow" in the menu that lets you pick an app. Airflow, being a Windows program, needs to use the same API to play audio that it's using to control it... selecting Airflow here will just control the volume of the Sonic Riders sounds.

 - This is a Windows-only app. It depends on WinForms, so don't try to use it on Linux or Mac. Even if it could technically compile, you still don't have CoreAudio or Win32 on those platforms.

 - I'm still not sure if unplugging the device your audio is playing on will crash Airflow... try at your own risk.

 - Airflow uses the same Windows API as the Windows Volume Mixer does to control your app's audio level, so don't try to cheat by manually adjusting the audio when you've run out of air. Airflow will revert what you try to do. This also means you'll need to fix the audio settings after exiting Airflow but there's no easy way for me to fix that.

## FAQ

**Q. Why?**

A. Because I like Sonic Riders and I like having my music controlled by how I type.

**Q: Can I disable the warning sound?**

A: Yes. Turn Airflow down in Windows Volume Mixer. You just won't be able to tell when you're about to run out of air, and I think that ruins the charm.

**Q: Will anyone tell me I'm gaining momentum?**

A: No, but I thought of implementing that. Maybe in the future I'll rip all of the voice clips for when characters gain momentum, and make you choose your favourite in-game character to determine what sound to play... I don't know. I just know I'd personally play as Jet.

**Q: Can I change the audio levels/intervals/other settings?**

A: Yes, by editing the code.

**Q: Will Airflow remember what program I chose when I exit it?**

A: No, that's impossible. Welcome to Win32.

**Q: Can I replace the sounds?**

A: As long as they're in WAV format, and have the same file names, yes.

 - `Navigate.wav`: Plays when using the arrow keys to select options in the menu
 - `Select.wav`: Plays when you press Enter on an option
 - `Back.wav`: Plays when going to the previous menu
 - `Warning.wav`: Plays periodically when running out of Air.

**Q: Will you maintain this program?**

A: Yes... on my free time. My gamedev projects take priority. :)