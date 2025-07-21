namespace SensorService
{
    public class RadarSensor : Sensor
    {
        public RadarSensor(string id, string name, int port, bool isBackup = false) :
            base(id, name, port, isBackup) { }
    }
}