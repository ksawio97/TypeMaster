using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TypeMaster.Service;

public class WikipediaService
{
    readonly Dictionary<string, string> languageRegex = new Dictionary<string, string>
        {
            { "en", "^[a-zA-Z0-9.,:;!?()\\[\\]{}'\"‘’“”/\\\\\\-\\s]+$" },
            { "pl", "^[a-zA-Z0-9ĄąĆćĘęŁłŃńÓóŚśŹźŻż.,:;!?()\\[\\]{}'\"‘’“”/\\\\\\-\\s]+$" },
            { "es", "^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ.,:;!?()\\[\\]{}'\"‘’“”/\\\\\\-\\s]+$" }
            // Add more language codes and regex patterns as needed
        };

    readonly int minChars;
    readonly int maxChars;

    public WikipediaService()
    {
        minChars = 10;
        maxChars = 1200;
    }

    public async Task<string?> TryGetWikipediaPageAsync(int aroundChars, string language)
    {
        if (aroundChars < minChars || maxChars < aroundChars || !languageRegex.ContainsKey(language))
            return null;
        return await GetWikipediaPageAsync(aroundChars, language);
    }

    private async Task<string> GetWikipediaPageAsync(int aroundChars, string language)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://{language}.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&explaintext=true&generator=random&grnnamespace=0&grnlimit=1&exchars={aroundChars}";
            string response = await client.GetStringAsync(url);
            //maybe add try?
            JObject json = JObject.Parse(response);
            JToken pages = json["query"]["pages"];
            var page = pages.First.First();

            string content = page["extract"].ToString();

            if (content.Length < aroundChars || !Regex.IsMatch(content, languageRegex[language]))
                return await GetWikipediaPageAsync(aroundChars, language);

            //return new WikipediaPageScore { 
            //    PageIndex = Int32.Parse(page["pageid"].ToString()),
            //    PageTitle = page["title"].ToString(),
            //    UserScore = 0
            //};
            return content;
        }
    }
}
