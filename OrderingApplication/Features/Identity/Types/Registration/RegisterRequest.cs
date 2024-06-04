namespace OrderingApplication.Features.Identity.Types.Registration;

internal readonly record struct RegisterRequest(
    string Email,
    string Username,
    string? Phone,
    string Password,
    string PasswordConfirm );