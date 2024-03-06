using Microsoft.AspNetCore.Components;

namespace Howbot.Web.Dashboard.Components.Layout;

public partial class NavMenu : ComponentBase
{
  private bool IsAuthenticated { get; set; } = false;
  private string LoggedInUserName { get; set; } = string.Empty;

  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();

    await CheckUserLoggedIn();
  }

  private async Task CheckUserLoggedIn()
  {
    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
    var user = authState.User;

    if (user.Identity != null)
    {
      IsAuthenticated = user.Identity.IsAuthenticated;

      if (IsAuthenticated)
      {
        LoggedInUserName = user.Identity.Name ?? string.Empty;
      }
    }
  }

  private void Logout()
  {
    if (!IsAuthenticated) return;

    // Call the blazor server logout endpoint
    NavigationManager.NavigateTo("/authentication/logout", forceLoad: true);
  }
}
