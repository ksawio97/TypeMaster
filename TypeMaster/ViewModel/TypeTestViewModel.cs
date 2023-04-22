using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace TypeMaster.ViewModel;

public partial class TypeTestViewModel : BaseViewModel
{

    [ObservableProperty]
    string? _userTypeInput;

    [ObservableProperty]
    private Inline[] _inlines;

    WikipediaService _wikipediaService;

    string[] _wikiContent;
    int wordsCompleted = 0;

    public TypeTestViewModel(WikipediaService wikipediaService)
    {
        _wikipediaService = wikipediaService;
        LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            _wikiContent = (await _wikipediaService.TryGetWikipediaPageAsync(600, "en")).Split(" ").Select(word => word + " ").ToArray();
        });

        SetInlines();
        IsBusy = false;
    }

    private void SetInlines()
    {
        Inlines = _wikiContent.Select(word => new Run(word) { Foreground = Brushes.Black }).ToArray();
    }

    partial void OnUserTypeInputChanged(string? value)
    {
        if(wordsCompleted == _wikiContent.Length)
        {
            //end of text implement later
            return;
        }
        else if (value == _wikiContent[wordsCompleted])
        {
            ReplaceInlineAt(wordsCompleted, new Run(_wikiContent[wordsCompleted++]) { Foreground = Brushes.Green });
            UserTypeInput = "";
        }
        else
        {
            CheckCurrentWord(value);
        }
    }

    private void CheckCurrentWord(string? value)
    {
        var colors = value.Substring(0, (_wikiContent[wordsCompleted].Length < value.Length ? _wikiContent[wordsCompleted].Length : value.Length)).Select((c, i) => (c == _wikiContent[wordsCompleted][i]) ? Brushes.Green : Brushes.Red).ToArray();
        Span word = new Span();
        for (int i = 0; i < _wikiContent[wordsCompleted].Length; i++)
        {
            Run character = new Run(_wikiContent[wordsCompleted][i].ToString());

            if (_wikiContent[wordsCompleted][i] != ' ')
                character.TextDecorations = TextDecorations.Underline;

            if (i < colors.Length)
            {
                character.Foreground = colors[i];
            }
            word.Inlines.Add(character);
        }
        ReplaceInlineAt(wordsCompleted, word);
    }

    //event not working properly so I need to do this
    private void ReplaceInlineAt(int index, Inline inline)
    {
        Inlines = Inlines.Select((word, i) => i == index ? inline : word).ToArray();
    }
}
