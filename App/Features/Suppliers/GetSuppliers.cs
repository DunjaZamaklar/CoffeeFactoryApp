using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.Suppliers.GetSuppliers;

namespace App.Features.Suppliers;

public static class GetSuppliers
{
    public class Query : IRequest<List<SupplierResponse>>
    {
    }

    internal sealed class Handler : IRequestHandler<Query, List<SupplierResponse>>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<List<SupplierResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplierResponse = await _applicationDbContext.Suppliers
                .Select(p => new SupplierResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Address = p.Address,
                    City = p.City,
                    Country = p.Country,
                    PhoneNumber = p.PhoneNumber,
                    Email = p.Email
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return supplierResponse;
        }
    }
}

public class GetAllSuppliersEndpoint : CarterModule
{
    public GetAllSuppliersEndpoint() : base("/api/supplier")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("", async (ISender sender) =>
        {
            var query = new GetSuppliers.Query { };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}