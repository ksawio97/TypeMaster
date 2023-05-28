﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace TypeMaster.Service;

public partial class WikipediaService
{
    readonly DataSaveLoadService _dataSaveLoadService;

    readonly List<WikipediaPageInfo> _pendingScores;
    public WikipediaService(DataSaveLoadService dataSaveLoadService)
    {
        _dataSaveLoadService = dataSaveLoadService;
        _pendingScores = new ();
    }

    public async Task<(SearchResult?, string?)> TryGetWikipediaPageInfoAsync(PageInfoArgs CurrentPageInfoArgs)
    {
        if (CurrentPageInfoArgs == null || !NetworkInterface.GetIsNetworkAvailable())
            return (null, null);

        return await GetWikipediaPageInfoAsync(CurrentPageInfoArgs.GetUrl(), CurrentPageInfoArgs.Language);
    }

    public async Task<SearchResult[]?> GetWikipediaSearchResultsAsync(string searchTitle, string language, int resultLimit = 10)
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
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
        if (!NetworkInterface.GetIsNetworkAvailable())
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
        await foreach(var score in _dataSaveLoadService.GetDataCollectionAsync<WikipediaPageInfo>())
        {
            var simillarPage = _pendingScores.Find(element => element.Id == score.Id);
            if (simillarPage == null || score.SecondsSpent < simillarPage.SecondsSpent)
                yield return score;
            else
                yield return simillarPage;
        }
        yield break;
    }

    public async Task SaveScoresDataAsync()
    {
        var scores = await _dataSaveLoadService.GetDataAsync<List<WikipediaPageInfo>>();
        foreach(var pendingScore in _pendingScores)
        {
            var simillarPage = _pendingScores.Find(element => element.Id == pendingScore.Id);
            if (simillarPage != null && pendingScore.SecondsSpent < simillarPage.SecondsSpent)
            {
                pendingScore.WPM = simillarPage.WPM;
                pendingScore.SecondsSpent = simillarPage.SecondsSpent;
            }
        }
        await _dataSaveLoadService.SaveDataAsync(scores);
    }
}