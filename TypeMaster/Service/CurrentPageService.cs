using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TypeMaster.Service;

public partial class CurrentPageService
{
    private PageInfoArgs? _currentPageInfoArgs;

    public PageInfoArgs? CurrentPageInfoArgs
    {
        get
        {
            return _currentPageInfoArgs;
        }

        set
        {
            WikipediaPageResult = null;
            Content = null;

            _currentPageInfoArgs = value;
        }
    }

    public bool IsCurrentPageInfoArgsNull => CurrentPageInfoArgs == null;

    SearchResult? WikipediaPageResult { get; set; }

    public string? Content { get; private set; }

    readonly WikipediaService WikipediaService;

    readonly LanguagesService LanguagesService;

    public CurrentPageService(WikipediaService wikipediaService, LanguagesService languagesService)
    {
        WikipediaService = wikipediaService;
        LanguagesService = languagesService;
    }

    public async Task<SearchResult?> TryGetPageResult()
    {
        if (!WikipediaPageResult.IsNullOrEmpty())
            return WikipediaPageResult;

        if (await TrySetPageInfoAndContent())
            return WikipediaPageResult;

        return null;
    }

    public async Task<string?> TryGetPageContent(bool formatted = false, bool cutted = false)
    {
        if (formatted || cutted)
            if (CurrentPageInfoArgs == null)
                return null;

        if(cutted && CurrentPageInfoArgs!.ProvidedTextLength == null)
            return null;

        if (Content != null)
            return GetPageContent(formatted, cutted);

        if (await TrySetPageInfoAndContent())
            return GetPageContent(formatted, cutted);

        return null;
    }

    private string GetPageContent(bool formatted, bool cutted)
    {
        string content = Content!;
        if(formatted)
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
        content = content.TrimStart();
        if (content.Length >= (int)textLength)
            return (content[..((int)textLength - 3)] + "...");
        return content.PadRight((int)textLength - content.Length, '.');
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
