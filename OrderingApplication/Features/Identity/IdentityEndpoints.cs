using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Features.Identity.Services.Account;
using OrderingApplication.Features.Identity.Services.Authentication;
using OrderingApplication.Features.Identity.Types.Accounts;
using OrderingApplication.Features.Identity.Types.Addresses;
using OrderingApplication.Features.Identity.Types.Login;
using OrderingApplication.Features.Identity.Types.Password;
using OrderingApplication.Features.Identity.Types.Registration;
using OrderingApplication.Extentions;
using LoginRequest = OrderingApplication.Features.Identity.Types.Login.LoginRequest;
using RegisterRequest = OrderingApplication.Features.Identity.Types.Registration.RegisterRequest;
using TwoFactorRequest = OrderingApplication.Features.Identity.Types.Login.TwoFactorRequest;

namespace OrderingApplication.Features.Identity;

using LoginRequest = Types.Login.LoginRequest;
using RegisterRequest = Types.Registration.RegisterRequest;
using TwoFactorRequest = Types.Login.TwoFactorRequest;

internal static class IdentityEndpoints
{
    internal static void MapIdentityEndpoints( this IEndpointRouteBuilder app )
    {
        // Authentication
        app.MapGet( "api/identity/check",
            async ( [FromQuery] string accessToken, LoginRefreshSystem system ) =>
            (await system.CheckLogin( accessToken ))
            .GetIResult() );
        app.MapPost( "api/identity/login",
            async ( [FromBody] LoginRequest request, LoginSystem system ) =>
            (await system.Login( request ))
            .GetIResult() );
        app.MapPost( "api/identity/2fa",
            async ( [FromBody] TwoFactorRequest request, LoginSystem system ) =>
            (await system.Login2Factor( request ))
            .GetIResult() );
        app.MapPost( "api/identity/refresh",
            async ( [FromBody] LoginRefreshRequest request, LoginRefreshSystem system ) =>
            (await system.LoginRefresh( request ))
            .GetIResult() );
        app.MapPost( "api/identity/refresh-full",
            async ( [FromBody] LoginRefreshRequest request, LoginRefreshSystem system ) =>
            (await system.LoginRefreshFull( request ))
            .GetIResult() );
        app.MapPost( "api/identity/logout",
            async ( [FromBody] string refreshToken, HttpContext http, LogoutSystem system ) =>
            (await system.Logout( http.User, refreshToken ))
            .GetIResult() ).RequireAuthorization();

        // Register
        app.MapPost( "api/identity/register",
            async ( [FromBody] RegisterRequest request, RegistrationSystem system ) =>
            (await system.RegisterIdentity( request ))
            .GetIResult() );
        app.MapPut( "api/identity/email/confirm",
            async ( [FromBody] ConfirmEmailRequest request, RegistrationSystem system ) =>
            (await system.ConfirmEmail( request ))
            .GetIResult() );
        app.MapPost( "api/identity/email/resend",
            async ( [FromBody] ConfirmResendRequest request, RegistrationSystem system ) =>
            (await system.ResendConfirmLink( request ))
            .GetIResult() );
        
        // Manage Account
        app.MapGet( "api/identity/account/view",
            async ( HttpContext http, ProfileSystem system ) =>
            (await system.ViewIdentity( http.UserId() ))
            .GetIResult() ).RequireAuthorization();
        app.MapPost( "api/identity/account/update",
            async ( [FromBody] UpdateAccountRequest request, HttpContext http, ProfileSystem system ) =>
            (await system.UpdateAccount( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();
        app.MapPost( "api/identity/account/delete",
            async ( [FromBody] DeleteAccountRequest request, HttpContext http, ProfileSystem system ) =>
            (await system.DeleteIdentity( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();

        // Password
        app.MapPost( "api/identity/password/update",
            async ( [FromBody] UpdatePasswordRequest request, HttpContext http, PasswordSystem system ) =>
            (await system.ManagePassword( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();
        app.MapPost( "api/identity/password/forgot",
            async ( [FromBody] ForgotPasswordRequest request, PasswordSystem system ) =>
                (await system.ForgotPassword( request ))
                .GetIResult() );
        app.MapPost( "api/identity/password/reset",
            async ( [FromBody] ResetPasswordRequest request, PasswordSystem system ) =>
            (await system.ResetPassword( request ))
            .GetIResult() );

        // Manage Addresses
        app.MapGet( "api/identity/address/view",
            async ( [FromQuery] int page, [FromQuery] int rows, HttpContext http, AddressSystem system ) =>
            (await system.ViewAddresses( http.UserId(), new ViewAddressesRequest( page, rows ) ))
            .GetIResult() ).RequireAuthorization();
        app.MapPut( "api/identity/address/add",
            async ( [FromBody] AddressDto request, HttpContext http, AddressSystem system ) =>
            (await system.AddAddress( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();
        app.MapPost( "api/identity/address/update",
            async ( [FromBody] AddressDto request, HttpContext http, AddressSystem system ) =>
            (await system.UpdateAddress( http.UserId(), request ))
            .GetIResult() ).RequireAuthorization();
        app.MapDelete( "api/identity/address/delete",
            async ( [FromQuery] Guid addressId, HttpContext http, AddressSystem system ) =>
            (await system.DeleteAddress( http.UserId(), addressId ))
            .GetIResult() ).RequireAuthorization();
    }
}