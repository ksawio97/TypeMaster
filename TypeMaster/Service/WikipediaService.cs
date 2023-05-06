using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public HashSet<WikipediaPageInfo> scores { get; private set; }
    DataSaveLoadService _dataSaveLoadService;

    readonly int minChars;
    readonly int maxChars;

    public WikipediaService(DataSaveLoadService dataSaveLoadService)
    {
        _dataSaveLoadService = dataSaveLoadService;
        scores = _dataSaveLoadService.GetWikipediaPageInfos();

        minChars = 10;
        maxChars = 1200;
    }

    public async Task<WikipediaPageInfo?> TryGetRandomWikipediaPageInfoAsync(int aroundChars, string language)
    {
        if (aroundChars < minChars || maxChars < aroundChars || !languageRegex.ContainsKey(language))
            return null;
        return await GetRandomWikipediaPageInfoAsync(aroundChars, language);
    }

    public async Task<string?> GetWikipediaPageContent(int id, int aroundChars)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://en.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&pageids={id}&explaintext=true&exchars={aroundChars}";
            string response = await client.GetStringAsync(url);

            JObject json = JObject.Parse(response);
            JToken? pages = json?["query"]?["pages"];
            JToken? page = pages?.First?.First();

            return page?["extract"]?.ToString() ?? null;
        }
    }


    private async Task<WikipediaPageInfo> GetRandomWikipediaPageInfoAsync(int aroundChars, string language)
    {
        using (HttpClient client = new HttpClient())
        {
            string url = $"https://{language}.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&explaintext=true&generator=random&grnnamespace=0&grnlimit=1&exchars={aroundChars}";
            string response = await client.GetStringAsync(url);

            try
            {
                JObject json = JObject.Parse(response);
                JToken? pages = json["query"]?["pages"];
                JToken? page = pages?.First?.First();
                string? content = page?["extract"]?.ToString();

                if (Int32.TryParse(page?["pageid"]?.ToString(), out int Id) && content?.Length >= aroundChars && Regex.IsMatch(content, languageRegex[language]))
                {
                    var score = new WikipediaPageInfo
                    {
                        Id = Id,
                        Title = page?["title"]?.ToString() ?? throw new NullReferenceException("title not found!"),
                        WPM = 0,
                        Words = 0,
                        AroundChars = aroundChars
                    };
                    scores.Add(score);
                    return score;
                }
            }
            catch(NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return await GetRandomWikipediaPageInfoAsync(aroundChars, language);
        }
    }
}

public static class HashSetExtensions 
{ 
    public static WikipediaPageInfo? GetWikipediaPageScoreById(this HashSet<WikipediaPageInfo> values, int id)
    {
        return values.FirstOrDefault(element => element.Id == id);
    }
}