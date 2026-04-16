using System.Text;

namespace WebServer.Configuration;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string Issuer { get; init; } = "WalForce.WebServer";

    public string Audience { get; init; } = "WalForce.Frontend";

    public string SigningKey { get; init; } = "WalForceDevelopmentSigningKeyChangeThisBeforeSharing123!";

    public int AccessTokenExpirationHours { get; init; } = 8;

    public byte[] GetSigningKeyBytes() => Encoding.UTF8.GetBytes(SigningKey);
}
