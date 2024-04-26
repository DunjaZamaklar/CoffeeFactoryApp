using App.Contracts;
using App.Data.Database;
using App.Data.Entities;
using Carter;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static App.Features.Login.CreateLogIn;

namespace App.Features.Login;
public static class CreateLogIn
{
    public class Command : IRequest<Result<string, IEnumerable<string>>>
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.UserName).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
        }
    }
    internal sealed class Handler : IRequestHandler<Command, Result<string, IEnumerable<string>>>
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IValidator<Command> _validator;
        private readonly IConfiguration _configuration;
        public Handler(ApplicationDbContext applicationDbContext, IValidator<Command> validator, IConfiguration configuration)
        {
            _applicationDbContext = applicationDbContext;
            _validator = validator;
            _configuration = configuration;
        }
        public async Task<Result<string, IEnumerable<string>>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return Result.Failure<string, IEnumerable<string>>(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var employee = await _applicationDbContext.Employees
                .Where(e => e.Username == request.UserName)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if(employee == null)
            {
                return Result.Failure<string, IEnumerable<string>>(new List<string> {"Invalid Username value!"});
            }
            if(employee.Password != request.Password)
            {
                return Result.Failure<string, IEnumerable<string>>(new List<string> { "Invalid Password value!" });
            }
            var roleName = employee.Role.Name;
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];


            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {

                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, roleName)
                }),
                // the life span of the token needs to be shorter and utilise refresh token to keep the user signedin
                // but since this is a demo app we can extend it to fit our current need
                Expires = DateTime.UtcNow.AddHours(6),
                Audience = audience,
                Issuer = issuer,
                // here we are adding the encryption alogorithim information which will be used to decrypt our token
                SigningCredentials = credentials
                //SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);

            var jwtToken = jwtTokenHandler.WriteToken(token);
            return jwtToken;
   
        }
        
    }
}

public class LoginEndpoint : CarterModule
{
    public LoginEndpoint() : base("/api/login")
    {

    }
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        _ = app.MapPost("", async (LoginRequest request, ISender sender, HttpContext context) =>
        {
            var command = new CreateLogIn.Command
            {
                UserName = request.Username,
                Password = request.Password
            };
           
            var result = await sender.Send(command).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Results.Created($"/api/login/{result.Value}", result.Value);
            }
            else
            {
                return Results.BadRequest(result.Error);
            }
        });
    }

}