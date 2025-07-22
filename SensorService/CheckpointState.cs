namespace SensorService
{
    public record CheckpointState(double Health, bool FallbackMode, int FallbackRecover);
}