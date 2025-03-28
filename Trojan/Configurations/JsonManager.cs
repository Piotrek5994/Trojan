using Core.Models.Settings;
using Microsoft.Extensions.Configuration;

namespace Trojan.Configurations;

public class JsonManager
{
    public static IConfigurationRoot _conf;

    static JsonManager()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
        _conf = builder.Build();
    }

    public static OptimaSettings GetSqlSettings()
    {
        return _conf.GetSection("OptimaSettings").Get<OptimaSettings>() ?? new OptimaSettings();
    }
}
