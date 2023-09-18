namespace SomeApi.Sdk;

public class SomeApiHttpClientException : Exception
{
    public SomeApiHttpClientException()
    {
    }

    public SomeApiHttpClientException(string message)
        : base(message)
    {
    }

    public SomeApiHttpClientException(
        string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
