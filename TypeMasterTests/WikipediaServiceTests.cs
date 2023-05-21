namespace TypeMasterTests
{
    public class WikipediaServiceTests
    {
        WikipediaService WikipediaService;
        LanguagesService LanguagesService;
        CurrentPageService CurrentPageService;
        [SetUp]
        public void Setup()
        {
            LanguagesService = new LanguagesService();
            var saveLoadService = new DataSaveLoadService(new CryptographyService());
            WikipediaService = new WikipediaService(saveLoadService);
            CurrentPageService = new CurrentPageService(WikipediaService, LanguagesService);
        }

        [Test]
        public async Task TryGetWikipediaPageInfoAsyncTest()
        {
            string lang = "en";
            TextLength textLength = TextLength.Medium;

            var args = new RandomPageInfoArgs(textLength, lang);
            (SearchResult? info, string? content) = await WikipediaService.TryGetWikipediaPageInfoAsync(args);
            Assert.IsTrue(info != null && content != null);
        }

        [Test]
        public async Task GetWikipediaPageContentTest()
        {
            string lang = "en";
            TextLength textLength = TextLength.Short;
            PageInfoArgs args = new RandomPageInfoArgs(textLength, lang);
            CurrentPageService.CurrentPageInfoArgs = args;

            (SearchResult? info, string? content) = await WikipediaService.TryGetWikipediaPageInfoAsync(args);
            Assert.IsTrue(info != null && content != null);

            args = new IdPageInfoArgs(info.Id, null, lang);
            (SearchResult? info2, string? content2) = await WikipediaService.TryGetWikipediaPageInfoAsync(args);
            Assert.True(info.IsEqualTo(info2));
            Assert.That(content, Is.EqualTo(content2));
        }

        [Test]
        public void AddScoreTest()
        {
            int oldCount = WikipediaService.Scores.Count;
            var wikipediaPageInfo = new WikipediaPageInfo();
            WikipediaService.AddScore(wikipediaPageInfo);
            Assert.That(oldCount + 1, Is.EqualTo(WikipediaService.Scores.Count));
            Assert.True(WikipediaService.Scores.Contains(wikipediaPageInfo));
        }
        [Test]
        public async Task GetWikipediaSearchResultsTest()
        {
            var result = await WikipediaService.GetWikipediaSearchResultsAsync("Dwayne Johnson", 5);

            Assert.IsNotNull(result);
            Assert.True(result.Length > 0);
        }
    }
}

public static class SearchResultExtensions
{
    public static bool IsEqualTo(this SearchResult thisInfo, SearchResult otherInfo)
    {
        return
            thisInfo.Id == otherInfo.Id &&
            thisInfo.Title == otherInfo.Title;
    }
}