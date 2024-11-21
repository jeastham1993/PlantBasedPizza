using Microsoft.Extensions.Options;
using Momento.Sdk.Auth;
using Momento.Sdk.Config;

namespace PlantBasedPizza.Shared.Caching;

public class MomentoCacheConfiguration : IOptions<MomentoCacheConfiguration>
{
    public string? CacheName { get; set; }
    
    public ICredentialProvider? CredentialProvider { get; set; }
    
    public IConfiguration? Configuration { get; set; }
    
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromSeconds(60);
    
    MomentoCacheConfiguration IOptions<MomentoCacheConfiguration>.Value
    {
        get { return this; }
    }
}