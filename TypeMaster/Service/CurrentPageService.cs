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
            _wikipediaPageResult = null;
            Content = null;

            _currentPageInfoArgs = value;
        }
    }

    public bool IsCurrentPageInfoArgsNull => CurrentPageInfoArgs == null;

    SearchResult? _wikipediaPageResult { get; set; }

    public string? Content { get; private set; }

    readonly WikipediaService _wikipediaService;

    readonly LanguagesService _languagesService;

    public CurrentPageService(WikipediaService wikipediaService, LanguagesService languagesService)
    {
        _wikipediaService = wikipediaService;
        _languagesService = languagesService;
    }

    public async Task<SearchResult?> TryGetPageResult()
    {
        if (!_wikipediaPageResult.IsNullOrEmpty())
            return _wikipediaPageResult;

        if (await TrySetPageInfoAndContent())
            return _wikipediaPageResult;

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
        if (_wikipediaPageResult.IsNullOrEmpty() || CurrentPageInfoArgs == null || CurrentPageInfoArgs.ProvidedTextLength == null || wordsCount <= 0)
            return null;

        return new WikipediaPageInfo
        {
            Id = _wikipediaPageResult!.Id,
            Title = _wikipediaPageResult!.Title,
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

        (_wikipediaPageResult, Content) = await _wikipediaService.TryGetWikipediaPageInfoAsync(CurrentPageInfoArgs);

        return !(_wikipediaPageResult == null || Content == null);
    }

    string FormatPageContent(string content, string language)
    {
        content = _languagesService.FilterTextByLanguage(content, language);
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