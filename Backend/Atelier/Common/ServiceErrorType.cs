namespace Atelier.Common
{
    public enum ServiceErrorType
    {
        None,
        ValidationError,
        AccountLockedOut,
        NotFound,
        Conflict,
        Unauthorized,
        Forbidden,
        InternalServerError
    }
}
