namespace SensorService
{
    public class LIDARSensor : Sensor
    {
        public LIDARSensor(string id, string name, int port, bool isBackup = false) :
            base(id, name, port, isBackup) { }
    }
}