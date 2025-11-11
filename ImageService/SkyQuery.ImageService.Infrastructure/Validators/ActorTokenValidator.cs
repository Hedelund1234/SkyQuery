using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SkyQuery.ImageService.Application.Interfaces;
using SkyQuery.ImageService.Domain.Entities.Validation;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Infrastructure.Validators
{
    public sealed class ActorTokenValidator : IActorTokenValidator // sealed = kan ikke nedarves
    {
        private readonly ActorTokenOptions _opts;
        private readonly JwtSecurityTokenHandler _handler = new();

        public ActorTokenValidator(IOptions<ActorTokenOptions> options)
        {
            _opts = options.Value;
        }

        public ClaimsPrincipal? Validate(string jwt)
        {
            if (string.IsNullOrWhiteSpace(jwt))
            {
                return null;
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _opts.Issuer,
                ValidateAudience = true,
                ValidAudience = _opts.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            try
            {
                return _handler.ValidateToken(jwt, parameters, out _);
            }
            catch
            {
                return null; // invalid signature, expired, wrong aud/iss, etc.
            }
        }
    }
}
