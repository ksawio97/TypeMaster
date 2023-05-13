using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Navigation;

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
    public PageInfoArgs? getPageInfoArgs { private get; set; }

    readonly int minChars;
    readonly int maxChars;

    public WikipediaService(DataSaveLoadService dataSaveLoadService)
    {
        getPageInfoArgs = null;
        _dataSaveLoadService = dataSaveLoadService;
        scores = _dataSaveLoadService.GetWikipediaPageInfos();

        minChars = 10;
        maxChars = 1200;
    }

    public async Task<WikipediaPageInfo?> TryGetWikipediaPageInfoAsync()
    {
        if (getPageInfoArgs == null || getPageInfoArgs.AroundChars < minChars || maxChars < getPageInfoArgs.AroundChars || !languageRegex.ContainsKey(getPageInfoArgs.Language))
            return null;

        return await GetWikipediaPageInfoAsync(getPageInfoArgs.GetUrl(), getPageInfoArgs.AroundChars, getPageInfoArgs.Language);
    }

    public async Task<List<WikipediaPageInfo>> GetWikipediaSearchResultPagesAsync(string url)
    {
        var result = new List<WikipediaPageInfo>();
        JToken? pages = await GetWikipediaPagesFromUrl(url);
        if (pages is not null)
            foreach (JToken page in pages.Children())
            {
                JToken extractedPage = page.First();
                int aroundChars = extractedPage?["extract"]?.ToString().Length ?? 0;
                if (Int32.TryParse(extractedPage?["pageid"]?.ToString(), out int Id) && aroundChars != 0)
                    result.Add(new WikipediaPageInfo
                    {
                        Id = Id,
                        Title = extractedPage?["title"]?.ToString() ?? "",
                        WPM = 0,
                        Words = 0,
                        AroundChars = aroundChars,
                        Language = "en"
                    });
            }
        return result;
    }

    //maybe change it to cut content aroundChars itself not wikipedia
    private async Task<WikipediaPageInfo> GetWikipediaPageInfoAsync(string url, int aroundChars, string language)
    {
        JToken? pages = await GetWikipediaPagesFromUrl(url);
        try
        {
            JToken? page = pages?.First?.First();
            string? content = page?["extract"]?.ToString();

            if (Int32.TryParse(page?["pageid"]?.ToString(), out int Id) && content?.Length >= aroundChars && Regex.IsMatch(content, languageRegex[language]))
            {
                var pageInfo = new WikipediaPageInfo
                {
                    Id = Id,
                    Title = page?["title"]?.ToString() ?? throw new NullReferenceException("title not found!"),
                    WPM = 0,
                    Words = 0,
                    AroundChars = aroundChars,
                    Language = language
                };
                return pageInfo;
            }
        }
        catch (NullReferenceException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return await GetWikipediaPageInfoAsync(url, aroundChars, language);
    }

    private async Task<JToken?> GetWikipediaPagesFromUrl(string url)
    {
        JToken? pages = null;
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string response = await client.GetStringAsync(url);
                JObject json = JObject.Parse(response);
                pages = json["query"]?["pages"];
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        return pages;
    }


    public async Task<string?> GetWikipediaPageContent()
    {
        JToken? pages = await GetWikipediaPagesFromUrl(getPageInfoArgs.GetUrl());
        string? content = null;
        try
        {
            JToken? page = pages?.First?.First();
            content = page?["extract"]?.ToString();
        }
        catch (NullReferenceException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return content;
    }

    public void AddScore(WikipediaPageInfo wikipediaPageInfo)
    {
        WikipediaPageInfo wikipediaPageInfoInScores = scores.FirstOrDefault(element => element.Id == wikipediaPageInfo.Id, wikipediaPageInfo);
        if (wikipediaPageInfo == wikipediaPageInfoInScores)
            scores.Add(wikipediaPageInfo);
        else if(wikipediaPageInfoInScores.WPM < wikipediaPageInfo.WPM)
        {
            wikipediaPageInfoInScores.WPM = wikipediaPageInfo.WPM;
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