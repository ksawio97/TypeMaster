using System;

namespace TypeMaster.Model;

[Serializable]
public class WikipediaPageInfo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int WPM { get; set; }

    public int Words { get; set; }

    public int AroundChars { get; set; }

    public string Language { get; set; }
}
