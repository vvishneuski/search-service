namespace Microsoft.Extensions.Configuration;

public class VaultOptions
{
    public string Address { get; set; }

    public string Namespace { get; set; }

    public string Token { get; set; }

    public IList<string> Secrets { get; set; } = new List<string>();
    public string Secret { get; set; }
}
