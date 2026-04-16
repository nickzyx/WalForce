using WebServer.Features.Auth;
using WebServer.Features.Docs;
using WebServer.Features.Employee;
using WebServer.Features.Manager;

namespace WebServer.Extensions;

public static class RouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapWalForceApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDocumentationEndpoints();
        endpoints.MapAuthEndpoints();
        endpoints.MapEmployeeEndpoints();
        endpoints.MapManagerEndpoints();

        return endpoints;
    }
}
