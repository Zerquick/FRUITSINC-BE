using Api.Models;

namespace ApiTest;

public class TestUser : User
{
    public TestUser()
    {
        ProviderId = "123456789";
        Username = "Foo";
        Email = "foo@bar.com";
        DisplayName = "Foo Bar";
        AvatarUrl = "https://foo.com/avatar.png";
    }
}