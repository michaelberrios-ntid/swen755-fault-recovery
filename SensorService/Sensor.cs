namespace SensorService
{
    /// <summary>
    /// Base class for all sensors in the system.
    /// This class provides common properties and methods for sensor management, including health checks and restart functionality.
    /// Each specific sensor type (e.g., LIDARSensor, CameraSensor, RadarSensor) will inherit from this class.
    /// </summary>
    public class Sensor
    {
        public string Id { get; }
        public string Name { get; }
        public int Port { get; }
        public bool IsBackup { get; }
        public double Health { get; private set; }
        public bool FallbackMode { get; private set; }

        private static readonly Random random = new();
        private const double HEALTHY_CAP = 0.9;
        private const double WARN_CAP = 0.8;
        private int fallbackRecover;


        /// <summary>
        /// Initializes a new instance of the <see cref="Sensor"/> class with the specified identifier and name.
        /// Sets the initial health to 1.0, disables fallback mode, and resets the fallback recovery counter.
        /// </summary>
        /// <param name="id">The unique identifier for the sensor.</param>
        /// <param name="name">The display name of the sensor.</param>
        public Sensor(string id, string name, int port, bool isBackup = false)
        {
            Id = id;
            Name = name;
            Port = port;
            IsBackup = isBackup;
            Health = 1.0;
            FallbackMode = false;
            fallbackRecover = 0;
        }

        /// <summary>
        /// Checks the health of the sensor and returns a status message.
        /// If the sensor is in fallback mode, it will increment the recovery counter.
        /// If the health is below the warning threshold, it will enter fallback mode.
        /// </summary>
        /// <returns>A string indicating the health status of the sensor.</returns>
        /// <remarks>
        /// The health is randomly adjusted based on predefined probabilities:
        /// - 90% chance to be between 0.9 and 1.0
        /// - 8% chance to be between 0.8 and 0.9
        /// - 2% chance to be between 0.5 and 0.8
        /// </remarks>
        public string CheckHealth()
        {
            if (FallbackMode && fallbackRecover < 3)
            {
                fallbackRecover++;
                return Format(Health, "FALLBACK");
            }

            double roll = random.NextDouble();

            if (roll < 0.90)
                Health = 0.9 + random.NextDouble() * 0.1;
            else if (roll < 0.98)
                Health = 0.8 + random.NextDouble() * 0.1;
            else
                Health = 0.5 + random.NextDouble() * 0.3;

            if (Health < WARN_CAP)
            {
                FallbackMode = true;
                return Format(Health, "FAIL");
            }

            if (Health < HEALTHY_CAP)
                return Format(Health, "WARN");

            return Format(Health, "HEALTHY");
        }

        /// <summary>
        /// Restarts the sensor and resets its health and fallback mode.
        /// </summary>
        /// <returns>A string indicating the sensor has been restarted and its new health status.</returns>
        /// <remarks>
        /// This method resets the health to 0.95, disables fallback mode, and resets the fallback recovery counter.
        /// </remarks>
        public string RestartAndReport()
        {
            FallbackMode = false;
            fallbackRecover = 0;
            Health = 0.95;
            return Format(Health, "RESTARTED");
        }

        private string Format(double health, string status)
        {
            return $"\t{health:P2}\t{status}";
        }
    }
}