namespace Microsoft.Extensions.Configuration;

using Newtonsoft.Json.Linq;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

public class VaultConfigurationProvider : ConfigurationProvider
{
    private readonly VaultOptions config;
    private readonly IVaultClient client;

    public VaultConfigurationProvider(VaultOptions config)
    {
        this.config = config;

        var vaultClientSettings = new VaultClientSettings(
            config.Address,
            new TokenAuthMethodInfo(config.Token)
        )
        {
            BeforeApiRequestAction = (httpClient, httpRequestMessage) =>
            {
                if (!string.IsNullOrWhiteSpace(config.Namespace))
                {
                    httpRequestMessage.Headers.Add("X-Vault-Namespace", config.Namespace);
                }
            }
        };

        this.client = new VaultClient(vaultClientSettings);
    }

    public override void Load() => this.LoadAsync().GetAwaiter().GetResult();

    public async Task LoadAsync()
    {
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


        foreach (var secretItem in this.config.Secrets.Concat(new[] { this.config.Secret }))
        {
            if (string.IsNullOrWhiteSpace(secretItem))
            {
                continue;
            }

            try
            {
                var secretResult = await this.client.V1
                    .Secrets
                    .KeyValue.V2
                    .ReadSecretAsync(secretItem, mountPoint: "kv");

                this.WriteConfiguration(data, null, secretResult.Data.Data);
            }
            catch
            {
                // skip configuration
            }
        }

        this.Data = data;
    }

    /// <summary>
    ///     Populates provided result object with IConfiguration complient config recursively
    /// </summary>
    /// <param name="result"></param>
    /// <param name="prefix"></param>
    /// <param name="source"></param>
    private void WriteConfiguration(
        Dictionary<string, string> result,
        string prefix,
        IDictionary<string, object> source)
    {
        foreach (var item in source)
        {
            var normalizedKey = item.Key?.Replace("__", ":", StringComparison.OrdinalIgnoreCase);
            var key = $"{prefix}{(prefix is null ? string.Empty : ":")}{normalizedKey}";

            if (item.Value is JObject sub)
            {
                this.WriteConfiguration(result, key, sub.ToObject<Dictionary<string, object>>());

                continue;
            }

            if (item.Value is JArray subArray)
            {
                var arrayItems = subArray
                    .Select((item, i) => new { i, item })
                    .ToDictionary(x => $"{key}:{x.i}", y => y.item as object);

                this.WriteConfiguration(result, prefix, arrayItems);

                continue;
            }

            result.Add(key, item.Value.ToString());
        }
    }
}

public class VaultConfigurationSource : IConfigurationSource
{
    private readonly VaultOptions config;

    public VaultConfigurationSource(Action<VaultOptions> config)
    {
        this.config = new VaultOptions();
        config.Invoke(this.config);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new VaultConfigurationProvider(this.config);
}
