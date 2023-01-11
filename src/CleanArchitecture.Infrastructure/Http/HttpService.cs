﻿using CleanArchitecture.Core.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Http;

/// <summary>
/// An implementation of IHttpService using HttpClient
/// </summary>
public class HttpService : IHttpService
{
  public async Task<int> GetUrlResponseStatusCodeAsync(string url)
  {
    using (var client = new HttpClient())
    {
      var result = await client.GetAsync(url);

      return (int)result.StatusCode;
    }
  }
}
