using CommunityToolkit.Mvvm.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TypeMaster.Behaviors;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace TypeMaster.ViewModel;

public partial class TypeTestViewModel : BaseViewModel
{
    #region observable properties
    [ObservableProperty]
    string? _userTypeInput;

    [ObservableProperty]
    private int _charLimit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(Cursor))]
    private bool _isBusy;
    public bool IsNotBusy => !IsBusy;

    public Cursor Cursor => IsBusy ? Cursors.Wait : Cursors.Arrow;
    #endregion
    public event EventHandler<InlinesElementChangedEventArgs> InlinesElementChanged;
    public event EventHandler<SetInlinesEventArgs> SetInlines;

    WikipediaService _wikipediaService;

    string[] _wikiContent;
    int wordsCompleted;

    int currWord;
    int startIndex;
    int maxErrorCharsCount;
    public TypeTestViewModel(WikipediaService wikipediaService)
    {
        wordsCompleted = 0;
        _wikipediaService = wikipediaService;

        currWord = 0;
        startIndex = 0;
        maxErrorCharsCount = 12;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            
            var randomWikiPageInfo = await _wikipediaService.TryGetRandomWikipediaPageInfoAsync(600, "en");
            if(randomWikiPageInfo != null)
            {
                var content = await _wikipediaService.GetWikipediaPageContent(randomWikiPageInfo.Id, randomWikiPageInfo.AroundChars);
                if (content != null)
                {
                    _wikiContent = Regex.Replace(content.Replace("\n", " "), @" {2,}", " ").Split(" ").Select(word => word + " ").ToArray();
                    randomWikiPageInfo.AroundChars = _wikiContent.Length;
                }
                else
                {
                    Debug.WriteLine($"couldn't get content of page id{randomWikiPageInfo.Id}");
                }
            }
            
            if (_wikiContent == null)
            {
                _wikiContent = new string[0];
                Debug.WriteLine("couldn't get RandomWikipediaPageInfo");
            }
        });

        SetInlines(this, new SetInlinesEventArgs(_wikiContent.Select(word => new Run(word) { Foreground = Brushes.Black })));
        SetCharLimit();
        CheckCurrentWord("", wordsCompleted, (c1, c2) => Brushes.Black);
        IsBusy = false;
    }

    partial void OnUserTypeInputChanged(string? value)
    {
        string v = value ?? "";
        if (wordsCompleted == _wikiContent.Length)
        {
            //end of text implement later
            return;
        }
        else if (value == _wikiContent[wordsCompleted])
        {
            ReplaceInlineAt(wordsCompleted, new[] { new CharStylePack(NewText: _wikiContent[wordsCompleted++], NewForeground: Brushes.Green) });
            currWord++;
            UserTypeInput = "";
            SetCharLimit();
        }
        else
        {
            CheckInput(v);
        }
    }
    
    private void SetCharLimit()
    {
        if (currWord + 1 == _wikiContent.Length)
            CharLimit = _wikiContent[currWord].Length;
        else
            CharLimit = _wikiContent[currWord].Length + maxErrorCharsCount;
    }
    private void CheckInput(string input)
    {
        //go to next word
        if (input.Length > startIndex + _wikiContent[currWord].Length)
        {
            startIndex += _wikiContent[currWord].Length;
            currWord++;

            CheckCurrentWord("", currWord, (c1, c2) => Brushes.Black);
        }
        //go one word back
        else if (startIndex > input.Length)
        {
            currWord--;
            startIndex -= _wikiContent[currWord].Length;

            ReplaceInlineAt(currWord + 1, new[] { new CharStylePack(NewText: _wikiContent[currWord + 1], NewForeground: Brushes.Black) });
        }
        //if types word he isnt supposed to type color is yellow
        Func<char, char, SolidColorBrush> colorPick = currWord == wordsCompleted ? (c1, c2) => c1 == c2 ? Brushes.Green : Brushes.Red : (c1, c2) => Brushes.Yellow;
        CheckCurrentWord(input.Substring(startIndex, input.Length - startIndex), currWord, colorPick);
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