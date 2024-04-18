using App.Contracts;
using App.Data.Database;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static App.Features.Suppliers.GetSupplier;

namespace App.Features.Suppliers;

public static class GetSupplier
{
    public class Query : IRequest<SupplierResponse>
    {
        public Guid Id { get; set; }
    }

    internal sealed class Handler : IRequestHandler<Query, SupplierResponse>
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public Handler(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<SupplierResponse> Handle(Query request, CancellationToken cancellationToken)
        {
            var supplierResponse = await _applicationDbContext.Suppliers
                .Where(p => p.Id == request.Id)
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
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (supplierResponse is null)
            {
                return null;
            }
            return supplierResponse;
        }
    }
}

public class GetSupplierEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("supplier/{id}", async (Guid id, ISender sender) =>
        {
            var query = new GetSupplier.Query { Id = id };
            var result = await sender.Send(query).ConfigureAwait(false);
            if (result is null)
            {
                return null;
            }
            return result;
        });
    }
}