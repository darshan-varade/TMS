using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TMS.DataAccess.DAL;

namespace TMS.WebApp.Infrastructure
{
    public static class JwtHelper
    {
        private static readonly string Secret = ConfigurationManager.AppSettings["JwtSecret"];
        private static readonly int AccessTokenExpiryMinutes = int.Parse(ConfigurationManager.AppSettings["JwtAccessTokenExpiryMinutes"] ?? "15");
        private static readonly int RefreshTokenExpiryDays = int.Parse(ConfigurationManager.AppSettings["JwtRefreshTokenExpiryDays"] ?? "2");

        private static SymmetricSecurityKey GetSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        }

        public static string GenerateAccessToken(int userId, string fullName, string email, string roleName, string mobile, string department)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetSecurityKey();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, fullName ?? ""),
                new Claim(ClaimTypes.Email, email ?? ""),
                new Claim(ClaimTypes.Role, roleName ?? "Employee"),
                new Claim("Mobile", mobile ?? ""),
                new Claim("Department", department ?? "")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(AccessTokenExpiryMinutes),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string HashRefreshToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                var sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static ClaimsPrincipal ValidateAccessToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = GetSecurityKey();
            SecurityToken validatedToken = null;

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public static RefreshTokenResult RefreshAccessToken(string refreshTokenCookie, AuthDAL dal)
        {
            if (string.IsNullOrEmpty(refreshTokenCookie))
                return null;

            string hash = HashRefreshToken(refreshTokenCookie);
            var record = dal.GetRefreshTokenByHash(hash);

            if (record == null)
                return null;

            string newAccessToken = GenerateAccessToken(record.UserId, record.FullName, record.EmailId, record.RoleName, record.MobileNumber, "");
            string newRefreshToken = GenerateRefreshToken();
            string newRefreshHash = HashRefreshToken(newRefreshToken);

            DateTime newExpiry = DateTime.Now.AddDays(RefreshTokenExpiryDays);
            dal.RotateRefreshToken(record.RefreshTokenId, newRefreshHash, newExpiry, record.UserId);

            var principal = ValidateAccessToken(newAccessToken);

            return new RefreshTokenResult
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Principal = principal,
                RefreshExpiry = newExpiry
            };
        }

        public static DateTime GetAccessTokenExpiry(bool rememberMe)
        {
            return rememberMe
                ? DateTime.Now.AddDays(RefreshTokenExpiryDays)
                : DateTime.Now.AddMinutes(AccessTokenExpiryMinutes);
        }
    }

    public class RefreshTokenResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public ClaimsPrincipal Principal { get; set; }
        public DateTime RefreshExpiry { get; set; }
    }
}
