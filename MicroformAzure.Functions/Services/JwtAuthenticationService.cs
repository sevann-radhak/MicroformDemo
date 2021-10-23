using MicroformAzure.Functions.Interface;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MicroformAzure.Functions.Services
{
    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        private readonly string _key;

        public JwtAuthenticationService(string key)
        {
            _key = key;
        }

        public string Authenticate(string applicationName, string applicationKey)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] tokenKey = Encoding.ASCII.GetBytes(_key);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, applicationName)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            try
            {
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}