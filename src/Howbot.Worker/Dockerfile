FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore as distinct layers
COPY ["src/Howbot.Worker/Howbot.Worker.csproj", "src/Howbot.Worker/"]
RUN dotnet restore "src/Howbot.Worker/Howbot.Worker.csproj" 

COPY . .
WORKDIR "/src/src/Howbot.Worker"
RUN dotnet build "Howbot.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Howbot.Worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Howbot.Worker.dll"]
