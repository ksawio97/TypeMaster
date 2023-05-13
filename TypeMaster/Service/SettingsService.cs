using System;

namespace TypeMaster.Service;

public class SettingsService
{
    LanguagesService LanguagesService { get; }
    public Settings Settings { get; private set; }

    public string CurrentLanguage => Settings.CurrentLanguage;

    public TextLength ProvidedTextLength => Settings.ProvidedTextLength;

    public SettingsService(DataSaveLoadService dataSaveLoadService, LanguagesService languagesService)
    {
        Settings = dataSaveLoadService.GetData<Settings>() ?? new Settings { CurrentLanguage = "en", ProvidedTextLength = TextLength.Short};
        LanguagesService = languagesService;
    }

    public void TryChangeCurrentLanguage(string language)
    {
        if (LanguagesService.IsInAvailableLanguages(language) && language != Settings.CurrentLanguage)
            Settings.CurrentLanguage = language;
    }

    public void TryChangeProvidedTextLength(string enumName)
    {
        if (Enum.TryParse<TextLength>(enumName, out TextLength option))
            Settings.ProvidedTextLength = option;
    }
}
