namespace OrderingApplication.Features.Identity.Types.Password;

internal readonly record struct UpdatePasswordRequest(
    string OldPassword,
    string NewPassword );