using System;

namespace TypeMaster.Service;

public class SettingsService
{
    readonly LanguagesService LanguagesService;

    readonly public Settings Settings;

    public string CurrentLanguage => Settings.CurrentLanguage;

    public SettingsService(DataSaveLoadService dataSaveLoadService, LanguagesService languagesService)
    {
        Settings = dataSaveLoadService.GetData<Settings>() ?? new Settings { CurrentLanguage = "en" };
        LanguagesService = languagesService;
    }

    public void TryChangeCurrentLanguage(string language)
    {
        if (LanguagesService.IsInAvailableLanguages(language) && language != Settings.CurrentLanguage)
            Settings.CurrentLanguage = language;
    }
}
