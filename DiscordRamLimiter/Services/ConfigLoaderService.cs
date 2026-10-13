
using Microsoft.Extensions.Configuration.Ini;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Reflection;
using System.Text;

namespace DiscordRamLimiter.Services;

public sealed class ConfigLoaderService
{
    bool isOnCooldown = false;
    string configFileLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\config.ini";

    IniConfigurationProvider createConfigProvider()
    {
        IniConfigurationProvider configProvider = new IniConfigurationProvider(new IniConfigurationSource()
        {
            Path = "config.ini",
            FileProvider = new PhysicalFileProvider(AppContext.BaseDirectory)
        });

        return configProvider;
    }

    public bool configFileExists()
    {
        return File.Exists(configFileLocation);
    }

    public void createConfigFile()
    {
        File.Create(configFileLocation);
    }

    public string start()
    {
        bool configFileExists = this.configFileExists();
        string GCTriggerLimit;

        if (!configFileExists)
        {
            File.WriteAllText(configFileLocation, "GCTriggerLimit=0" + Environment.NewLine);
            GCTriggerLimit = "0";
        } else
        {
            IniConfigurationProvider configProvider = createConfigProvider();
            configProvider.Load();
            configProvider.TryGet("GCTriggerLimit", out GCTriggerLimit);
        }

        return GCTriggerLimit;
    }

    public async void changeConfigSetting(string GCTriggerLimit)
    {
        if (!isOnCooldown)
        {
            File.WriteAllText(configFileLocation, "GCTriggerLimit=" + GCTriggerLimit + Environment.NewLine);
            isOnCooldown = true;
            await Task.Delay(5000);
            isOnCooldown = false;
        }
    }
}
