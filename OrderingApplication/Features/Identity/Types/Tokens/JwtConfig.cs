using Microsoft.IdentityModel.Tokens;

namespace OrderingApplication.Features.Identity.Types.Tokens;

internal sealed class JwtConfig
{
    public SymmetricSecurityKey Key { get; set; } = null!;
    public string Audience { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public bool ValidateAudience { get; set; } = false;
    public bool ValidateIssuer { get; set; } = false;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public TimeSpan AccessLifetime { get; set; }
    public TimeSpan RefreshLifetime { get; set; }
}