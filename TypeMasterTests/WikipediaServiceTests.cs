namespace TypeMasterTests
{
    public class WikipediaServiceTests
    {
        WikipediaService WikipediaService;
        LanguagesService LanguagesService;
        [SetUp]
        public void Setup()
        {
            LanguagesService = new LanguagesService();
            var saveLoadService = new DataSaveLoadService(new CryptographyService());
            WikipediaService = new WikipediaService(saveLoadService, LanguagesService);
        }

        [Test]
        public async Task TryGetWikipediaPageInfoAsyncTest()
        {
            string lang = "en";
            TextLength textLength = TextLength.Medium;

            var args = new RandomPageInfoArgs(textLength, lang);
            (SearchResult? info, string? content) = await WikipediaService.TryGetWikipediaPageInfoAsync();
            Assert.IsNull(info);
            Assert.IsNull(content);

            WikipediaService.GetPageInfoArgs = args;
            
            (info, content) = await WikipediaService.TryGetWikipediaPageInfoAsync();
            Assert.IsNotNull(info);
            Assert.IsNotNull(content);
        }

        [Test]
        public async Task GetWikipediaPageContentTest()
        {
            string lang = "en";
            TextLength textLength = TextLength.Short;
            PageInfoArgs args = new RandomPageInfoArgs(textLength, lang);
            WikipediaService.GetPageInfoArgs = args;

            SearchResult? info;
            string? content;
            do
                (info, content) = await WikipediaService.TryGetWikipediaPageInfoAsync();
            while ((info, content) == (null, null));

            //for future
            content = WikipediaService.FormatPageContent(content, lang);
            if (content.Length >= (int)textLength)
                Assert.IsTrue(WikipediaService.CutPageContent(content, textLength).Length == (int)textLength);

            args = new IdPageInfoArgs(info!.Id, textLength, lang);
            WikipediaService.GetPageInfoArgs = args;

            (SearchResult? infoAgain, string? contentAgain) = await WikipediaService.TryGetWikipediaPageInfoAsync();
            Assert.IsNotNull(infoAgain);
            Assert.IsNotNull(contentAgain);

            Assert.IsTrue(info.IsEqualTo(infoAgain));
            Assert.That(content, Is.EqualTo(contentAgain!));

            var content3 = await WikipediaService.GetWikipediaPageContent();
            Assert.That(content, Is.EqualTo(content3));
            Assert.That(contentAgain, Is.EqualTo(content3));
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