using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TypeMaster.ViewModel;

public partial class TypeTestViewModel : BaseViewModel
{
    #region observable properties
    [ObservableProperty]
    string? _userTypeInput;

    [ObservableProperty]
    private Inline[] _inlines;

    WikipediaService _wikipediaService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(Cursor))]
    private bool _isBusy;
    public bool IsNotBusy => !IsBusy;

    public Cursor Cursor => IsBusy ? Cursors.Wait : Cursors.Arrow;
    #endregion

    string[] _wikiContent;
    int wordsCompleted;
    int lastInputLength;

    public TypeTestViewModel(WikipediaService wikipediaService)
    {
        wordsCompleted = 0;
        lastInputLength = 0;
        _wikipediaService = wikipediaService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            _wikiContent = (await _wikipediaService.TryGetWikipediaPageAsync(600, "en")).Replace("\n", " ").Split(" ").Select(word => word.Trim() + " ").ToArray();
        });

        SetInlines();
        IsBusy = false;
    }

    private void SetInlines()
    {
        Inlines = _wikiContent.Select(word => new Run(word) { Foreground = Brushes.Black }).ToArray();
        CheckCurrentWord("", wordsCompleted, (c1, c2) => Brushes.Black);
    }

    partial void OnUserTypeInputChanged(string? value)
    {
        string v = value == null ? "" : value;
        if (wordsCompleted == _wikiContent.Length)
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
            CheckInput(v);
        }
        lastInputLength = v.Length;
    }
    
    //maybe add something that will change only changes that were added from last input (good for performence)
    private void CheckInput(string input)
    {

        Func<char, char, SolidColorBrush> colorPick = (c1, c2) => c1 == c2 ? Brushes.Green : Brushes.Red;

        int currWord = wordsCompleted;
        int startIndex = 0;
        while(startIndex + _wikiContent[currWord].Length < input.Length)
        {
            startIndex += _wikiContent[currWord].Length;
            currWord++;
        }
        CheckCurrentWord(input.Substring(startIndex, input.Length - startIndex), currWord, colorPick);

        //if deleted char from input and is on word edge
        if(input.Length - startIndex == _wikiContent[currWord].Length)
            CheckCurrentWord("", currWord + 1, (c1, c2) => Brushes.Black);
        else if (input.Length < lastInputLength && input.Length + 1 - startIndex == _wikiContent[currWord].Length)
            ReplaceInlineAt(currWord + 1, new Run(_wikiContent[currWord + 1]) { Foreground = Brushes.Black });
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