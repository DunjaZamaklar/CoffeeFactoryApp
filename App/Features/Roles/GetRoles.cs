using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static App.Features.Roles.GetRoles;

namespace App.Features.Roles;

public static class GetRoles
{
    public class Query : IRequest<List<UserRole>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<UserRole>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<UserRole>> Handle(Query request, CancellationToken cancellationToken)
        {
            var roles = await _applicationDbContext.UserRoles
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return roles;
        }
    }
}

public class GetAllRolesEndpoint : CarterModule
{
    public GetAllRolesEndpoint() : base("/api/roles")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (ISender sender) =>
        {
            var query = new GetRoles.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        }).RequireAuthorization("AdminPolicy");
    }
}