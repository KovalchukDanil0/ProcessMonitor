using System.Diagnostics;
using System.Timers;
using Colored;

using Timer = System.Timers.Timer;

namespace ProcessMonitor
{
    /// <summary>
    /// A class that monitors processes with given process name, lifetime, and check frequency parameters
    /// </summary>
    public class ProcessMonitor
    {
        /// <summary>
        /// Constructor with parameters for external use
        /// </summary>
        /// <param name="_processName"></param>
        /// <param name="_lifetime"></param>
        /// <param name="_frequencyСheck"></param>
        public ProcessMonitor(string _processName, int _lifetime = 5, float _frequencyСheck = 1)
        {
            processName = _processName;
            lifetime = _lifetime;
            frequency = _frequencyСheck;
        }

        /// <summary>
        /// Empty constructor initializes input variables
        /// </summary>
        public ProcessMonitor() => InitVariables();

        /// <summary>
        /// Array of system processes
        /// </summary>
        readonly string[] systemProcesses = new string[] { "explorer", "ntoskrnl", "WerFault", "backgroundTaskHost", "backgroundTransferHost", "winlogon",
        "wininit", "csrss", "lsass", "smss", "services", "taskeng", "taskhost", "dwm", "conhost", "svchost", "sihost" };

        /// <summary>
        /// Timer for the main program loop - checking the lifetime of the process
        /// </summary>
        Timer? timer;

        string processName = "explorer";
        /// <summary>
        /// Name of the selected process
        /// </summary>
        public string ProcessName => processName;

        int lifetime = 5;
        /// <summary>
        /// Process lifetime
        /// </summary>
        public int Lifetime => lifetime;

        float frequency = 1;
        /// <summary>
        /// Calling a timer with a given frequency
        /// </summary>
        public float Frequency => frequency;

        // !!! An example of a shortcut with arguments located in the ...\ProcessMonitor\ProcessMonitor\bin\Debug\net6.0\ folder

        /// <summary>
        /// The main function that sets the timer and keeps the program from closing
        /// </summary>
        public void ProcessHolder()
        {
            SetTimer();

            // If the Q button was pressed, the timer stops and the program ends.
            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                if (timer != null)
                    timer.Stop();
            }
        }

        /// <summary>
        /// Prints all found processes to the console
        /// </summary>
        void PrintProcesses()
        {
            ColoredLine.Write("All processes.\nAttention the red ones are system processes\n", ConsoleColor.DarkMagenta);

            string lastName = string.Empty;
            ConsoleColor consoleColor = ConsoleColor.Gray;

            foreach (string procName in Process.GetProcesses().Select(proc => proc.ProcessName)) // A loop that writes all processes and determines if these are system.
            {
                // If this process name matches the previous one, then we skip the array check. The magic of optimization.
                // On my machine, I improved the speed of checking by 25%.
                if (procName != lastName)
                {
                    if (Array.IndexOf(systemProcesses, procName) >= 0)
                        consoleColor = ConsoleColor.Red;
                    else
                        consoleColor = ConsoleColor.Gray;

                    lastName = procName;
                }

                ColoredLine.Write(procName, consoleColor);
            }

            ColoredLine.Write("\n--------------------------------------------------------------\n", ConsoleColor.DarkMagenta);
        }

        /// <summary>
        /// Initialization of required variables
        /// </summary>
        public void InitVariables()
        {
            PrintProcesses();

            ColoredLine.Write("Enter the process from the list above");

            processName = Console.ReadLine() ?? processName;
            lifetime = ColoredLine.WriteAndRead("\nEnter lifetime in minutes", defaultValue: lifetime);
            frequency = ColoredLine.WriteAndRead("\nEnter frequency check in minutes", defaultValue: frequency);
        }

        /// <summary>
        /// Print processName, lifetime and frequency to the console
        /// </summary>
        public void PrintValues()
        {
            ColoredLine.Write($"Process name - {processName}, lifetime of the process - {lifetime}, timer check frequency - {frequency}", ConsoleColor.DarkMagenta);
        }

        /// <summary>
        /// Setting the timer and then starting it
        /// </summary>
        void SetTimer()
        {
            // Create a timer with a two second intervasl.
            timer = new(frequency * 60000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;

            ColoredLine.Write("\nTo exit the program, press the Q button\n", ConsoleColor.Red);
            CheckLifetime();
        }

        /// <summary>
        /// The function is called when the timer expires
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        void OnTimedEvent(object? obj, ElapsedEventArgs args)
        {
            CheckLifetime();
        }

        /// <summary>
        /// Check if the process exists then kill it if it lives too long
        /// </summary>
        void CheckLifetime()
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                // If a process with this name is not found, then output a message to the console and interrupt the function.
                ColoredLine.Write($"There are no processes named {processName}");
                return;
            }

            foreach (Process process in processes)
            {
                // Determines if the process has closed. Useful when the main process closes and with it automatically all subprocesses too.
                if (process.HasExited)
                {
                    ColoredLine.Write($"The {processName} has exited", ConsoleColor.Red);
                    break;
                }

                ColoredLine.Write($"{process.ProcessName} {process.Id} {process.StartTime}");

                // Calculate the time in minutes between the system date and the process start time.
                TimeSpan ts = DateTime.Now - process.StartTime;
                if (ts.Minutes >= lifetime) // If a few minutes is more than the specified process lifetime, then the process is killed
                {
                    ColoredLine.Write($"The {process.ProcessName}({process.Id}) was killed\n", ConsoleColor.Red);
                    process.Kill();
                }
            }
        }
    }
}
