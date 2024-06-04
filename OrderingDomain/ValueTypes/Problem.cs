namespace OrderingDomain.ValueTypes;

public enum Problem
{
    None,
    BadRequest,
    Validation,
    LockedOut,
    Unauthorized,
    Internal,
    Network,
    NotFound,
    Conflict
}