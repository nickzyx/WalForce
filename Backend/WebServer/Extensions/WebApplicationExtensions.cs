using WebServer.Configuration;

namespace WebServer.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseWalForcePipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "WalForce Backend API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "WalForce Swagger UI";
        });
        app.UseCors(CorsOptions.PolicyName);
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
