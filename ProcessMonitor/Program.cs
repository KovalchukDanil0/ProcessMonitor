namespace ProcessMonitor
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            ProcessMonitor processMonitor;
            if (args.Length == 3) // Check if arguments exist
            {
                processMonitor = new ProcessMonitor(args[0], Convert.ToInt32(args[1]), Convert.ToSingle(args[2]));
                processMonitor.PrintValues();
            }
            else
                processMonitor = new ProcessMonitor();

            processMonitor.ProcessHolder(); // Start main process function
        }
    }
}
