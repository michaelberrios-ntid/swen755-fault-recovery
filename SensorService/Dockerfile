FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SensorService/SensorService.csproj", "SensorService/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "SensorService/SensorService.csproj"
COPY . .
RUN dotnet build "SensorService/SensorService.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "SensorService.dll"]