using MicroformAzure.Functions.Interface;
using MicroformAzure.Functions.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

[assembly: FunctionsStartup(typeof(MicroformAzure.Functions.Startup))]
namespace MicroformAzure.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContext<MicroformAzureContext>(options =>
            
            options.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnectionString", EnvironmentVariableTarget.Process)));
            
            string key = Environment.GetEnvironmentVariable("AccessCodeTokenKey", EnvironmentVariableTarget.Process);
            builder.Services.AddSingleton<IJwtAuthenticationService>(new JwtAuthenticationService(key));
            builder.Services.AddSingleton<IApplicationLogsService>(new ApplicationLogsService());
            builder.Services.AddAuthentication(a =>
            {
                a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(b =>
            {
                b.RequireHttpsMetadata = false;
                b.SaveToken = true;
                b.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }
    }
}
