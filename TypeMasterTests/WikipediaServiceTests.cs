namespace TypeMasterTests
{
    public class WikipediaServiceTests
    {
        WikipediaService _wikipediaService;
        LanguagesService _languagesService;
        CurrentPageService _currentPageService;
        [SetUp]
        public void Setup()
        {
            _languagesService = new LanguagesService();
            var saveLoadService = new DataSaveLoadService(new CryptographyService());
            _wikipediaService = new WikipediaService(saveLoadService);
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