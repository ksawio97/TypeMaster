using NUnit.Framework;

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
            WikipediaService = new WikipediaService(saveLoadService, LanguagesService, new SettingsService(saveLoadService, LanguagesService));
        }

        [Test]
        public async Task TryGetWikipediaPageInfoAsyncTest()
        {
            string lang = "en";
            TextLength textLength = TextLength.Medium;

            var args = new RandomPageInfoArgs(textLength, lang);
            (WikipediaPageInfo? info, string? content) = await WikipediaService.TryGetWikipediaPageInfoAsync();
            Assert.IsNull(info);
            Assert.IsNull(content);

            WikipediaService.GetPageInfoArgs = args;
            
            (info, content) = await WikipediaService.TryGetWikipediaPageInfoAsync();

            if (content != null && LanguagesService.CanTypeThisText(content, "en"))
                Assert.IsFalse(info == null || content == null);
            else
            {
                Assert.IsNull(info);
                Assert.IsNull(content);
            }
        }

        [Test]
        public async Task GetWikipediaPageContentTest()
        {
            string lang = "en";
            TextLength textLength = TextLength.Medium;
            PageInfoArgs args = new RandomPageInfoArgs(textLength, lang);
            WikipediaService.GetPageInfoArgs = args;

            (WikipediaPageInfo? info, string? content) = await WikipediaService.TryGetWikipediaPageInfoAsync();
            
            if (info != null && content != null)
            {
                //TO DO rewrite TryGetWikipediaPageInfoAsync for it to work (url takes only header not all content and then cuts it)
                Assert.IsTrue(content.Length >= (int)textLength);

                args = new IdPageInfoArgs(info.Id, textLength, lang);
                WikipediaService.GetPageInfoArgs = args;

                (WikipediaPageInfo? infoAgain, string? contentAgain) = await WikipediaService.TryGetWikipediaPageInfoAsync();
                Assert.IsNotNull(infoAgain);
                Assert.IsNotNull(contentAgain);

                Assert.IsTrue(info.IsEqualTo(infoAgain));
                Assert.That(content, Is.EqualTo(contentAgain!));

                var content3 = await WikipediaService.GetWikipediaPageContent();
                Assert.That(content, Is.EqualTo(content3));
                Assert.That(contentAgain, Is.EqualTo(content3));
            }
            else
            {
                //test until language matches
                await GetWikipediaPageContentTest();
            }
        }   
    }
}

public static class WikipediaPageInfoExtensions
{
    public static bool IsEqualTo(this WikipediaPageInfo thisInfo, WikipediaPageInfo otherInfo)
    {
        return
            thisInfo.Id == otherInfo.Id &&
            thisInfo.Title == otherInfo.Title &&
            thisInfo.WPM == otherInfo.SecondsSpent &&
            thisInfo.Words == otherInfo.Words &&
            thisInfo.ProvidedTextLength == otherInfo.ProvidedTextLength &&
            thisInfo.Language == otherInfo.Language;
    }
}