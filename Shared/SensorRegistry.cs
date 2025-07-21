namespace Shared
{
    /// <summary>
    /// SensorRegistry is a static class that holds a list of all sensors available in the system.
    /// Each sensor is represented by a SensorInfo record containing its ID, name, port,
    /// and whether it is a backup sensor.
    /// </summary>
    public static class SensorRegistry
    {
        /// <summary>
        /// A list of all sensors available in the system.
        /// Each sensor is represented by a SensorInfo record containing its ID, name, port, and whether it is a backup sensor.
        /// This list can be used to register sensors, check their availability, or manage sensor configurations.
        /// </summary>
        public static readonly List<SensorInfo> All = new()
        {
            new SensorInfo("lidar-1", "LIDAR Front", 9001),
            new SensorInfo("lidar-1-backup", "LIDAR Front Backup", 9002, true),
            new SensorInfo("lidar-2", "LIDAR Rear", 9003),
            new SensorInfo("lidar-2-backup", "LIDAR Rear Backup", 9004, true),

            new SensorInfo("camera-1", "Camera Left", 9005),
            new SensorInfo("camera-1-backup", "Camera Left Backup", 9006, true),
            new SensorInfo("camera-2", "Camera Right", 9007),
            new SensorInfo("camera-2-backup", "Camera Right Backup", 9008, true),

            new SensorInfo("radar-1", "Radar Front", 9009),
            new SensorInfo("radar-1-backup", "Radar Front Backup", 9010, true),
            new SensorInfo("radar-2", "Radar Rear", 9011),
            new SensorInfo("radar-2-backup", "Radar Rear Backup", 9012, true)
        };
    }

    public record SensorInfo(string Id, string Name, int Port, bool IsBackup = false);
}
