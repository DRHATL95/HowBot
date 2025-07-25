using Howbot.Web.Components;
using Howbot.Web.Hubs;
using Howbot.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

builder.Services.AddSignalR();

builder.Services.AddHttpClient<IBotApiService, BotApiService>(client =>
{
  client.BaseAddress = new Uri("https://localhost:7000");
});

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies")
    .AddDiscord(options =>
    {
      options.ClientId = builder.Configuration["Discord:ClientId"] ?? "";
      options.ClientSecret = builder.Configuration["Discord:ClientSecret"] ?? "";
      options.Scope.Add("guilds");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<BotHub>("/botHub");

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
  .AddInteractiveServerRenderMode();

app.Run();
