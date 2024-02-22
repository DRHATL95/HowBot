FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Howbot.Web.API/Howbot.Web.API.csproj", "src/Howbot.Web.API/"]
RUN dotnet restore "src/Howbot.Web.API/Howbot.Web.API.csproj"
COPY . .
WORKDIR "/src/src/Howbot.Web.API"
RUN dotnet build "Howbot.Web.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Howbot.Web.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Howbot.Web.API.dll"]
