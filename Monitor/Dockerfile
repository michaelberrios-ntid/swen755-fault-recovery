FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Monitor/Monitor.csproj", "Monitor/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Monitor/Monitor.csproj"
COPY . .
RUN dotnet build "Monitor/Monitor.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "Monitor.dll"]