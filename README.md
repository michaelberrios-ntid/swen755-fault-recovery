# SWEN 755 - Fault Recovery Tactic Implementation

TThis project implements the Fault Recovery with Redundancy tactic for critical components in an autonomous system, including LIDAR, camera, and radar sensors. It builds on the Heartbeat tactic by adding failover and recovery using passive redundancy.

Each sensor (primary or backup) runs in its own process, and all components communicate over TCP across separate Docker containers. A central monitor continuously pings the sensors. If a primary sensor fails or becomes unresponsive, the monitor promotes a backup sensor to take over. If the primary recovers, control is handed back.

The system simulates realistic failure and recovery behavior using randomized health checks, real process isolation, and inter-container socket messaging. Built in C# with .NET 9 console apps and deployed using Docker and Docker Compose

# Fault Recovery Implementation
Created Docker file

```
docker build -t sensorservice . 
```

# Project Structure
```
HeartbeatTactic/
├── SensorService/        # Contains sensor implementations
│   ├── Program.cs        # Main entry point for the sensor service  (cluster of sensors)
│   ├── Sensor.cs         # Base class for sensors
│   ├── LidarSensor.cs    # LIDAR sensor implementation
│   ├── CameraSensor.cs   # Camera sensor implementation
│   └── RadarSensor.cs    # Radar sensor implementation
├── Monitor/              # Contains the monitor implementation
│   ├── Program.cs        # Main entry point for the monitor program 
├── Shared/               # Contains shared code and data structures
│   ├── SensorRegistry.cs # Registry of all sensors
│   └── SensorMessages.cs # Data structure for sensor messages
├── Dockerfile            # Dockerfile for building the sensor service
```

# Requirements
- .NET SDK 7.0 or later - https://dotnet.microsoft.com/en-us/download
- C# development environment
    - VSC with C# Extensions 
    - Or Visual Studio

# Compile and Run
## Start the Sensor Service (Cluster of Sensors)
### Terminal 1
```bash
dotnet run --project SensorService
```

## Start the Monitor
### Terminal 2
```bash
dotnet run --project Monitor
```

### (Optionally) Run Monitor with custom sleep time slower heartbeat checks
#### Must be greater than 500ms
```bash
dotnet run --project Monitor -- 3000
```

## Stop the Monitor
### Terminal 2
```bash
ctrl + C
``` 

## Stop the Sensor Service
### Terminal 1
```bash
ctrl + C
```

# Sample Output
## Healthy Sensor Status
```bash
Sensor Name                     Health  Status
LIDAR Front               → 	92.69%	HEALTHY
LIDAR Rear                → 	98.74%	HEALTHY
Camera Left               → 	99.11%	HEALTHY
Camera Right              → 	95.87%	HEALTHY
Radar Front               → 	99.19%	HEALTHY
Radar Rear                → 	95.22%	HEALTHY
```

## Sensor Fails Falls Back to Backup
```bash
Sensor Name                     Health  Status
LIDAR Front               → 	54.98%	FAIL
→ Switching to backup: LIDAR Front Backup
LIDAR Rear                → 	95.11%	HEALTHY
Camera Left               → 	92.06%	HEALTHY
Camera Right              → 	92.43%	HEALTHY
Radar Front               → 	90.59%	HEALTHY
Radar Rear                → 	92.70%	HEALTHY
```

## Sensor Fails Falls Back to Backup
```bash
Sensor Name                     Health  Status
LIDAR Front Backup        → 	92.50%	HEALTHY
→ Primary recovered. Switching back to: LIDAR Front
LIDAR Rear                → 	95.11%	HEALTHY
Camera Left               → 	92.06%	HEALTHY
Camera Right              → 	92.43%	HEALTHY
Radar Front               → 	90.59%	HEALTHY
Radar Rear                → 	92.70%	HEALTHY
```

## Sensors Warnings when Health Drops Below Threshold 90%
```bash
Sensor Name                     Health  Status
LIDAR Front               → 	92.50%	HEALTHY
LIDAR Rear                → 	95.11%	HEALTHY
Camera Left               → 	85.00%	WARN
Camera Right              → 	92.43%	HEALTHY
Radar Front               → 	87.61%	WARN
Radar Rear                → 	92.70%	HEALTHY
```
