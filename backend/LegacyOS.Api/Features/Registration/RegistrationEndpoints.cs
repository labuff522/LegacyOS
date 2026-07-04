using LegacyOS.Api.Data;

namespace LegacyOS.Api.Features.Registration;

public static class RegistrationEndpoints
{
    public static RouteGroupBuilder MapRegistrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/registration");

        group.MapPost("/family", async (
            FamilyRegistrationRequest request,
            LegacyOSDbContext db) =>
        {
            // For now we're just proving the request reaches the API.
            // We'll save everything in the next step.

            return Results.Ok(new
            {
                message = "Registration request received.",
                family = request
            });
        });

        return group;
    }
}