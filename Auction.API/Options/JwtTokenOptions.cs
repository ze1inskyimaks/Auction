namespace Auction.API.Options;

public class JwtTokenOptions
{
    public bool ValidateIssuer { get; set; }
    public bool ValidateAudience { get; set; }
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public string Key { get; set; } = String.Empty;
    public int ExpiresTime { get; set; } = 12;
    public string Word { get; set; } = String.Empty;
}