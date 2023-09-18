namespace SearchService.Infrastructure.Options;

using System.Globalization;
using System.Text;

public class RabbitMQConfiguration
{
    public const string Section = "RabbitMQConfiguration";

    public string Host { get; set; }
    public string Password { get; set; }
    public string QueueName { get; set; }
    public string Username { get; set; }
    public string VirtualHost { get; set; }
    public int? Port { get; set; }
    public bool SSL { get; set; }

    public string ToConnectionString()
    {
        if (string.IsNullOrWhiteSpace(this.Host))
        {
            throw new InvalidOperationException("Host should not be empty.");
        }

        var culture = CultureInfo.InvariantCulture;
        var endpoint = new StringBuilder(this.SSL ? "amqps" : "amqp");
        endpoint.Append("://");

        var credentials = $"{this.Username}:{this.Password}".Trim();
        if (credentials.Length > 1)
        {
            endpoint.Append(culture, $"{credentials}@");
        }

        endpoint.Append(this.Host);

        var port = this.Port.HasValue ? this.Port.ToString() : string.Empty;

        if (!string.IsNullOrEmpty(port))
        {
            endpoint.Append(culture, $":{port}");
        }

        var virtualHost = this.VirtualHost ?? "";

        if (!string.IsNullOrEmpty(virtualHost))
        {
            endpoint.Append(culture, $"/{virtualHost}");
        }

        return endpoint.ToString();
    }
}
