namespace IcutechTestApi.Clients;

public class SoapClientException : Exception
{
    public SoapClientException(string message) : base(message)
    {
    }

    public SoapClientException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

