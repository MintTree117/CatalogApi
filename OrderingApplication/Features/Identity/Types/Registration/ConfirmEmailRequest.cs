namespace OrderingApplication.Features.Identity.Types.Registration;

internal readonly record struct ConfirmEmailRequest(
    string Email,
    string Code );