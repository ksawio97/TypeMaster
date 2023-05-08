using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Documents;
using TypeMaster.Behaviors;

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

    [ObservableProperty]
    private string _infoForUser;
    #endregion
    public event EventHandler<InlinesElementChangedEventArgs> InlinesElementChanged;
    public event EventHandler<SetInlinesEventArgs> SetInlines;

    INavigationService _navigationService;
    WikipediaService _wikipediaService;
    WikipediaPageInfo? currWikiPageInfo;

    string[] _wikiContent;
    int wordsCompleted;

    int currWord;
    int startIndex;
    int maxErrorCharsCount;

    Timer timer;
    Stopwatch stopwatch;

    public TypeTestViewModel(WikipediaService wikipediaService, INavigationService navigationService)
    {
        _navigationService = navigationService;
        _wikipediaService = wikipediaService;

        wordsCompleted = 0;
        currWord = 0;
        startIndex = 0;
        maxErrorCharsCount = 12;

        stopwatch = new Stopwatch();
        timer = new Timer(100);
        timer.Enabled = false;
        timer.Elapsed += Timer_Elapsed;

        _infoForUser = "Loading";
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            int aroundChars = 100;
            currWikiPageInfo = await _wikipediaService.TryGetRandomWikipediaPageInfoAsync(aroundChars, "en");
            if(currWikiPageInfo != null)
            {
                var content = await _wikipediaService.GetWikipediaPageContent(currWikiPageInfo.Id, currWikiPageInfo.AroundChars);
                if (content != null)
                {
                    _wikiContent = Regex.Replace(content.Replace("\n", " "), @" {2,}", " ").Split(" ").Select(word => word + " ").ToArray();
                    currWikiPageInfo.Words = _wikiContent.Length;
                    currWikiPageInfo.AroundChars = aroundChars;
                }
                else
                {
                    Debug.WriteLine($"couldn't get content of page id{currWikiPageInfo.Id}");
                }

                await CountDownAsync(3);
                timer.Enabled = true;
                stopwatch.Start();
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
            stopwatch.Stop();
            timer.Enabled = false;
            if (currWikiPageInfo != null)
            {
                currWikiPageInfo.WPM = (int)(_wikiContent.Length / stopwatch.Elapsed.TotalMinutes);
                _wikipediaService.AddScore(currWikiPageInfo);
            }

            _navigationService.NavigateTo<HomeViewModel>();
            return;
        }
        else if (value == _wikiContent[wordsCompleted])
        {
            ReplaceInlineAt(wordsCompleted, new[] { new CharStylePack(NewText: _wikiContent[wordsCompleted++], NewForeground: Brushes.Green) });
            currWord++;
            UserTypeInput = "";
            if (currWord != _wikiContent.Length)
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

    private async Task CountDownAsync(int seconds)
    {
        for (int i = seconds; i > 0; i--)
        {
            InfoForUser = i.ToString();
            await Task.Delay(1000);
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        int minutes = (int)stopwatch.Elapsed.TotalMinutes;
        string minutesText = minutes != 0 ? minutes.ToString() + ":" : "";
        InfoForUser = $"Elapsed time: {minutesText}{stopwatch.Elapsed.Seconds.ToString().PadLeft(2, '0')}";
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