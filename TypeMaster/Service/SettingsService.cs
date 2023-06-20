using System;
using System.Resources;
using System.Threading.Tasks;

namespace TypeMaster.Service;

public class SettingsService
{
    readonly LanguagesService _languagesService;
    readonly DataSaveLoadService _dataSaveLoadService;
    readonly Settings _settings;

    public string CurrentLanguage => _settings.CurrentLanguage;

    public event EventHandler<OnLanguageChangedEventArgs> OnLanguageChanged;

    readonly ResourceManager resourceManager;
    public SettingsService(DataSaveLoadService dataSaveLoadService, LanguagesService languagesService)
    {
        _dataSaveLoadService = dataSaveLoadService;
        _settings = new Settings { CurrentLanguage = "en" };
        _languagesService = languagesService;
        Properties.Resources.Culture = new System.Globalization.CultureInfo("en");

        resourceManager = new ResourceManager("TypeMaster.Properties.Text", this.GetType().Assembly);
    }

    public async Task GetSettingsDataAsync()
    {
        var data = await _dataSaveLoadService.GetDataAsync<Settings>();
        if (data != null)
            TryChangeCurrentLanguage(data.CurrentLanguage);
    }

    public async Task SaveSettingsDataAsync()
    {
        await _dataSaveLoadService.SaveDataAsync(_settings);
    }
    public void TryChangeCurrentLanguage(string language)
    {
        if (_languagesService.IsInAvailableLanguages(language) && language != _settings.CurrentLanguage)
        {
            _settings.CurrentLanguage = language;
            Properties.Resources.Culture = new System.Globalization.CultureInfo(language);
            if (OnLanguageChanged != null)
                OnLanguageChanged(this, new OnLanguageChangedEventArgs(GetUITextValue));
        }
    }

    public string GetUITextValue(string name)
    {
        return resourceManager.GetString(name, Properties.Resources.Culture) ?? "";
    }
}

public class OnLanguageChangedEventArgs : EventArgs
{
    public Func<string, string> GetText;
    public OnLanguageChangedEventArgs(Func<string, string> getText)
    {
        GetText = getText;
    }
}