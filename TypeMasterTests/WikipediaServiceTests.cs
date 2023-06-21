namespace TypeMasterTests
{
    public class WikipediaServiceTests
    {
        WikipediaService _wikipediaService;
        LanguagesService _languagesService;
        CurrentPageService _currentPageService;
        NetworkAvailabilityService _networkAvailabilityService;
        DataSaveLoadService _dataSaveLoadService;
        [SetUp]
        public void Setup()
        {
            _languagesService = new LanguagesService();
            _dataSaveLoadService = new DataSaveLoadService(new CryptographyService());
            _networkAvailabilityService = new NetworkAvailabilityService();
            _wikipediaService = new WikipediaService(_dataSaveLoadService, _networkAvailabilityService);
            _currentPageService = new CurrentPageService(_wikipediaService, _languagesService);
        }

        [Test]
        public async Task TryGetWikipediaPageInfoAsyncTest()
        {
            string lang = "en";
            TextLength textLength = TextLength.Medium;

            var args = new RandomPageInfoArgs(textLength, lang);
            (SearchResult? info, string? content) = await _wikipediaService.TryGetWikipediaPageInfoAsync(args);
            Assert.IsTrue(info != null && content != null);
        }

        [Test]
        public async Task GetWikipediaPageContentTest()
        {
            string lang = "en";
            TextLength textLength = TextLength.Short;
            PageInfoArgs args = new RandomPageInfoArgs(textLength, lang);
            _currentPageService.CurrentPageInfoArgs = args;

            (SearchResult? info, string? content) = await _wikipediaService.TryGetWikipediaPageInfoAsync(args);
            Assert.IsTrue(info != null && content != null);

            args = new IdPageInfoArgs(info.Id, null, lang);
            (SearchResult? info2, string? content2) = await _wikipediaService.TryGetWikipediaPageInfoAsync(args);
            Assert.True(info.IsEqualTo(info2));
            Assert.That(content, Is.EqualTo(content2));
        }

        [Test]
        public async Task GetWikipediaSearchResultsTest()
        {
            var result = await _wikipediaService.GetWikipediaSearchResultsAsync("Dwayne Johnson", "en", 5);

            Assert.IsNotNull(result);
            Assert.True(result.Length > 0);
        }

        [Test]
        public async Task SaveScoresDataAsyncTest()
        {
            _wikipediaService.AddScore(new WikipediaPageInfo(){ Id=213, Title="Test"});
            _wikipediaService.AddScore(new WikipediaPageInfo(){ Id=2132, Title="Test2"});
            _wikipediaService.AddScore(new WikipediaPageInfo(){ Id=321, Title="Test3"});

            //get scores
            List<WikipediaPageInfo> scores = new ();
            await foreach (var score in _wikipediaService.GetScoresDataAsync())
                scores.Add(score);

            //save scores
            await _wikipediaService.SaveScoresData();

            //get scores from file
            List<WikipediaPageInfo> results = await _dataSaveLoadService.GetDataAsync<List<WikipediaPageInfo>>() ?? new List<WikipediaPageInfo>();
            if (results.Count != scores.Count)
                Assert.Fail("Diffrent lengths!");
            for(int i = 0; i < results.Count; i++)
            {
                if (!results[i].IsEqualTo(scores[i]))
                    Assert.Fail("Diffrent scores!");
            }
        }
    }
}

public static class SearchResultExtensions
{
    public static bool IsEqualTo(this SearchResult thisResult, SearchResult otherResult)
    {
        return
            thisResult.Id == otherResult.Id &&
            thisResult.Title == otherResult.Title;
    }
}

public static class WikipediaPageInfoExtensions
{
    public static bool IsEqualTo(this WikipediaPageInfo thisInfo, WikipediaPageInfo otherInfo)
    {
        return
            thisInfo.Id == otherInfo.Id &&
            thisInfo.Title == otherInfo.Title &&
            thisInfo.WPM == otherInfo.WPM &&
            thisInfo.SecondsSpent == otherInfo.SecondsSpent &&
            thisInfo.Words == otherInfo.Words &&
            thisInfo.ProvidedTextLength == otherInfo.ProvidedTextLength &&
            thisInfo.Language == otherInfo.Language;
    }
}