using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.SupplyCategories.GetSupplyCategory;

namespace App.Features.SupplyCategories;

public static class GetRoleByName
{
    public class Query : IRequest<UserRole>
    {
        public string Name { get; set; } = string.Empty;
    }

    internal sealed class Handler : IRequestHandler<Query, UserRole>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<UserRole> Handle(Query request, CancellationToken cancellationToken)
        {
            var role = await _applicationDbContext.UserRoles
                .Where(p => p.Name == request.Name)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (role is null)
            {
                return null;
            }
            return role;
        }
    }
}

public class GetSupplyRoleByNameEndpoint : CarterModule
{
    public GetSupplyRoleByNameEndpoint() : base("/api/roles")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{Name}", async (string Name, ISender sender) =>
        {
            var query = new GetRoleByName.Query { Name = Name };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        }).RequireAuthorization("AdminPolicy");
    }
}