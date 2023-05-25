using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TypeMaster.Service;

public partial class LanguagesService : ObservableObject
{
    readonly Dictionary<string, string> _regexMatchWordsInDiffrentLanguages;

    public string[] AvailableLanguages => _regexMatchWordsInDiffrentLanguages.Keys.ToArray();

    public bool IsInAvailableLanguages(string language) => _regexMatchWordsInDiffrentLanguages.ContainsKey(language);

    public LanguagesService()
    {
        _regexMatchWordsInDiffrentLanguages = new Dictionary<string, string>
        {
            { "en", "(?:\\w*[^\\x00-\\x7F]+\\w*)" },
            { "pl", "(?:\\w*[^\\x00-\\x7FĄąĆćĘęŁłŃńÓóŚśŹźŻż]+\\w*)" },
            { "es", "(?:\\w*[^\\x00-\\x7FáéíóúÁÉÍÓÚñÑ-]+\\w*)" }
        };
    }

    public string FilterTextByLanguage(string inputText, string language)
    {
        if (_regexMatchWordsInDiffrentLanguages.TryGetValue(language, out string regexPattern))
        {
            string filteredText = Regex.Replace(inputText, regexPattern, "");

            return filteredText;
        }

        return inputText;
    }
}
