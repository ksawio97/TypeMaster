using System;
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
    int lastWordChangedIndex;

    public TypeTestViewModel(WikipediaService wikipediaService)
    {
        _wikipediaService = wikipediaService;
        lastWordChangedIndex = 0;
        LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            _wikiContent = (await _wikipediaService.TryGetWikipediaPageAsync(600, "en")).Split(" ").Select(word => word.Trim() + " ").ToArray();
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
            CheckInput((value == null ? "" : value));
        }
    }
    
    //maybe add something that will change only changes that were added from last input (good for performence)
    private void CheckInput(string input)
    {

        Func<char, char, SolidColorBrush> colorPick = (c1, c2) => c1 == c2 ? Brushes.Green : Brushes.Red;

        int currWord = wordsCompleted;
        int charsLeft = input.Length;
        int startIndex = 0;
        while (0 < charsLeft)
        {
            if (charsLeft <= _wikiContent[currWord].Length)
            {
                CheckCurrentWord(input.Substring(startIndex, (startIndex + _wikiContent[currWord].Length < input.Length ? _wikiContent[currWord].Length : input.Length - startIndex)), currWord, colorPick);
            }
            charsLeft -= _wikiContent[currWord].Length;
            startIndex += _wikiContent[currWord].Length;
            currWord++;
        }
        lastWordChangedIndex = currWord - 1;
    }

    private void CheckCurrentWord(string input, int wordIndex, Func<char, char, SolidColorBrush> colorPick)
    {
        var colors = input.Substring(0, (_wikiContent[wordIndex].Length < input.Length ? _wikiContent[wordIndex].Length : input.Length)).Select((c, i) => colorPick(c, _wikiContent[wordIndex][i])).ToArray();

        Span word = new Span();
        for (int i = 0; i < _wikiContent[wordIndex].Length; i++)
        {
            Run character = new Run(_wikiContent[wordIndex][i].ToString());

            if (_wikiContent[wordIndex][i] != ' ')
                character.TextDecorations = TextDecorations.Underline;

            if (i < colors.Length)
            {
                character.Foreground = colors[i];
                character.Background = Brushes.Transparent;
            }
            else if (i == colors.Length)
            {
                character.Background = Brushes.Purple;
            }

            word.Inlines.Add(character);
        }
        ReplaceInlineAt(wordIndex, word);
    }

    //temporary solution
    private void ReplaceInlineAt(int index, Inline inline)
    {
        Inlines = Inlines.Select((word, i) => i == index ? inline : word).ToArray();
    }
}
