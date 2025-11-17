using IcutechTestApi.Models;

namespace IcutechTestApi.Clients;

public interface ISoapAuthClient
{
    Task<LoginResult> LoginAsync(string login, string password);
    Task<RegisterResult> RegisterNewCustomerAsync(RegisterRequest request);
}

