using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TypeMaster.Behaviors;

namespace TypeMaster.ViewModel;

public partial class TypeTestViewModel : BaseViewModel
{
    #region observable properties
    [ObservableProperty]
    string? _userTypeInput;

    WikipediaService _wikipediaService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(Cursor))]
    private bool _isBusy;
    public bool IsNotBusy => !IsBusy;

    public Cursor Cursor => IsBusy ? Cursors.Wait : Cursors.Arrow;
    #endregion

    public event EventHandler<InlinesElementChangedEventArgs> InlinesElementChanged;
    public event EventHandler<SetInlinesEventArgs> SetInlines;

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

        SetInlines(this, new SetInlinesEventArgs(_wikiContent.Select(word => new Run(word) { Foreground = Brushes.Black })));
        CheckCurrentWord("", wordsCompleted, (c1, c2) => Brushes.Black);
        IsBusy = false;
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
            ReplaceInlineAt(wordsCompleted, new[] { new CharStylePack(NewText: _wikiContent[wordsCompleted++], NewForeground: Brushes.Green) });
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


        int currWord = wordsCompleted;
        int startIndex = 0;
        while(startIndex + _wikiContent[currWord].Length < input.Length)
        {
            startIndex += _wikiContent[currWord].Length;
            currWord++;
        }
        //if types word he isnt supposed to type color is yellow
        Func<char, char, SolidColorBrush> colorPick = currWord == wordsCompleted ? (c1, c2) => c1 == c2 ? Brushes.Green : Brushes.Red : (c1, c2) => Brushes.Yellow;
        CheckCurrentWord(input.Substring(startIndex, input.Length - startIndex), currWord, colorPick);

        //if deleted char from input and is on word edge
        if(input.Length - startIndex == _wikiContent[currWord].Length)
            CheckCurrentWord("", currWord + 1, (c1, c2) => Brushes.Black);
        else if (input.Length < lastInputLength && input.Length + 1 - startIndex == _wikiContent[currWord].Length)
            ReplaceInlineAt(currWord + 1, new[] { new CharStylePack(NewText: _wikiContent[currWord + 1], NewForeground: Brushes.Black) });
    }

    private void CheckCurrentWord(string input, int wordIndex, Func<char, char, SolidColorBrush> colorPick)
    {
        var colors = input.Substring(0, (_wikiContent[wordIndex].Length < input.Length ? _wikiContent[wordIndex].Length : input.Length)).Select((c, i) => colorPick(c, _wikiContent[wordIndex][i])).ToArray();

        var wordStyles = _wikiContent[wordIndex].Select(
            (character, index) =>
                new CharStylePack(
                    NewText: character.ToString(),
                    NewForeground: index < colors.Length ? colors[index] : null,
                    NewBackground: index == colors.Length ? Brushes.Purple : Brushes.Transparent,
                    NewTextDecorations: character == ' ' ? null : TextDecorations.Underline
                )
        ).ToArray();

        ReplaceInlineAt(wordIndex, wordStyles);
    }

    private void ReplaceInlineAt(int oldInlineIndex, CharStylePack[] wordStyles)
    {
        var args = new InlinesElementChangedEventArgs(oldInlineIndex, wordStyles);
        InlinesElementChanged(this, args);
    }
}

public struct CharStylePack
{
    public readonly string NewText;
    public readonly Brush NewForeground;
    public readonly Brush NewBackground;
    public readonly TextDecorationCollection? NewTextDecorations;

    public CharStylePack(string NewText, Brush? NewForeground = null, Brush? NewBackground = null, TextDecorationCollection? NewTextDecorations = null)
    {
        this.NewText = NewText;
        this.NewForeground = NewForeground == null ? Brushes.Black : NewForeground;
        this.NewBackground = NewBackground == null ? Brushes.Transparent : NewBackground;
        this.NewTextDecorations = NewTextDecorations;
    }
}