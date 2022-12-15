namespace Api.Exceptions;

public class Auth0UserNotFoundException : Exception
{
    public Auth0UserNotFoundException()
    {
    }

    public Auth0UserNotFoundException(string userId) : base(userId)
    {
    }
}