using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace TypeMaster.Service;

public partial class WikipediaService
{
    readonly List<WikipediaPageInfo> _pendingScores;

    readonly DataSaveLoadService _dataSaveLoadService;
    readonly NetworkAvailabilityService _networkAvailabilityService;
    public WikipediaService(DataSaveLoadService dataSaveLoadService, NetworkAvailabilityService networkAvailabilityService)
    {
        _dataSaveLoadService = dataSaveLoadService;
        _networkAvailabilityService = networkAvailabilityService;
        _pendingScores = new ();
    }

    public async Task<(SearchResult?, string?)> TryGetWikipediaPageInfoAsync(PageInfoArgs CurrentPageInfoArgs)
    {
        if (CurrentPageInfoArgs == null || !_networkAvailabilityService.CheckAvailability())
            return (null, null);

        return await GetWikipediaPageInfoAsync(CurrentPageInfoArgs.GetUrl(), CurrentPageInfoArgs.Language);
    }

    public async Task<SearchResult[]?> GetWikipediaSearchResultsAsync(string searchTitle, string language, int resultLimit = 10)
    {
        if (!_networkAvailabilityService.CheckAvailability())
            return null;

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
            catch (Exception ex)
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
        if (pages == null)
            return (null, null);

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
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        return (null, null);
    }

    private async Task<JToken?> GetWikipediaPagesFromUrl(string url)
    {
        if (!_networkAvailabilityService.CheckAvailability())
            return null;

        JToken? pages = null;
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string response = await client.GetStringAsync(url);
                JObject json = JObject.Parse(response);
                pages = json["query"]?["pages"];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        return pages;
    }

    public void AddScore(WikipediaPageInfo wikipediaPageInfo)
    {
        WikipediaPageInfo? wikipediaPageInfoInScores = _pendingScores.Find(element => element.Id == wikipediaPageInfo.Id);
        if (wikipediaPageInfoInScores == null)
        {
            _pendingScores.Add(wikipediaPageInfo);
            return;
        }
        if(wikipediaPageInfoInScores.SecondsSpent < wikipediaPageInfo.SecondsSpent)
        {
            wikipediaPageInfoInScores.WPM = wikipediaPageInfo.WPM;
            wikipediaPageInfoInScores.SecondsSpent = wikipediaPageInfo.SecondsSpent;
        }
    }

    public async IAsyncEnumerable<WikipediaPageInfo> GetScoresDataAsync()
    {
        await foreach (var score in _dataSaveLoadService.GetDataCollectionAsync<WikipediaPageInfo>())
        {
            var simillarPage = _pendingScores.Find(element => element.Id == score.Id);
            //when simillarPage has better score
            if (simillarPage != null && score.SecondsSpent > simillarPage.SecondsSpent)
                yield return simillarPage;
            else
            {
                if(simillarPage != null)
                    _pendingScores.Remove(simillarPage);
                yield return score;
            }
        }
        //return remaining scores
        foreach (var score in _pendingScores) yield return score;

        yield break;
    }

    public Task SaveScoresData()
    {
        var scores = new List<WikipediaPageInfo>();

        foreach(var score in _dataSaveLoadService.GetData<List<WikipediaPageInfo>>() ?? new List<WikipediaPageInfo>())
        {
            var simillarPage = _pendingScores.Find(element => element.Id == score.Id);
            //when simillarPage has better score
            if (simillarPage != null && score.SecondsSpent > simillarPage.SecondsSpent)
                scores.Add(simillarPage);
            else
            {
                if (simillarPage != null)
                    _pendingScores.Remove(simillarPage);
                scores.Add(score);
            }
        }

        scores.AddRange(_pendingScores);
        _pendingScores.Clear();
        _dataSaveLoadService.SaveData(scores);
        return Task.CompletedTask;
    }
}