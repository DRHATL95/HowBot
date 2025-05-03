# Howbot

A music and general purpose bot for Discord written in C# using .NET9 Worker Service

## Give a Star! :star:

If you like this project, please give it a star. Thanks!

## Getting Started

Clone or download the repository.

Install the ef core cli tools `dotnet tool install --global dotnet-ef`. If you already have an old version, first
try `dotnet tool update --global dotnet-ef --version 8.0.0-*`, if that doesn't work,
see [Updating Ef Core Cli](https://github.com/aspnet/EntityFrameworkCore/issues/14016#issuecomment-487308603) First,
delete C:\Users\{yourUser}\.dotnet\tools\.store\dotnet-ef tool.

This app is currently configured to run against a Postgres SQL. To initialized the database, you will need to run this
command in the /src/Howbot.Worker folder:

```powershell
dotnet ef database update -c appdbcontext -p ../Howbot.Infrastructure/Howbot.Infrastructure.csproj -s Howbot.Worker.csproj
```

Check the connection string in `appsettings.json` in the Howbot.Worker project to verify its details if you have
problems.

Open the solution in Visual Studio and run it with ctrl-F5 (the Howbot.Worker project should be the startup project) or
in the console go to the `src/Howbot.Worker` folder and run `dotnet run`.
