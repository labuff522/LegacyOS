namespace LegacyOS.Api.Features.Registration;

public static class RegistrationEndpoints
{
    public static RouteGroupBuilder MapRegistrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/registration");

        group.MapPost("/family", async (
            FamilyRegistrationRequest request,
            FamilyRegistrationService service) =>
        {
            var response = await service.RegisterFamilyAsync(request);

            return Results.Created($"/families/{response.FamilyId}", response);
        });

        return group;
    }
}