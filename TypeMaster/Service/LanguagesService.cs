using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TypeMaster.Service;

public partial class LanguagesService : ObservableObject
{
    Dictionary<string, string> RegexMatchWordsInDiffrentLanguages { get; }

    public string[] AvailableLanguages => RegexMatchWordsInDiffrentLanguages.Keys.ToArray();

    public bool IsInAvailableLanguages(string language) => RegexMatchWordsInDiffrentLanguages.ContainsKey(language);

    public LanguagesService()
    {
        RegexMatchWordsInDiffrentLanguages = new Dictionary<string, string>
        {
            { "en", "(?:\\w*[^\\x00-\\x7F]+\\w*)" },
            { "pl", "(?:\\w*[^\\x00-\\x7FĄąĆćĘęŁłŃńÓóŚśŹźŻż]+\\w*)" },
            { "es", "(?:\\w*[^\\x00-\\x7FáéíóúÁÉÍÓÚñÑ-]+\\w*)" }
        };
    }

    public string FilterTextByLanguage(string inputText, string language)
    {
        if (RegexMatchWordsInDiffrentLanguages.TryGetValue(language, out string regexPattern))
        {
            string filteredText = Regex.Replace(inputText, regexPattern, "");

            return filteredText;
        }

        return inputText;
    }
}
