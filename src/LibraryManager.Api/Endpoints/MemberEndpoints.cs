using LibraryManager.Api.Requests;
using LibraryManager.Domain.Entities;
using LibraryManager.Domain.Interfaces;
using LibraryManager.Domain.ValueObjects;

namespace LibraryManager.Api.Endpoints;

public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/members");

        group.MapPost("/", async (CreateMemberRequest req, IMemberRepository repo, CancellationToken ct) =>
        {
            var member = new Member(req.Name, new Email(req.Email));

            await repo.AddAsync(member, ct);

            return Results.Created($"/members/{member.Id}", new { id = member.Id });
        });

        return app;
    }
}