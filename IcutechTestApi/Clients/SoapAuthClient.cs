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
    private readonly string _soapServiceUrl;

    public SoapAuthClient(HttpClient httpClient, IConfiguration configuration, ILogger<SoapAuthClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _soapServiceUrl = configuration["SoapService:Url"] 
            ?? Environment.GetEnvironmentVariable("SoapService__Url") 
            ?? "http://isapi.mekashron.com/icu-tech/icutech-test.dll";
    }

    public async Task<LoginResult> LoginAsync(string login, string password)
    {
        try
        {
            var soapEnvelope = BuildLoginSoapEnvelope(login, password);
            var response = await _httpClient.PostAsync(_soapServiceUrl, 
                new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"));

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("SOAP Login Response: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new SoapClientException($"SOAP service returned status {response.StatusCode}: {responseContent}");
            }

            return ParseLoginResponse(responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during SOAP login request");
            throw new SoapClientException($"Network error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing SOAP login response");
            throw new SoapClientException($"Error processing SOAP response: {ex.Message}", ex);
        }
    }

    public async Task<RegisterResult> RegisterNewCustomerAsync(RegisterRequest request)
    {
        try
        {
            var soapEnvelope = BuildRegisterSoapEnvelope(request);
            var response = await _httpClient.PostAsync(_soapServiceUrl, 
                new StringContent(soapEnvelope, Encoding.UTF8, "text/xml"));

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("SOAP Register Response: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new SoapClientException($"SOAP service returned status {response.StatusCode}: {responseContent}");
            }

            return ParseRegisterResponse(responseContent);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during SOAP register request");
            throw new SoapClientException($"Network error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing SOAP register response");
            throw new SoapClientException($"Error processing SOAP response: {ex.Message}", ex);
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
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <RegisterNewCustomer xmlns=""http://tempuri.org/"">
      <login>{EscapeXml(request.Login)}</login>
      <password>{EscapeXml(request.Password)}</password>
      <email>{EscapeXml(request.Email ?? "")}</email>
      <firstName>{EscapeXml(request.FirstName ?? "")}</firstName>
      <lastName>{EscapeXml(request.LastName ?? "")}</lastName>
    </RegisterNewCustomer>
  </soap:Body>
</soap:Envelope>";
    }

    private LoginResult ParseLoginResponse(string responseContent)
    {
        if (responseContent.Contains("<html>", StringComparison.OrdinalIgnoreCase) ||
            responseContent.Contains("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SOAP service returned HTML instead of XML. Response: {Response}", responseContent);
            throw new SoapClientException("Внутренняя ошибка сервера при обращении к сервису аутентификации. Сервис вернул неверный формат ответа.");
        }

        try
        {
            var doc = XDocument.Parse(responseContent);
            var ns = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");
            var body = doc.Descendants(ns + "Body").FirstOrDefault();
            
            if (body == null)
            {
                throw new SoapClientException("Неверный формат SOAP ответа");
            }

            var success = !responseContent.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                         !responseContent.Contains("failed", StringComparison.OrdinalIgnoreCase);

            return new LoginResult
            {
                Success = success,
                Message = success ? "Login successful" : "Invalid credentials",
                EntityDetails = success ? new { Message = "Login successful" } : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing XML response: {Response}", responseContent);
            throw new SoapClientException($"Внутренняя ошибка сервера при обращении к сервису аутентификации: {ex.Message}");
        }
    }

    private RegisterResult ParseRegisterResponse(string responseContent)
    {
        if (responseContent.Contains("<html>", StringComparison.OrdinalIgnoreCase) ||
            responseContent.Contains("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SOAP service returned HTML instead of XML. Response: {Response}", responseContent);
            throw new SoapClientException("Внутренняя ошибка сервера при обращении к сервису аутентификации. Сервис вернул неверный формат ответа.");
        }

        try
        {
            var doc = XDocument.Parse(responseContent);
            var ns = XNamespace.Get("http://schemas.xmlsoap.org/soap/envelope/");
            var body = doc.Descendants(ns + "Body").FirstOrDefault();
            
            if (body == null)
            {
                throw new SoapClientException("Неверный формат SOAP ответа");
            }

            var success = !responseContent.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                         !responseContent.Contains("failed", StringComparison.OrdinalIgnoreCase) &&
                         !responseContent.Contains("already exists", StringComparison.OrdinalIgnoreCase);

            return new RegisterResult
            {
                Success = success,
                Message = success ? "Registration successful" : "Registration failed",
                CreatedCustomerId = success ? Guid.NewGuid().ToString() : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing XML response: {Response}", responseContent);
            throw new SoapClientException($"Внутренняя ошибка сервера при обращении к сервису аутентификации: {ex.Message}");
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

