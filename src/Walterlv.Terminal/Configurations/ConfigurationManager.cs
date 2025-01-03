using Tomlet;

namespace Walterlv.Terminal.Configurations;

public class ConfigurationManager
{
    private readonly string _configurationPath;

    public ConfigurationManager()
    {
        _configurationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "terminal.toml");
    }

    public void Reload()
    {
        if (!File.Exists(_configurationPath))
        {
            return;
        }

    }
}
