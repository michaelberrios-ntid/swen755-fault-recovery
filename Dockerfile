# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and projects
COPY *.sln ./
COPY SensorService/*.csproj ./SensorService/
COPY Monitor/*.csproj ./Monitor/
COPY Shared/*.csproj ./Shared/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build all projects
RUN dotnet publish SensorService/SensorService.csproj -c Release -o /out/SensorService
RUN dotnet publish Monitor/Monitor.csproj -c Release -o /out/Monitor

# Final image
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Set environment variable to show console output immediately
ENV DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION=1

# Copy built apps from build stage
COPY --from=build /out/SensorService ./SensorService
COPY --from=build /out/Monitor ./Monitor

# Default entrypoint (override in docker-compose)
CMD ["dotnet", "Monitor/Monitor.dll"]