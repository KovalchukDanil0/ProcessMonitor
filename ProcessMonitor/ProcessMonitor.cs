using System.Diagnostics;
using System.Timers;

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
        public ProcessMonitor(string _processName, int _lifetime, float _frequencyСheck)
        {
            Main(new string[3] { _processName, _lifetime.ToString(), _frequencyСheck.ToString() });
        }

        /// <summary>
        /// Array of system processes
        /// </summary>
        static readonly string[] systemProcesses = new string[] { "explorer", "ntoskrnl", "WerFault", "backgroundTaskHost", "backgroundTransferHost", "winlogon",
        "wininit", "csrss", "lsass", "smss", "services", "taskeng", "taskhost", "dwm", "conhost", "svchost", "sihost" };

        /// <summary>
        /// Timer for the main program loop - checking the lifetime of the process
        /// </summary>
        static Timer? timer;

        /// <summary>
        /// Name of the selected process
        /// </summary>
        static string processName = "explorer";
        /// <summary>
        /// Process lifetime
        /// </summary>
        static int lifetime = 1;
        /// <summary>
        /// Calling a timer with a given frequency
        /// </summary>
        static float frequencyСheck = 1;

        /// <summary>
        /// Returns true if the user has given arguments to the program
        /// </summary>
        static bool withArguments = false;

        /// <summary>
        /// The function is called when the program starts
        /// </summary>
        /// <param name="args">Given arguments to the program</param>
        /// <exception cref="Exception"></exception>
        static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                withArguments = true; // If arguments was exist.
            }
            else
            {
                WriteColoredLine("All processes.\nAttention the red ones are system processes\n", ConsoleColor.Magenta);

                string lastName = string.Empty;
                ConsoleColor consoleColor = ConsoleColor.Gray;
                foreach (Process _process in Process.GetProcesses()) // A loop that determines if these are system processes.
                {
                    // If this process name matches the previous one, then we skip the array check. The magic of optimization.
                    // On my machine, I improved the speed of checking by 25%.
                    if (_process.ProcessName != lastName)
                    {
                        if (Array.IndexOf(systemProcesses, _process.ProcessName) >= 0)
                            consoleColor = ConsoleColor.Red;
                        else
                            consoleColor = ConsoleColor.Gray;

                        lastName = _process.ProcessName;
                    }

                    WriteColoredLine(_process.ProcessName, consoleColor);
                }

                WriteColoredLine("\n--------------------------------------------------------------\n", ConsoleColor.Magenta);
            }

            InitVariables(args);
            SetTimer();

            // If the Q button was pressed, the timer stops and the program ends.
            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                if (timer != null)
                    timer.Stop();
                else
                    throw new NullReferenceException();
            }
        }

        /// <summary>
        /// Custom Console.WriteLine() function with line coloring support
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        static void WriteColoredLine(string message = "", ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Custom function that writes a string and returns the values entered by the user
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        static string WriteAndReadColLine(string message = "", ConsoleColor color = ConsoleColor.Gray)
        {
            WriteColoredLine(message, color);
            // If the value of the variable is null, then the function returns an empty string. 
            return Console.ReadLine() ?? string.Empty;
        }

        /// <summary>
        /// Initialization of required variables
        /// </summary>
        /// <param name="args"></param>
        static void InitVariables(string[] args)
        {
            if (withArguments)
            {
                if (args[0] != string.Empty)
                    processName = args[0];

                // If numeric variables not empty.
                if (args[1] != string.Empty && args[2] != string.Empty)
                {
                    SetNumVariables(args[1], args[2], args);
                    WriteColoredLine($"process name = {processName}, lifetime = {lifetime}, frequency check = {frequencyСheck}", ConsoleColor.Magenta);
                }
            }
            else
            {
                WriteColoredLine("Enter the process from the list above");
                processName = Console.ReadLine() ?? processName;

                SetNumVariables(WriteAndReadColLine("\nenter lifetime in minutes"), WriteAndReadColLine("\nenter frequency check in minutes"), args);
            }
        }

        /// <summary>
        /// initialization of numeric variables
        /// </summary>
        /// <param name="_lifetime"></param>
        /// <param name="_frequencyСheck"></param>
        /// <param name="args"></param>
        static void SetNumVariables(string _lifetime, string _frequencyСheck, string[] args)
        {
            // Try to convert string to int and float.
            try
            {
                lifetime = Convert.ToInt32(_lifetime);
                frequencyСheck = Convert.ToSingle(_frequencyСheck);
            }
            catch (FormatException ex)
            {
                // If an error was occured, then print it to the console and initialize the variables again.
                WriteColoredLine($"\n{ex}", ConsoleColor.Red);
                WriteColoredLine("\nEnter text in the correct format\n", ConsoleColor.Magenta);
                InitVariables(args);
            }
        }

        /// <summary>
        /// Setting the timer and then starting it
        /// </summary>
        static void SetTimer()
        {
            // Create a timer with a two second intervasl.
            timer = new(frequencyСheck * 60000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;

            WriteColoredLine("\nTo exit the program, press the Q button\n", ConsoleColor.Red);
            CheckLifetime();
        }

        /// <summary>
        /// The function is called when the timer expires
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        static void OnTimedEvent(object? obj, ElapsedEventArgs args)
        {
            CheckLifetime();
        }

        /// <summary>
        /// Check if the process exists then kill it if it lives too long
        /// </summary>
        static void CheckLifetime()
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                // If a process with this name is not found, then output a message to the console and interrupt the function.
                WriteColoredLine($"There are no processes named {processName}");
                return;
            }

            foreach (Process process in processes)
            {
                // Determines if the process has closed. Useful when the main process closes and with it automatically all subprocesses too.
                if (process.HasExited)
                {
                    WriteColoredLine($"The {processName} has exited", ConsoleColor.Red);
                    break;
                }

                WriteColoredLine($"{process.ProcessName} {process.Id} {process.StartTime}");

                // Calculate the time in minutes between the system date and the process start time.
                TimeSpan ts = DateTime.Now - process.StartTime;
                if (ts.Minutes >= lifetime) // If a few minutes is more than the specified process lifetime, then the process is killed
                {
                    WriteColoredLine($"The {process.ProcessName}({process.Id}) was killed\n", ConsoleColor.Red);
                    process.Kill();
                }
            }
        }
    }
}
