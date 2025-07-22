using System.Net.Sockets;
using System.Text;
using Shared;
using System.Collections.Concurrent;

namespace MonitorApp
{
    public class SensorMonitor
    {
        private readonly int _sleepTime;
        private readonly ConcurrentDictionary<string, SensorInfo> _activeSensors;
        private readonly Dictionary<string, SensorInfo> _backupLookup;
        private readonly Dictionary<string, SensorInfo> _primaryLookup;
        private volatile bool _isRunning;

        public SensorMonitor(int sleepTime)
        {
            _sleepTime = sleepTime;
            _activeSensors = new ConcurrentDictionary<string, SensorInfo>();

            foreach (var sensor in SensorRegistry.All)
            {
                if (!sensor.IsBackup)
                    _activeSensors.TryAdd(sensor.Id, sensor);
            }

            _backupLookup = SensorRegistry.All
                .Where(s => s.IsBackup)
                .ToDictionary(
                    backup => backup.Id.Replace("-backup", ""),
                    backup => backup
                );

            _primaryLookup = SensorRegistry.All
                .Where(s => !s.IsBackup)
                .ToDictionary(
                    primary => primary.Id,
                    primary => primary
                );

            _isRunning = true;
        }

        // This method will be run in the separate thread
        public void StartMonitoring()
        {
            Console.WriteLine("Heartbeat Monitor starting up...");

            while (_isRunning)
            {
                // Iterate over a snapshot of the active sensors
                foreach (var (id, currentSensor) in _activeSensors.ToList())
                {
                    string host = currentSensor.IsBackup ? "backup-sensor-cluster" : "primary-sensor-cluster";
                    string status = PingSensor(host, currentSensor.Port);

                    // Create a new SensorInfo record with the updated status
                    SensorInfo updatedSensor = currentSensor with { LastStatus = status };

                    _activeSensors.AddOrUpdate(id, updatedSensor, (key, existingValue) => updatedSensor);

                    // Handle the fallback logic if a primary sensor fails
                    if (status.Contains("FALLBACK") || status.Contains("FAIL"))
                    {
                        if (!currentSensor.IsBackup && _backupLookup.TryGetValue(id, out var backup))
                        {
                            SensorInfo backupWithStatus = backup with { LastStatus = "FAIL - SWITCHING TO BACKUP" };
                            _activeSensors.AddOrUpdate(id, backupWithStatus, (key, existingValue) => backupWithStatus);
                            // Force the backup to load checkpoint
                            PingSensor("backup-sensor-cluster", backup.Port, SensorMessages.RELOAD);
                        }
                    }
                    // Handle the fallback logic if a primary sensor recovers, switch back to the primary sensor
                    else if (currentSensor.IsBackup && _primaryLookup.TryGetValue(id, out var primary))
                    {
                        // Check if primary is now healthy
                        host = "primary-sensor-cluster";
                        string primaryStatus = PingSensor(host, primary.Port);
                        if (primaryStatus.Contains("HEALTHY") || primaryStatus.Contains("WARN"))
                        {
                            SensorInfo primaryWithStatus = primary with { LastStatus = "PRIMARY RECOVERED" };
                            _activeSensors.AddOrUpdate(id, primaryWithStatus, (key, existingValue) => primaryWithStatus);
                        }
                    }
                }

                Thread.Sleep(_sleepTime);
            }
            Console.WriteLine("Heartbeat Monitor shutting down.");
        }

        public void StopMonitoring()
        {
            _isRunning = false;
        }

        // Method to safely get the current active sensors for display in the main thread
        public Dictionary<string, SensorInfo> GetActiveSensors()
        {
            return _activeSensors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Pings a sensor and returns its status.
        /// </summary>
        /// <param name="host">The host address of the sensor.</param>
        /// <param name="port">The port number of the sensor.</param>
        /// <returns>Status message from the sensor.</returns>
        static string PingSensor(string host, int port, string message = SensorMessages.PING)
        {
            try
            {
                using var client = new TcpClient(host, port);
                using var stream = client.GetStream();

                byte[] payload = Encoding.UTF8.GetBytes(message);
                stream.Write(payload, 0, payload.Length);

                byte[] buffer = new byte[256];
                int read = stream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, read);
            }
            catch (Exception ex)
            {
                return $"FAIL";
            }
        }
    }
}