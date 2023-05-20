using System;

namespace TypeMaster.Service;

public class SettingsService
{
    LanguagesService LanguagesService { get; }

    public Settings Settings { get; private set; }

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
