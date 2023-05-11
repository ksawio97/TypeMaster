using System.Web;

namespace TypeMaster.DataModels;

public abstract class PageInfoArgs
{
    public int AroundChars { get; }
    public string Language { get; }

    protected PageInfoArgs(int aroundChars, string language)
    {
        AroundChars = aroundChars;
        Language = language;
    }

    public abstract string GetUrl();
}

public class RandomPageInfoArgs : PageInfoArgs
{
    public RandomPageInfoArgs(int aroundChars, string language)
        : base(aroundChars, language)
    {
    }

    public override string GetUrl()
    {
        return $"https://{Language}.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&explaintext=true&generator=random&grnnamespace=0&grnlimit=1&exchars={AroundChars}";
    }
}

public class IdPageInfoArgs : PageInfoArgs
{
    public int Id { get; }

    public IdPageInfoArgs(int id, int aroundChars, string language)
        : base(aroundChars, language)
    {
        Id = id;
    }

    public override string GetUrl()
    {
        return $"https://{Language}.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&explaintext=true&pageids={Id}&exchars={AroundChars}";
    }
}

public class TitlePageInfoArgs : PageInfoArgs
{
    public string Title { get; }

    public TitlePageInfoArgs(string title, int aroundChars, string language)
        : base(aroundChars, language)
    {
        Title = title;
    }

    public override string GetUrl()
    {
        return $"https://{Language}.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&exintro=true&explaintext=true&titles={HttpUtility.UrlEncode(Title)}&exchars={AroundChars}";
    }
}

