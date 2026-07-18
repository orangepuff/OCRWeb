using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace OCRWeb.Bff.Infrastructure.InternalToken;

public class InternalTokenIssuer : IInternalTokenIssuer
{
    private const string Issuer = "ocrweb-bff";
    private const string Audience = "ocrweb-api";

    private readonly SigningCredentials _signingCredentials;

    public InternalTokenIssuer(IConfiguration configuration)
    {
        var privateKeyBase64 = configuration["Authentication:InternalToken:PrivateKey"]
            ?? throw new InvalidOperationException("Authentication:InternalToken:PrivateKey is not configured.");

        var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(privateKeyBase64), out _);

        _signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
    }

    public string MintToken(string? subject = null)
    {
        var claims = new List<Claim>();
        if (subject is not null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));
        }

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(30),
            signingCredentials: _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
