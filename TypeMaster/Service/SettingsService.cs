using System.Threading.Tasks;

namespace TypeMaster.Service;

public class SettingsService
{
    readonly LanguagesService _languagesService;
    readonly DataSaveLoadService _dataSaveLoadService;
    readonly Settings _settings;

    public string CurrentLanguage => _settings.CurrentLanguage;

    public SettingsService(DataSaveLoadService dataSaveLoadService, LanguagesService languagesService)
    {
        _dataSaveLoadService = dataSaveLoadService;
        _settings = new Settings { CurrentLanguage = "en" };
        _languagesService = languagesService;
    }

    public async Task GetSettingsDataAsync()
    {
        var data = await _dataSaveLoadService.GetDataAsync<Settings>();
        if (data != null && data.CurrentLanguage != _settings.CurrentLanguage)
            _settings.CurrentLanguage = data.CurrentLanguage;
    }

    public async Task SaveSettingsDataAsync()
    {
        await _dataSaveLoadService.SaveDataAsync(_settings);
    }
    public void TryChangeCurrentLanguage(string language)
    {
        if (_languagesService.IsInAvailableLanguages(language) && language != _settings.CurrentLanguage)
            _settings.CurrentLanguage = language;
    }
}
