using System.Net;
using System.Net.Sockets;
using System.Text;
using Shared;

namespace SensorService
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start each sensor server in its own thread
            foreach (var sensor in SensorRegistry.All)
            {
                var thread = new Thread(() => StartSensorServer(sensor));
                thread.Start();
            }

            Console.WriteLine("All sensors started.");
            Thread.Sleep(Timeout.Infinite); // keep app running
        }

        /// <summary>
        /// Starts a sensor server for the given sensor information.
        /// This is a sensor-cluster
        /// </summary>
        static void StartSensorServer(SensorInfo sensorInfo)
        {
            // Create a sensor instance based on the type by the metadata in SensorInfo
            // SensorInfo includes the ID, name, port, and whether it is a backup sensor
            // This allows us to instantiate the correct sensor type dynamically
            // SensorRegistry lives in the Shared/ namespace
            Sensor sensor = sensorInfo.Id switch
            {
                var id when id.StartsWith("lidar") => new LIDARSensor(sensorInfo.Id, sensorInfo.Name, sensorInfo.Port, sensorInfo.IsBackup),
                var id when id.StartsWith("camera") => new CameraSensor(sensorInfo.Id, sensorInfo.Name, sensorInfo.Port, sensorInfo.IsBackup),
                var id when id.StartsWith("radar") => new RadarSensor(sensorInfo.Id, sensorInfo.Name, sensorInfo.Port, sensorInfo.IsBackup),
                _ => throw new ArgumentException($"Unknown sensor type in ID: {sensorInfo.Id}")
            };

            var listener = new TcpListener(IPAddress.Loopback, sensorInfo.Port);
            listener.Start();

            Console.WriteLine($"Sensor '{sensorInfo.Name}' is listening on port {sensorInfo.Port}...");

            while (true)
            {
                using var client = listener.AcceptTcpClient();
                using var stream = client.GetStream();

                byte[] buffer = new byte[256];
                int read = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, read).Trim().ToUpper();

                // Handle the request based on the command received
                string response = request switch
                {
                    SensorMessages.PING => sensor.CheckHealth(),
                    SensorMessages.RESTART => sensor.RestartAndReport(),
                    _ => "UNKNOWN_COMMAND"
                };

                byte[] reply = Encoding.UTF8.GetBytes(response);
                stream.Write(reply, 0, reply.Length);
            }
        }
    }
}
