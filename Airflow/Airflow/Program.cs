// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

#if WINDOWS
using system.Windows.Forms;
#endif

namespace Airflow
{
    public class Program
    {
        public static string ApplicationDataDirectory => AppDomain.CurrentDomain.BaseDirectory;

        private bool isInFlow = false;
        private ConcurrentQueue<Action> pendingActions = new ConcurrentQueue<Action>();
        private IAudioSession targetSession;
        private float keyPressScore = 0.01f;
        private float lowAirVolume = 0.25f;
        private float lowAirVolumePipewire = 0.6f;
        private float highAirVolume = 1f;
        private bool running = false;
        private float currentAir;
        private const float airLowInterval = 0.900f;
        private const float airCriticalInterval = 0.400f;
        private SoundEffect flowLostSound;
        private SoundEffect flowGainedSound;
        private SoundEffect navigateSound;
        private SoundEffect selectSound;
        private SoundEffect backSound;
        private SoundEffect lowAirSound;
        private Stack<IConsoleMenu> previousMenus = new Stack<IConsoleMenu>();
        private IConsoleMenu currentMenu;
        private List<ConsoleMenuChoice> currentChoices = new List<ConsoleMenuChoice>();
        private int currentChoice;
        private const float airDecreaseInterval = 0.4f;
        private float timeSinceLastKeypress;
        private float timeSinceLastWarn;
        private KeyListener keyListener = new KeyListener();

        private void LoadResources()
        {
            string soundsPath = Path.Combine(ApplicationDataDirectory, "Sounds");

            this.flowGainedSound = SoundEffect.FromFile(Path.Combine(soundsPath, "power of babylon.wav"));
            this.navigateSound = SoundEffect.FromFile(Path.Combine(soundsPath, "Navigate.wav"));
            this.selectSound = SoundEffect.FromFile(Path.Combine(soundsPath, "Select.wav"));
            this.backSound = SoundEffect.FromFile(Path.Combine(soundsPath, "Back.wav"));
            this.lowAirSound = SoundEffect.FromFile(Path.Combine(soundsPath, "Warning.wav"));
            this.flowLostSound = SoundEffect.FromFile(Path.Combine(soundsPath, "next time.wav"));
            
            keyListener.DestructiveKeyPressed += OnDestructiveKeyPressed;
            keyListener.KeyPressed += OnKeyPressed;
        }

        private void OnKeyPressed()
        {
            pendingActions.Enqueue(() =>
            {
                if (!running)
                    return;

                IncreaseAir();
            });
        }

        private void OnDestructiveKeyPressed()
        {
            pendingActions.Enqueue(() =>
            {
                if (!running)
                    return;

                DecreaseAir();
            });
        }

        private void DecreaseAir()
        {
            currentAir -= keyPressScore;
            if (currentAir < 0)
                currentAir = 0;

            PrintAirGuage();
        }

        private void IncreaseAir()
        {
            currentAir += keyPressScore;
            if (currentAir > 1)
                currentAir = 1;

            timeSinceLastKeypress = 0;
            PrintAirGuage();
        }

        private void Update(float deltaSeconds)
        {
            float lowVolume = Environment.OSVersion.Platform == PlatformID.Unix
                ? lowAirVolumePipewire
                : lowAirVolume;
            
            if (pendingActions.TryDequeue(out Action action))
                action?.Invoke();
            
            if (currentAir <= 0)
            {
                if (isInFlow)
                {
                    isInFlow = false;
                    flowLostSound.Play(0.3f);
                }

                var volume = targetSession.Volume;
                if (Math.Abs(volume - lowVolume) > float.Epsilon)
                {
                    var diff = lowVolume - volume;
                    var sign = Math.Sign(diff);

                    targetSession.Volume += (deltaSeconds * 2) * sign;
                }
            }
            else if (currentAir <= 0.1 && isInFlow)
            {
                timeSinceLastWarn += deltaSeconds;
                if (timeSinceLastWarn >= airCriticalInterval)
                {
                    lowAirSound.Play();
                    timeSinceLastWarn = 0;
                }
            }
            else if (currentAir <= 0.34f && isInFlow)
            {
                timeSinceLastWarn += deltaSeconds;
                if (timeSinceLastWarn >= airLowInterval)
                {
                    lowAirSound.Play();
                    timeSinceLastWarn = 0;
                }
            }
            else if (currentAir >= 1)
            {
                if (!isInFlow)
                {
                    isInFlow = true;
                    flowGainedSound.Play(0.3f);
                }

                var volume = targetSession.Volume;
                if (Math.Abs(volume - highAirVolume) > float.Epsilon)
                {
                    var diff = highAirVolume - volume;
                    var sign = Math.Sign(diff);

                    targetSession.Volume += (deltaSeconds / 8) * sign;
                }                
            }


            timeSinceLastKeypress += deltaSeconds;
            if (timeSinceLastKeypress >= airDecreaseInterval)
            {
                if (currentAir > 0)
                {
                    currentAir -= keyPressScore;
                    PrintAirGuage();
                }
                else
                {
                    currentAir = 0;
                }

                timeSinceLastKeypress = 0;
            }
        }
        
