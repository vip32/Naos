namespace Naos.Core.Messaging.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;

    public class ServiceUtils // TODO: move to Authentication.Utility?
    {
        private const string EndpointProperty = "endpoint";
        private const string AccessKeyProperty = "accesskey";
        private static readonly char[] PropertySeparator = { ';' };
        private static readonly char[] KeyValueSeparator = { '=' };
        private static readonly JwtSecurityTokenHandler JwtTokenHandler = new JwtSecurityTokenHandler();

        public ServiceUtils(string connectionString)
        {
            (this.Endpoint, this.AccessKey) = ParseConnectionString(connectionString);
        }

        public string Endpoint { get; }

        public string AccessKey { get; }

        public string GenerateAccessToken(string audience, string userId, TimeSpan? lifetime = null)
        {
            IEnumerable<Claim> claims = null;
            if (userId != null)
            {
                claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                };
            }

            return this.GenerateAccessTokenInternal(audience, claims, lifetime ?? TimeSpan.FromDays(365));
        }

        public string GenerateAccessTokenInternal(string audience, IEnumerable<Claim> claims, TimeSpan lifetime)
        {
            var expire = DateTime.UtcNow.Add(lifetime);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.AccessKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = JwtTokenHandler.CreateJwtSecurityToken(
                issuer: null,
                audience: audience,
                subject: claims == null ? null : new ClaimsIdentity(claims),
                expires: expire,
                signingCredentials: credentials);
            return JwtTokenHandler.WriteToken(token);
        }

        internal static (string, string) ParseConnectionString(string connectionString)
        {
            var properties = connectionString.Split(PropertySeparator, StringSplitOptions.RemoveEmptyEntries);
            if (properties.Length > 1)
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var property in properties)
                {
                    var kvp = property.Split(KeyValueSeparator, 2);
                    if (kvp.Length != 2)
                    {
                        continue;
                    }

                    var key = kvp[0].Trim();
                    if (dict.ContainsKey(key))
                    {
                        throw new ArgumentException($"Duplicate properties found in connection string: {key}.");
                    }

                    dict.Add(key, kvp[1].Trim());
                }

                if (dict.ContainsKey(EndpointProperty) && dict.ContainsKey(AccessKeyProperty))
                {
                    return (dict[EndpointProperty].TrimEnd('/'), dict[AccessKeyProperty]);
                }
            }

            throw new ArgumentException($"Connection string missing required properties {EndpointProperty} and {AccessKeyProperty}.");
        }
    }
}
