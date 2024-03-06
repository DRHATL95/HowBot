using System.Security.Claims;
using Howbot.Core.Settings;
using Howbot.Web.Dashboard.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add HttpClientFactory to web api
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5284") });

builder.Services.AddAuthentication(options =>
  {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Discord";
  })
  .AddCookie(options =>
  {
    options.Cookie.HttpOnly = true; // Can only be accessed by the server, prevents XSS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requires HTTPS, if HTTP the cookie will not be sent
  })
  .AddDiscord(options =>
  {
    options.CallbackPath = "/oauth-callback";
    options.ClientId = Configuration.DiscordOAuthClientId;
    options.ClientSecret = Configuration.DiscordOAuthClientSecret;
    options.Scope.Add("identify");
    options.Scope.Add("guilds");
    options.Events = new OAuthEvents()
    {
      OnCreatingTicket = async context =>
      {
        var accessToken = context.AccessToken;

        if (context.Principal?.Identity is ClaimsIdentity identity)
        {
          if (!string.IsNullOrEmpty(accessToken))
          {
            identity.AddClaim(new Claim("access_token", accessToken));
          }
        }

        await Task.CompletedTask.ConfigureAwait(false);
      }
    };
  });

builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}
else
{
  app.UseDeveloperExceptionPage();
}

app.MapGet("/authentication/login/discord", async context =>
{
  var properties = new AuthenticationProperties { RedirectUri = "/" };
  await context.ChallengeAsync("Discord", properties).ConfigureAwait(false);
});

app.MapGet("/authentication/logout", async context =>
{
  await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
  context.Response.Redirect("/");
});

app.UseCookiePolicy();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
  .AddInteractiveServerRenderMode();

app.Run();
