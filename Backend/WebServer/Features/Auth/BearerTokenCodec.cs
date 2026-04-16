using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebServer.Configuration;
using WebServer.Domain.Models;

namespace WebServer.Features.Auth;

internal static class BearerTokenCodec
{
    public static string CreateToken(UserRecord user, AuthOptions options)
    {
        var header = Base64UrlEncode(Encoding.UTF8.GetBytes("""{"alg":"HS256","typ":"JWT"}"""));
        var payload = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new TokenPayload
        {
            Subject = user.Id.ToString(),
            Name = $"{user.FirstName} {user.LastName}",
            Email = user.Email,
            Role = user.Role,
            Issuer = options.Issuer,
            Audience = options.Audience,
            ExpiresAtUnixSeconds = DateTimeOffset.UtcNow.AddHours(options.AccessTokenExpirationHours).ToUnixTimeSeconds()
        })));

        var signingInput = $"{header}.{payload}";
        var signature = Base64UrlEncode(Sign(signingInput, options.GetSigningKeyBytes()));

        return $"{signingInput}.{signature}";
    }

    public static bool TryReadToken(string token, AuthOptions options, out TokenPayload? payload)
    {
        payload = null;

        var segments = token.Split('.');
        if (segments.Length != 3)
        {
            return false;
        }

        var signingInput = $"{segments[0]}.{segments[1]}";
        var expectedSignature = Sign(signingInput, options.GetSigningKeyBytes());

        byte[] providedSignature;
        try
        {
            providedSignature = Base64UrlDecode(segments[2]);
        }
        catch
        {
            return false;
        }

        if (!CryptographicOperations.FixedTimeEquals(expectedSignature, providedSignature))
        {
            return false;
        }

        try
        {
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(segments[1]));
            payload = JsonSerializer.Deserialize<TokenPayload>(payloadJson);
        }
        catch
        {
            return false;
        }

        if (payload is null ||
            string.IsNullOrWhiteSpace(payload.Subject) ||
            !string.Equals(payload.Issuer, options.Issuer, StringComparison.Ordinal) ||
            !string.Equals(payload.Audience, options.Audience, StringComparison.Ordinal) ||
            payload.ExpiresAtUnixSeconds <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            payload = null;
            return false;
        }

        return true;
    }

    private static byte[] Sign(string signingInput, byte[] key)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value
            .Replace('-', '+')
            .Replace('_', '/');

        var padding = 4 - (base64.Length % 4);
        if (padding is > 0 and < 4)
        {
            base64 = base64.PadRight(base64.Length + padding, '=');
        }

        return Convert.FromBase64String(base64);
    }

    internal sealed class TokenPayload
    {
        [JsonPropertyName("sub")]
        public string Subject { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; init; } = string.Empty;

        [JsonPropertyName("iss")]
        public string Issuer { get; init; } = string.Empty;

        [JsonPropertyName("aud")]
        public string Audience { get; init; } = string.Empty;

        [JsonPropertyName("exp")]
        public long ExpiresAtUnixSeconds { get; init; }
    }
}
