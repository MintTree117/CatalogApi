namespace OrderingApplication.Features.Identity.Types.Password;

internal sealed class PasswordConfig
{
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireDigit { get; set; }
    public bool RequireSpecial { get; set; }
    public string Specials { get; set; } = string.Empty;
}