using Api.Data;
using Api.Exceptions;
using Api.Models;
using Auth0.ManagementApi;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;

namespace Api.Services;

public class UserService
{
    private readonly IConfiguration _configuration;
    private readonly KwekkerContext _context;
    private readonly RestClient _client;
    
    private string? _token;

    public UserService(KwekkerContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _client = new RestClient(configuration["Auth0:ManagementApi:TokenUrl"]);
    }

    public async Task<User?> GetUserFromUsername(string username)
    {
        return await _context.Users
            .Where(u => u.Username == username)
            .FirstOrDefaultAsync();
    }

    public async Task<User> LoadAndCreateFromAuth0(string userId)
    {
        var userAuth0 = await GetAuth0User(userId);
        
        if (userAuth0 == null)
        {
            throw new Auth0UserNotFoundException(userId);
        }
        
        var localUser = await _context.Users.FirstOrDefaultAsync(u => u.ProviderId == userAuth0.UserId);
        
        if (localUser != null)
        {
            throw new UserAlreadyExistsException(userAuth0, localUser);
        }

        return await CreateFromAuth0User(userAuth0);
    }

    private async Task<string> GetAuth0Token()
    {
        if (!string.IsNullOrEmpty(_token))
        {
            return _token;
        }
        
        var request = new RestRequest();
        
        request.AddJsonBody(new
        {
            client_id = _configuration["Auth0:ManagementApi:ClientId"],
            client_secret = _configuration["Auth0:ManagementApi:ClientSecret"],
            audience = _configuration["Auth0:ManagementApi:Audience"],
            grant_type = "client_credentials"
        });
        
        var response = await _client.ExecutePostAsync(request);
        
        return _token = JsonConvert.DeserializeObject<dynamic>(response.Content!)!.access_token.ToString();
    }
    
    private async Task<Auth0.ManagementApi.Models.User?> GetAuth0User(string userId)
    {
        var apiClient = new ManagementApiClient(await GetAuth0Token(), "kwekker.eu.auth0.com");
        return await apiClient.Users.GetAsync(userId);
    }

    private async Task<User> CreateFromAuth0User(Auth0.ManagementApi.Models.User user)
    {
        var userToCreate = new User
        {
            ProviderId = user.UserId,
            Username = user.UserName,
            DisplayName = user.FullName,
            Email = user.Email,
            AvatarUrl = user.Picture
        };

        _context.Users.Add(userToCreate);
        await _context.SaveChangesAsync();

        return userToCreate;
    }
}