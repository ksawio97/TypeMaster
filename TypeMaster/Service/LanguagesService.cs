using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TypeMaster.Service;

public partial class LanguagesService : ObservableObject
{
    Dictionary<string, string> LanguagesRegex { get; }

    public string[] AvailableLanguages => LanguagesRegex.Keys.ToArray();

    public bool CanTypeThisText(string text, string language) => Regex.IsMatch(text, LanguagesRegex[language]);

    public bool IsInAvailableLanguages(string language) => LanguagesRegex.ContainsKey(language);

    public LanguagesService()
    {
        LanguagesRegex = new Dictionary<string, string>
        {
            { "en", "^[a-zA-Z0-9.,:;!?()\\[\\]{}'\"‘’“”/\\\\\\-\\s]+$" },
            { "pl", "^[a-zA-Z0-9ĄąĆćĘęŁłŃńÓóŚśŹźŻż.,:;!?()\\[\\]{}'\"‘’“”/\\\\\\-\\s]+$" },
            { "es", "^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ.,:;!?()\\[\\]{}'\"‘’“”/\\\\\\-\\s]+$" }
        };
    }
}
