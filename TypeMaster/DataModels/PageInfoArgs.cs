using System.Web;

namespace TypeMaster.DataModels;

public abstract class PageInfoArgs
{
    public TextLength ProvidedTextLength { get; }
    public string Language { get; }

    protected PageInfoArgs(TextLength providedTextLength, string language)
    {
        ProvidedTextLength = providedTextLength;
        Language = language;
    }

    public abstract string GetUrl();
}

public class RandomPageInfoArgs : PageInfoArgs
{
    public RandomPageInfoArgs(TextLength ProvidedTextLength, string language)
        : base(ProvidedTextLength, language)
    {
    }

    public override string GetUrl()
    {
        return $"https://{Language}.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&explaintext=true&generator=random&grnnamespace=0&grnlimit=1&exchars={(int)ProvidedTextLength}";
    }
}

public class IdPageInfoArgs : PageInfoArgs
{
    public int Id { get; }

    public IdPageInfoArgs(int id, TextLength ProvidedTextLength, string language)
        : base(ProvidedTextLength, language)
    {
        Id = id;
    }

    public override string GetUrl()
    {
        return $"https://{Language}.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&explaintext=true&pageids={Id}&exchars={(int)ProvidedTextLength}";
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
        return $"https://{Language}.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&explaintext=true&titles={HttpUtility.UrlEncode(Title)}&exchars={(int)ProvidedTextLength}";
    }
}

