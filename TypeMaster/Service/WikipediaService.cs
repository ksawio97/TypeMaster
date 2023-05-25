using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TypeMaster.Service;

public partial class WikipediaService
{
    readonly DataSaveLoadService _dataSaveLoadService;

    public readonly HashSet<WikipediaPageInfo> Scores;

    public WikipediaService(DataSaveLoadService dataSaveLoadService)
    {
        _dataSaveLoadService = dataSaveLoadService;

        Scores = _dataSaveLoadService.GetData<HashSet<WikipediaPageInfo>>() ?? new ();
    }

    public async Task<(SearchResult?, string?)> TryGetWikipediaPageInfoAsync(PageInfoArgs CurrentPageInfoArgs)
    {
        if (CurrentPageInfoArgs == null)
            return (null, null);

        return await GetWikipediaPageInfoAsync(CurrentPageInfoArgs.GetUrl(), CurrentPageInfoArgs.Language);
    }

    public async Task<SearchResult[]?> GetWikipediaSearchResultsAsync(string searchTitle, string language, int resultLimit = 10)
    {
        var url = $"https://{language}.wikipedia.org/w/api.php?action=query&format=json&list=search&sroffset=0&srsearch={searchTitle}&srprop=&srinfo=&srlimit={resultLimit}";
        
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

    private async Task<(SearchResult?, string?)> GetWikipediaPageInfoAsync(string url, string language)
    {
        JToken? pages = await GetWikipediaPagesFromUrl(url);
        try
        {
            JToken? page = pages?.First?.First();
            string? content = page?["extract"]?.ToString();

            string? title = null;
            if (Int32.TryParse(page?["pageid"]?.ToString(), out int id) && (title = page?["title"]?.ToString()) != null && content != null)
            {
                var pageInfo = new SearchResult
                {
                    Id = id,
                    Title = title
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