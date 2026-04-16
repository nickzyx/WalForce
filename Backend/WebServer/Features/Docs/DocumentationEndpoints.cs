namespace WebServer.Features.Docs;

public static class DocumentationEndpoints
{
    public static IEndpointRouteBuilder MapDocumentationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", () => TypedResults.Ok(new
            {
                name = "WalForce Backend API",
                swaggerUi = "/swagger",
                openApiDocument = "/swagger/v1/swagger.json",
                loginEndpoint = "/api/auth/login",
                authentication = "Use the returned accessToken as Authorization: Bearer <token>.",
                employeeEndpoints = new[]
                {
                    "/api/me/profile",
                    "/api/me/schedule?from=YYYY-MM-DD&to=YYYY-MM-DD",
                    "/api/me/availability"
                },
                managerEndpoints = new[]
                {
                    "/api/manager/roster",
                    "/api/manager/schedule?from=YYYY-MM-DD&to=YYYY-MM-DD"
                }
            }))
            .WithTags("Docs")
            .WithName("GetApiInfo")
            .WithSummary("Get basic API usage information for the prototype.");

        return endpoints;
    }
}
