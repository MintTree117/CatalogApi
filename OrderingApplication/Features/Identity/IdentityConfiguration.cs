using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OrderingApplication.Features.Identity.Services.Account;
using OrderingApplication.Features.Identity.Services.Authentication;
using OrderingDomain.Identity;
using OrderingInfrastructure.Features.Identity;

namespace OrderingApplication.Features.Identity;

internal static class IdentityConfiguration
{
    internal static void ConfigureIdentity( this WebApplicationBuilder builder )
    {
        builder.Services
               .AddIdentityCore<UserAccount>( options => GetIdentityOptions( options, builder.Configuration ) )
               .AddEntityFrameworkStores<IdentityDbContext>()
               .AddDefaultTokenProviders();
        builder.Services
               .AddAuthentication( GetAuthenticationOptions )
               .AddJwtBearer( options => GetJwtOptions( options, builder ) );
        builder.Services
               .AddAuthorization();
        builder.Services.AddSingleton<IdentityConfigCache>();
        builder.Services.AddSingleton<RevokedTokenBroadcaster>();
        builder.Services.AddSingleton<RevokedTokenCache>();
        builder.Services.AddScoped<LoginSystem>();
        builder.Services.AddScoped<LoginRefreshSystem>();
        builder.Services.AddScoped<LogoutSystem>();
        builder.Services.AddScoped<RegistrationSystem>();
        builder.Services.AddScoped<ProfileSystem>();
        builder.Services.AddScoped<PasswordSystem>();
        builder.Services.AddScoped<AddressSystem>();
    }
    internal static void AddIdentityMiddleware( this WebApplication app )
    {
        app.UseMiddleware<RevokedTokenMiddleware>();
    }

    static void GetIdentityOptions( IdentityOptions options, IConfiguration configuration )
    {
        options.Stores.ProtectPersonalData = configuration.GetSection( "Identity:User:ProtectPersonalData" ).Get<bool>();
        
        options.User.RequireUniqueEmail = configuration.GetSection( "Identity:User:RequireConfirmedEmail" ).Get<bool>();
        options.User.AllowedUserNameCharacters = configuration.GetSection( "Identity:User:AllowedUserNameCharacters" ).Get<string>() ?? "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
        
        options.SignIn.RequireConfirmedEmail = configuration.GetSection( "Identity:SignIn:RequireConfirmedEmail" ).Get<bool>();
        options.SignIn.RequireConfirmedAccount = configuration.GetSection( "Identity:SignIn:RequireConfirmedEmail" ).Get<bool>();
        options.SignIn.RequireConfirmedPhoneNumber = configuration.GetSection( "Identity:SignIn:RequireConfirmedEmail" ).Get<bool>();

        IConfigurationSection passwordSection = configuration.GetSection( "Identity:Validation:PasswordCriteria:" );
        options.Password.RequiredLength = passwordSection.GetSection( "MinLength" ).Get<int>();
        options.Password.RequireLowercase = passwordSection.GetSection( "RequireUppercase" ).Get<bool>();
        options.Password.RequireUppercase = passwordSection.GetSection( "RequireLowercase" ).Get<bool>();
        options.Password.RequireDigit = passwordSection.GetSection( "RequireDigit" ).Get<bool>();
        options.Password.RequireNonAlphanumeric = passwordSection.GetSection( "RequireSpecial" ).Get<bool>();

        IConfigurationSection lockoutSection = configuration.GetSection( "Identity:Lockout" );
        options.Lockout.DefaultLockoutTimeSpan = lockoutSection.GetSection( "DefaultLockoutTimeSpan" ).Get<TimeSpan>();
        options.Lockout.MaxFailedAccessAttempts = lockoutSection.GetSection( "MaxFailedAccessAttempts" ).Get<int>();
        options.Lockout.AllowedForNewUsers = lockoutSection.GetSection( "AllowedForNewUsers" ).Get<bool>();
    }
    static void GetAuthenticationOptions( AuthenticationOptions options )
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
    static void GetJwtOptions( JwtBearerOptions options, WebApplicationBuilder builder )
    {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Identity:Jwt:Issuer"],
            ValidAudience = builder.Configuration["Identity:Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( builder.Configuration["Identity:Jwt:Key"] ?? throw new Exception( "Fatal: Failed to get Jwt key from config during startup." ) ) )
        };

        options.SaveToken = true;
    }
}