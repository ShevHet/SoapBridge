using System.Text;
using System.Xml.Linq;
using IcutechTestApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IcutechTestApi.Clients;

public class SoapAuthClient : ISoapAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SoapAuthClient> _logger;
    private readonly string _serviceUrl;

    public SoapAuthClient(HttpClient httpClient, IConfiguration configuration, ILogger<SoapAuthClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceUrl = configuration["SoapService:Url"] 
            ?? throw new InvalidOperationException("SoapService:Url не настроен в appsettings.json");
        
        _httpClient.BaseAddress = new Uri(_serviceUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<LoginResult> LoginAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentException("Логин не может быть пустым", nameof(login));
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Пароль не может быть пустым", nameof(password));

        try
        {
            _logger.LogInformation("Выполняется SOAP-запрос Login для пользователя: {Login}", login);

            var soapEnvelope = BuildLoginSoapEnvelope(login, password);
            var response = await SendSoapRequestAsync(soapEnvelope, "Login");
            
            return ParseLoginResponse(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка сети при вызове SOAP-сервиса Login");
            throw new SoapClientException("Ошибка сети при обращении к SOAP-сервису. Проверьте подключение к интернету и доступность сервиса.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Тайм-аут при вызове SOAP-сервиса Login");
            throw new SoapClientException("Превышено время ожидания ответа от SOAP-сервиса. Сервис может быть недоступен или перегружен.", ex);
        }
        catch (SoapClientException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при вызове SOAP-сервиса Login");
            throw new SoapClientException("Произошла неожиданная ошибка при обращении к SOAP-сервису.", ex);
        }
    }

    public async Task<RegisterResult> RegisterNewCustomerAsync(RegisterRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        
        if (string.IsNullOrWhiteSpace(request.Login))
            throw new ArgumentException("Логин не может быть пустым", nameof(request));
        
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Пароль не может быть пустым", nameof(request));

        try
        {
            _logger.LogInformation("Выполняется SOAP-запрос RegisterNewCustomer для пользователя: {Login}", request.Login);

            var soapEnvelope = BuildRegisterSoapEnvelope(request);
            var response = await SendSoapRequestAsync(soapEnvelope, "RegisterNewCustomer");
            
            return ParseRegisterResponse(response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка сети при вызове SOAP-сервиса RegisterNewCustomer");
            throw new SoapClientException("Ошибка сети при обращении к SOAP-сервису. Проверьте подключение к интернету и доступность сервиса.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Тайм-аут при вызове SOAP-сервиса RegisterNewCustomer");
            throw new SoapClientException("Превышено время ожидания ответа от SOAP-сервиса. Сервис может быть недоступен или перегружен.", ex);
        }
        catch (SoapClientException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при вызове SOAP-сервиса RegisterNewCustomer");
            throw new SoapClientException("Произошла неожиданная ошибка при обращении к SOAP-сервису.", ex);
        }
    }

    private string BuildLoginSoapEnvelope(string login, string password)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <Login xmlns=""http://tempuri.org/"">
            <login>{EscapeXml(login)}</login>
            <password>{EscapeXml(password)}</password>
        </Login>
    </soap:Body>
</soap:Envelope>";
    }

    private string BuildRegisterSoapEnvelope(RegisterRequest request)
    {
        var email = string.IsNullOrWhiteSpace(request.Email) ? string.Empty : $"<email>{EscapeXml(request.Email)}</email>";
        var firstName = string.IsNullOrWhiteSpace(request.FirstName) ? string.Empty : $"<firstName>{EscapeXml(request.FirstName)}</firstName>";
        var lastName = string.IsNullOrWhiteSpace(request.LastName) ? string.Empty : $"<lastName>{EscapeXml(request.LastName)}</lastName>";

        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Body>
        <RegisterNewCustomer xmlns=""http://tempuri.org/"">
            <login>{EscapeXml(request.Login)}</login>
            <password>{EscapeXml(request.Password)}</password>
            {email}
            {firstName}
            {lastName}
        </RegisterNewCustomer>
    </soap:Body>
</soap:Envelope>";
    }

    private async Task<string> SendSoapRequestAsync(string soapEnvelope, string operationName)
    {
        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", $"http://tempuri.org/{operationName}");

        var response = await _httpClient.PostAsync(string.Empty, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("SOAP-сервис вернул ошибку. StatusCode: {StatusCode}, Content: {Content}", 
                response.StatusCode, errorContent);
            throw new SoapClientException($"SOAP-сервис вернул ошибку: {response.StatusCode}. {errorContent}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    private LoginResult ParseLoginResponse(string soapResponse)
    {
        try
        {
            var doc = XDocument.Parse(soapResponse);
            var ns = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");
            var tempNs = XNamespace.Get("http://tempuri.org/");
            
            var body = doc.Descendants(ns + "Body").FirstOrDefault();
            var loginResponse = body?.Descendants(tempNs + "LoginResponse").FirstOrDefault();
            var loginResult = loginResponse?.Descendants(tempNs + "LoginResult").FirstOrDefault();

            if (loginResult == null)
            {
                _logger.LogWarning("Не удалось распарсить ответ SOAP-сервиса. Response: {Response}", soapResponse);
                return new LoginResult
                {
                    Success = false,
                    Message = "Не удалось обработать ответ от SOAP-сервиса"
                };
            }

            var successElement = loginResult.Element(tempNs + "Success");
            var messageElement = loginResult.Element(tempNs + "Message");
            var entityDetailsElement = loginResult.Element(tempNs + "EntityDetails");

            object? entityDetails = null;
            if (entityDetailsElement != null)
            {
                // Если есть текстовое значение, используем его, иначе весь элемент
                entityDetails = !string.IsNullOrEmpty(entityDetailsElement.Value) 
                    ? entityDetailsElement.Value 
                    : entityDetailsElement;
            }

            return new LoginResult
            {
                Success = bool.TryParse(successElement?.Value, out var success) && success,
                Message = messageElement?.Value ?? string.Empty,
                EntityDetails = entityDetails
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка парсинга ответа SOAP-сервиса. Response: {Response}", soapResponse);
            throw new SoapClientException("Ошибка при обработке ответа от SOAP-сервиса. Ответ имеет неверный формат.", ex);
        }
    }

    private RegisterResult ParseRegisterResponse(string soapResponse)
    {
        try
        {
            var doc = XDocument.Parse(soapResponse);
            var ns = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");
            var tempNs = XNamespace.Get("http://tempuri.org/");
            
            var body = doc.Descendants(ns + "Body").FirstOrDefault();
            var registerResponse = body?.Descendants(tempNs + "RegisterNewCustomerResponse").FirstOrDefault();
            var registerResult = registerResponse?.Descendants(tempNs + "RegisterNewCustomerResult").FirstOrDefault();

            if (registerResult == null)
            {
                _logger.LogWarning("Не удалось распарсить ответ SOAP-сервиса. Response: {Response}", soapResponse);
                return new RegisterResult
                {
                    Success = false,
                    Message = "Не удалось обработать ответ от SOAP-сервиса"
                };
            }

            var successElement = registerResult.Element(tempNs + "Success");
            var messageElement = registerResult.Element(tempNs + "Message");
            var createdCustomerIdElement = registerResult.Element(tempNs + "CreatedCustomerId");

            return new RegisterResult
            {
                Success = bool.TryParse(successElement?.Value, out var success) && success,
                Message = messageElement?.Value ?? string.Empty,
                CreatedCustomerId = createdCustomerIdElement?.Value
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка парсинга ответа SOAP-сервиса. Response: {Response}", soapResponse);
            throw new SoapClientException("Ошибка при обработке ответа от SOAP-сервиса. Ответ имеет неверный формат.", ex);
        }
    }

    private static string EscapeXml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}

