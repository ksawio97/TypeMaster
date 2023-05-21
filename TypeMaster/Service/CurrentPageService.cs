using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TypeMaster.Service;

public class CurrentPageService
{
    public PageInfoArgs? CurrentPageInfoArgs { get; set; }

    public bool IsCurrentPageInfoArgsNull => CurrentPageInfoArgs == null;

    SearchResult? WikipediaPageResult { get; set; }

    public string? Content { get; private set; }

    WikipediaService WikipediaService { get; }

    LanguagesService LanguagesService { get; }

    public CurrentPageService(WikipediaService wikipediaService, LanguagesService languagesService)
    {
        WikipediaService = wikipediaService;
        LanguagesService = languagesService;
    }

    public async Task<SearchResult?> GetPageResult()
    {
        if (!WikipediaPageResult.IsNullOrEmpty())
            return WikipediaPageResult;

        if (await TrySetPageInfoAndContent())
            return WikipediaPageResult;

        return null;
    }

    public async Task<string?> TryGetPageContent(bool formated = false, bool cutted = false)
    {
        if (formated || cutted)
            if (CurrentPageInfoArgs == null)
                return null;

        if(cutted && CurrentPageInfoArgs!.ProvidedTextLength == null)
            return null;

        if (Content != null)
            return GetPageContent(formated, cutted);

        if (await TrySetPageInfoAndContent())
            return GetPageContent(formated, cutted);

        return null;
    }

    private string GetPageContent(bool formated, bool cutted)
    {
        string content = Content!;
        if(formated)
            content = FormatPageContent(content, CurrentPageInfoArgs!.Language);
        if(cutted)
            content = CutPageContent(content, (TextLength)CurrentPageInfoArgs!.ProvidedTextLength!);

        return content;
    }

    public WikipediaPageInfo? GetWikipediaPageInfo(int wordsCount, int wpm = 0, int secondsSpent = 0)
    {
        if (WikipediaPageResult.IsNullOrEmpty() || CurrentPageInfoArgs == null || CurrentPageInfoArgs.ProvidedTextLength == null || wordsCount <= 0)
            return null;

        return new WikipediaPageInfo
        {
            Id = WikipediaPageResult!.Id,
            Title = WikipediaPageResult!.Title,
            WPM = wpm,
            SecondsSpent = secondsSpent,
            Words = wordsCount,
            ProvidedTextLength = (TextLength)CurrentPageInfoArgs.ProvidedTextLength,
            Language = CurrentPageInfoArgs.Language
        };
    }

    async Task<bool> TrySetPageInfoAndContent()
    {
        if (CurrentPageInfoArgs == null)
            return false;

        (WikipediaPageResult, Content) = await WikipediaService.TryGetWikipediaPageInfoAsync(CurrentPageInfoArgs);

        return !(WikipediaPageResult == null || Content == null);
    }

    string FormatPageContent(string content, string language)
    {
        content = LanguagesService.FilterTextByLanguage(content, language);
        content = content.Replace("\n", " ");
        content = TwoOrMoreSpaces().Replace(content, " ").ToString();

        return content;
    }

    string CutPageContent(string content, TextLength textLength)
    {
        return content[..((int)textLength - 3)] + "...";
    }


    [GeneratedRegex(" {2,}")]
    private partial Regex TwoOrMoreSpaces();
}

public static class SearchResultExtensions 
{ 
    public static bool IsNullOrEmpty(this SearchResult? searchResult)
    {
        return searchResult == null ||
               searchResult.Title.Equals(string.Empty) ||
               searchResult.Id <= 0;
    }
}
