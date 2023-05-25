using System;

namespace TypeMaster.Service;

public class SettingsService
{
    readonly LanguagesService _languagesService;

    readonly public Settings _settings;

    public string CurrentLanguage => _settings.CurrentLanguage;

    public SettingsService(DataSaveLoadService dataSaveLoadService, LanguagesService languagesService)
    {
        _settings = dataSaveLoadService.GetData<Settings>() ?? new Settings { CurrentLanguage = "en" };
        _languagesService = languagesService;
    }

    public void TryChangeCurrentLanguage(string language)
    {
        if (_languagesService.IsInAvailableLanguages(language) && language != _settings.CurrentLanguage)
            _settings.CurrentLanguage = language;
    }
}
