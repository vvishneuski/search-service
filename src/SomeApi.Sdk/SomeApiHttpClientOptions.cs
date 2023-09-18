namespace SomeApi.Sdk;

using System.ComponentModel.DataAnnotations;

public class SomeApiHttpClientOptions
{
    public const string Section = "SomeApiHttpClientOptions";

    [Required]
    public string BaseUrl { get; set; } = default!;

    public string XApiKey { get; set; } = default!;

    public bool AuthDisabled { get; set; }

    public string ClientId { get; set; } = default!;

    public string ClientSecret { get; set; } = default!;

    public string TargetAudience { get; set; } = default!;

    public string TokenEndpointUrl { get; set; } = default!;
}
