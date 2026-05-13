using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using invoices.front.Services.Abstractions;

namespace invoices.front.Services;

public class AuthTokenHandler(IAuthClient authClient) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct
    )
    {
        if (authClient.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                authClient.Token
            );
        }

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized && authClient.IsAuthenticated)
        {
            // Try to refresh
            var success = await authClient.TryRefreshAsync(ct);
            if (success)
            {
                // Update the request with the new token
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    authClient.Token
                );
                
                // Retry the request
                response.Dispose();
                response = await base.SendAsync(request, ct);
            }
        }

        return response;
    }
}
