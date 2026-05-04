using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;



namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController :ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public ProductsController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _context.Products.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Post(Product product)
    {
        Log.Information("Add Products" +  product.ToString());
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return Ok();
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