        private void StartAirflow()
        {
            while (pendingActions.TryDequeue(out _))
                continue;
            
            this.running = true;
            this.currentAir = 0.5f;
            this.isInFlow = false;
            
            Console.Clear();
            PrintAirGuage();

            var time = DateTime.Now;
            while (running)
            {
                var now = DateTime.Now;
                var delta = now - time;
                time = now;

                var deltaSeconds = (float) delta.TotalSeconds;
                Update(deltaSeconds);
            }
        }

        private void PrintAirGuage()
        {
            Console.Clear();

            Console.WriteLine("Application: {0}", targetSession.DisplayName);

            var airInt = (int)Math.Round(50 * currentAir);
            var airPercent = (int)Math.Floor(currentAir * 100);

            Console.Write("Current Air: [");
            for (var i = 0; i < 50; i++)
            {
                if (i < airInt)
                    Console.BackgroundColor = ConsoleColor.Red;
                else Console.BackgroundColor = ConsoleColor.DarkGray;

                Console.Write(" ");
            }

            Console.BackgroundColor = ConsoleColor.Black;

            Console.Write("] {0}%", airPercent);
            if (isInFlow)
                Console.WriteLine(" [FLOW]");
            else
                Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Press ESC to stop");
        }
        
        private void Run()
        {
            LoadResources();

            this.PushMenu(new IntroMenu());

            while (this.currentMenu != null)
            {
                if (targetSession != null)
                {
                    StartAirflow();
                }
                
                RefreshMenu();

                var keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.DownArrow:
                        navigateSound.Play();
                        if (currentChoice == currentChoices.Count - 1)
                            currentChoice = 0;
                        else
                            currentChoice++;
                        break;
                    case ConsoleKey.UpArrow:
                        navigateSound.Play();
                        if (currentChoice == 0)
                            currentChoice = currentChoices.Count - 1;
                        else
                            currentChoice--;
                        break;
                    case ConsoleKey.Enter:
                        selectSound.Play();
                        currentChoices[currentChoice].Choose(this);
                        break;
                    case ConsoleKey.Backspace:
                    case ConsoleKey.Escape:
                        if (currentMenu.CanGoBack && previousMenus.Count > 0)
                            GoBack();
                        break;
                    
                }
            }
        }

        public void SetTargetSession(IAudioSession session)
        {
            this.targetSession = session;
            session.Volume = Environment.OSVersion.Platform == PlatformID.Unix
                ? lowAirVolumePipewire
                : lowAirVolume;
        }
        
        private void GoBack()
        {
            backSound.Play();

            currentChoice = 0;
            currentMenu = previousMenus.Pop();
            currentChoices.Clear();
            currentChoices.AddRange(currentMenu.Choices);
        }

        private void RefreshMenu()
        {
            Console.Clear();

            if (currentMenu == null)
                return;
            
            var sb = new StringBuilder();
            var title = currentMenu.Title;
            sb.AppendLine(title);
            for (var i = 0; i < title.Length; i++)
            {
                sb.Append('=');
            }

            sb.AppendLine();
            sb.AppendLine();

            if (string.IsNullOrWhiteSpace(currentMenu.Description))
            {
                sb.AppendLine(currentMenu.Description);
                sb.AppendLine();
            }

            Console.Write(sb.ToString());

            for (var i = 0; i < currentChoices.Count; i++)
            {
                if (i == currentChoice)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Green;
                }

                Console.WriteLine("  {0}. {1}", i + 1, currentChoices[i].Title);

                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            var description = currentChoices[currentChoice].Description;
            if (!string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine();
                Console.WriteLine(description);
            }
            
        }

        public void RefreshChoices()
        {
            this.currentChoices.Clear();
            this.currentChoice = 0;

            if (this.currentMenu != null)
                this.currentChoices.AddRange(this.currentMenu.Choices);
        }
        
        public void PushMenu(IConsoleMenu newMenu)
        {
            if (currentMenu != null)
                this.previousMenus.Push(currentMenu);

            currentMenu = newMenu;
            this.currentChoices.Clear();
            this.currentChoices.AddRange(this.currentMenu.Choices);
            this.currentChoice = 00;
        }

        public void StartListening()
        {
            this.keyListener.StartListening();
        }

        public void StopListening()
        {
            this.keyListener.StopListening();
        }

        private void RunX11KeyboardListener()
        {
            this.keyListener.STartX11Loop();
        }
        
        [STAThread]
        public static void Main(string[] args)
        {
            var program = new Program();

            var thread = new Thread(StartGuiMessageLoop);
            thread.Start(program);

            program.Run();
        }

        private static void StartGuiMessageLoop(object ctx)
        {
            Program program = ctx as Program;

            program.StartListening();

#if WINDOWS
            System.Windows.Forms.Application.Run();
            #else
            program.RunX11KeyboardListener();
#endif
            
            program.StopListening();
        }
    }

    public interface IConsoleMenu
    {
        string Title { get; }
        string Description { get; }
        bool CanGoBack { get; }
        
        IEnumerable<ConsoleMenuChoice> Choices { get; }
    }

    public class ConsoleMenuChoice
    {
        private Action<Program> selectDelegate;
        
        public string Title { get; }
        public string Description { get; }

        public ConsoleMenuChoice(string title, Action<Program> action)
        {
            this.Title = title;
            this.selectDelegate = action;
        }

        public ConsoleMenuChoice(string title, string description, Action<Program> action)
            : this(title, action)
        {
            this.Description = description;
        }
        
        public void Choose(Program program)
        {
            selectDelegate?.Invoke(program);
        }
    }
}