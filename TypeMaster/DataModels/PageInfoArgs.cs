using System.Web;

namespace TypeMaster.DataModels;

public abstract class PageInfoArgs
{
    public TextLength? ProvidedTextLength { get; set; }
    public string Language { get; }

    protected PageInfoArgs(TextLength? providedTextLength, string language)
    {
        ProvidedTextLength = providedTextLength;
        Language = language;
    }

    public abstract string GetUrl();
}

public class RandomPageInfoArgs : PageInfoArgs
{
    public RandomPageInfoArgs(TextLength? ProvidedTextLength, string language)
        : base(ProvidedTextLength, language)
    {
    }

    public override string GetUrl()
    {
        return $"https://{Language}.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro&explaintext&generator=random&grnnamespace=0&grnlimit=1";
    }
}

public class IdPageInfoArgs : PageInfoArgs
{
    public int Id { get; }

    public IdPageInfoArgs(int id, TextLength? ProvidedTextLength, string language)
        : base(ProvidedTextLength, language)
    {
        Id = id;
    }

    public override string GetUrl()
    {
        return $"https://{Language}.wikipedia.org/w/api.php?action=query&format=json&pageids={Id}&prop=extracts&exintro&explaintext";
    }
}

public class TitlePageInfoArgs : PageInfoArgs
{
    public string Title { get; }

    public TitlePageInfoArgs(string title, TextLength ProvidedTextLength, string language)
        : base(ProvidedTextLength, language)
    {
        Title = title;
    }

    public override string GetUrl()
    {
        return $"https://en.wikipedia.org/w/api.php?action=query&format=json&titles={HttpUtility.UrlEncode(Title)}&prop=extracts&exintro&explaintext";
    }
}

