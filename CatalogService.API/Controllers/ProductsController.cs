using CatalogService.Application.Commands;
using CatalogService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Serilog;


namespace CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        private readonly IConfiguration _configuration;

        public ProductsController(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _mediator.Send(new GetProductsQuery());
            if (products == null) return NotFound();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetProducts), new { id }, command);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductCommand command)
        {
            if (id != command.Id) return BadRequest();
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _mediator.Send(new DeleteProductCommand(id));
            return NoContent();
        }

        [HttpPost("GenerateAPIValidationToken")]
        [Produces("application/json")]
        public ActionResult GenerateAPIValidationToken()
        {
            try
            {
                Log.Information("GenerateAPIValidationToken Begins");
                var JWTConfigs = _configuration.GetSection("JWT");
                var Claims = new[]
                {
                    new Claim(ClaimTypes.Name, "Bala"),
                    new Claim(ClaimTypes.Role , "Admin")
                };

                var APIKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTConfigs["SecretKey"] ?? "2388-siewi2-23edj-3988"));

                var SigningCredentialsKey = new SigningCredentials(APIKey, SecurityAlgorithms.HmacSha256);

                var JwtToken = new JwtSecurityToken(
                    issuer: JWTConfigs["Issuer"],
                    audience: JWTConfigs["Audience"],
                    claims: Claims,
                    expires: DateTime.Now.AddMinutes(5),
                    signingCredentials: SigningCredentialsKey

                );

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(JwtToken) });
            }
            catch (Exception ex)
            {
                Log.Error("Exception Occured" + ex);
                return null;

            }
            finally
            {
                Log.Information("GenerateAPIValidationToken Ends");
            }
        }
    }
}
