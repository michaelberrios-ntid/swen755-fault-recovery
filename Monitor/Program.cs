using Shared;

namespace Monitor
{
    class Program
    {
        static void Main(string[] args)
        {
            int sleepTime = 1000;

            // Read arguments for specified sleep time between checks
            if (args.Length > 0 && int.TryParse(args[0], out sleepTime) && sleepTime < 500)
            {
                Console.WriteLine("Sleep time must be at least 500ms. Using default of 1000ms.");
                sleepTime = 1000;
            }

            SensorMonitor monitor = new SensorMonitor(sleepTime);

            Thread monitorThread = new Thread(monitor.StartMonitoring);

            monitorThread.Start();

            Console.WriteLine("\nHeartbeat Monitor display starting...");

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Sensor Status - {DateTime.Now:HH:mm:ss}\n");
                Console.WriteLine($"{"Sensor Name",-32}{"Health",-8}{"Status"}");

                Dictionary<string, SensorInfo> currentSensors = monitor.GetActiveSensors();

                foreach (var (id, currentSensor) in currentSensors)
                {
                    string hostType = currentSensor.IsBackup ? "backup" : "primary";
                    Console.WriteLine($"{currentSensor.Name,-25} → {currentSensor.LastStatus,-8}");
                }

                Console.WriteLine("\nPress Ctrl+C to exit.");

                Thread.Sleep(sleepTime);
            }
        }
    }
}