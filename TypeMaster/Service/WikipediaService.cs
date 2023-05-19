using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TypeMaster.Service;

public class WikipediaService
{
    DataSaveLoadService DataSaveLoadService { get; }
    LanguagesService LanguagesService { get; }

    public HashSet<WikipediaPageInfo> Scores { get; private set; }
    public PageInfoArgs? GetPageInfoArgs { get; set; }

    public WikipediaService(DataSaveLoadService dataSaveLoadService, LanguagesService languagesService)
    {
        DataSaveLoadService = dataSaveLoadService;
        LanguagesService = languagesService;

        GetPageInfoArgs = null;
        Scores = DataSaveLoadService.GetData<HashSet<WikipediaPageInfo>>() ?? new ();
    }

    public async Task<(WikipediaPageInfo?, string?)> TryGetWikipediaPageInfoAsync()
    {
        if (GetPageInfoArgs == null)
            return (null, null);

        return await GetWikipediaPageInfoAsync(GetPageInfoArgs.GetUrl(), GetPageInfoArgs.ProvidedTextLength, GetPageInfoArgs.Language);
    }

    public async Task<SearchResult[]?> GetWikipediaSearchResultsAsync(string searchTitle, int resultLimit = 10)
    {
        var url = $"https://en.wikipedia.org/w/api.php?action=query&format=json&list=search&sroffset=0&srsearch={searchTitle}&srprop=&srinfo=&srlimit={resultLimit}";
        
        JToken? searchResults = null;
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string? response = await client.GetStringAsync(url);
                JObject json = JObject.Parse(response);
                searchResults = json?["query"]?["search"];
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        if (searchResults == null)
            return null;

        return GetSearchResults(searchResults).ToArray();

        IEnumerable<SearchResult> GetSearchResults(JToken searchResults)
        {
            foreach (var jToken in searchResults)
            {
                string title;
                if (int.TryParse(jToken?["pageid"]?.ToString(), out int id) && (title = jToken?["title"]!.ToString() ?? string.Empty) != string.Empty)
                {
                    yield return new SearchResult
                    {
                        Id = id,
                        Title = title
                    };
                }
            }
        }
    }

    private async Task<(WikipediaPageInfo?, string?)> GetWikipediaPageInfoAsync(string url, TextLength ProvidedTextLength, string language)
    {
        JToken? pages = await GetWikipediaPagesFromUrl(url);
        try
        {
            JToken? page = pages?.First?.First();
            string? content = page?["extract"]?.ToString();

            if (Int32.TryParse(page?["pageid"]?.ToString(), out int Id) && LanguagesService.CanTypeThisText(content, language))
            {
                var pageInfo = new WikipediaPageInfo
                {
                    Id = Id,
                    Title = page?["title"]?.ToString() ?? throw new NullReferenceException("title not found!"),
                    WPM = 0,
                    Words = 0,
                    ProvidedTextLength = ProvidedTextLength,
                    Language = language
                };
                return (pageInfo, content);
            }
        }
        catch (NullReferenceException ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return (null, null);
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
        string? content = null;
        if (GetPageInfoArgs == null)
            return null;
        try
        {
            JToken? pages = await GetWikipediaPagesFromUrl(GetPageInfoArgs.GetUrl());
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
        WikipediaPageInfo wikipediaPageInfoInScores = Scores.FirstOrDefault(element => element.Id == wikipediaPageInfo.Id, wikipediaPageInfo);
        if (wikipediaPageInfo == wikipediaPageInfoInScores)
        {
            Scores.Add(wikipediaPageInfo);
            return;
        }
        if(wikipediaPageInfoInScores.SecondsSpent < wikipediaPageInfo.SecondsSpent)
        {
            wikipediaPageInfoInScores.WPM = wikipediaPageInfo.WPM;
            wikipediaPageInfoInScores.SecondsSpent = wikipediaPageInfo.SecondsSpent;
        }
    }
}