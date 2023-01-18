# Howbot

A music and general purpose bot for Discord written in C# using .NET7 Worker Service

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Getting Started

Clone or download the repository.

Install the ef core cli tools `dotnet tool install --global dotnet-ef`. If you already have an old version, first try `dotnet tool update --global dotnet-ef  --version 6.0.0-*`, if that doesn't work, see [Updating Ef Core Cli](https://github.com/aspnet/EntityFrameworkCore/issues/14016#issuecomment-487308603) First, delete C:\Users\{yourUser}\.dotnet\tools\.store\dotnet-ef tool.

This app is currently configured to run against a Postgres SQL. To initialized the database, you will need to run this command in the /src/Howbot.Worker folder:

```powershell
dotnet ef database update -c appdbcontext -p ../Howbot.Infrastructure/Howbot.Infrastructure.csproj -s Howbot.Worker.csproj
```

Check the connection string in `appsettings.json` in the Howbot.Worker project to verify its details if you have problems.

Open the solution in Visual Studio and run it with ctrl-F5 (the Howbot.Worker project should be the startup project) or in the console go to the `src/Howbot.Worker` folder and run `dotnet run`.

On startup the app queues up 10 URLs to hit (google.com) and you should see it make 10 requests and save them to the database and then do nothing, logging each second.

## References

- [Clean Architecture template for ASP.NET Core solutions](https://github.com/ardalis/CleanArchitecture)
- [Creating a Clean Architecture Worker Service Template](https://www.youtube.com/watch?v=_jfnnAMNb94) ([Twitch](https://twitch.tv/ardalis) Stream 1)
- [Creating a Clean Architecture Worker Service Template](https://www.youtube.com/watch?v=Nttt33GoTXg) ([Twitch](https://twitch.tv/ardalis) Stream 2)

Useful Pluralsight courses:
- [SOLID Principles of Object Oriented Design](https://www.pluralsight.com/courses/principles-oo-design)
- [SOLID Principles for C# Developers](https://www.pluralsight.com/courses/csharp-solid-principles)
- [Domain-Driven Design Fundamentals](https://www.pluralsight.com/courses/domain-driven-design-fundamentals)
