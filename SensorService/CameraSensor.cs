namespace SensorService
{
    public class CameraSensor : Sensor
    {
        public CameraSensor(string id, string name, int port, bool isBackup = false) :
            base(id, name, port, isBackup) { }
    }
}