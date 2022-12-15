using Auth0.ManagementApi.Models;

namespace Api.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException()
    {
    }

    public UserAlreadyExistsException(User auth0User, Models.User localUser) : base(
        $"Auth0 user with ID {auth0User.UserId} already exists in the local database (user ID {localUser.Id}.")
    {
    }
}