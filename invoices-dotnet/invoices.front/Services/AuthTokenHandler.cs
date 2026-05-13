using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using invoices.front.Services.Abstractions;

namespace invoices.front.Services;

public class AuthTokenHandler(IAuthClient _authClient) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (_authClient.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authClient.Token);
        }

        return base.SendAsync(request, ct);
    }
}
