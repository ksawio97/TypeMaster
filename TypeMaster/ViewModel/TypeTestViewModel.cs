using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Documents;

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

    INavigationService NavigationService { get; }
    WikipediaService WikipediaService { get; }
    WikipediaPageInfo? CurrWikiPageInfo;

    string[] _wikiContent;
    int wordsCompleted;

    int currWord;
    int startIndex;
    int MaxErrorCharsCount { get; }

    Timer Timer { get; }
    Stopwatch Stopwatch { get; }

    public TypeTestViewModel(WikipediaService wikipediaService, INavigationService navigationService)
    {
        NavigationService = navigationService;
        WikipediaService = wikipediaService;

        wordsCompleted = 0;
        currWord = 0;
        startIndex = 0;
        MaxErrorCharsCount = 12;

        Stopwatch = new Stopwatch();
        Timer = new Timer(100)
        {
            Enabled = false
        };
        Timer.Elapsed += Timer_Elapsed;

        _infoForUser = "Loading";
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            CurrWikiPageInfo = await WikipediaService.TryGetWikipediaPageInfoAsync();
            if(CurrWikiPageInfo != null)
            {
                var content = await WikipediaService.GetWikipediaPageContent();
                if (content != null)
                {
                    _wikiContent = TwoOrMoreSpaces().Replace(content.Replace("\n", " "), " ").Split(" ").Select(word => word + " ").SkipLast(1).ToArray();
                    CurrWikiPageInfo.Words = _wikiContent.Length;
                    CurrWikiPageInfo.ProvidedTextLength = WikipediaService.GetPageInfoArgs!.ProvidedTextLength;
                    await CountDownAsync(3);
                    Timer.Enabled = true;
                    Stopwatch.Start();
                }
                else
                {
                    Debug.WriteLine($"couldn't get content of page id{CurrWikiPageInfo.Id}");
                }
            }
            
            if (_wikiContent == null)
            {
                _wikiContent = Array.Empty<string>();
                Debug.WriteLine("couldn't get WikipediaPageInfo");
                //bad luck try again
                if (WikipediaService.GetPageInfoArgs! is RandomPageInfoArgs)
                {
                    var args = new RandomPageInfoArgs(WikipediaService.GetPageInfoArgs!.ProvidedTextLength, WikipediaService.GetPageInfoArgs!.Language);
                    NavigationService.NavigateTo<TypeTestViewModel>();
                }
            }
        });
        if(_wikiContent.Length != 0)
        {
            SetInlines(this, new SetInlinesEventArgs(_wikiContent.Select(word => new Run(word) { Foreground = Brushes.Black })));
            SetCharLimit();
            CheckCurrentWord("", wordsCompleted, (c1, c2) => Brushes.Black);
        }
        else
        {
            Debug.WriteLine("couldn't get WikipediaPageContent");
            NavigationService.NavigateTo<HomeViewModel>();
        }

        IsBusy = false;
    }

    partial void OnUserTypeInputChanged(string? value)
    {
        string v = value ?? "";
        if (wordsCompleted == _wikiContent.Length)
        {    
            Stopwatch.Stop();
            Timer.Enabled = false;
            if (CurrWikiPageInfo != null)
            {
                CurrWikiPageInfo.WPM = (int)(_wikiContent.Length / (Stopwatch.Elapsed.TotalMinutes % 0.01));
                CurrWikiPageInfo.SecondsSpent = (int)Stopwatch.Elapsed.TotalSeconds;
                WikipediaService.AddScore(CurrWikiPageInfo);
            }

            NavigationService.NavigateTo<HomeViewModel>();
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
            CharLimit = _wikiContent[currWord].Length + MaxErrorCharsCount;  
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
        CheckCurrentWord(input[startIndex..], currWord, colorPick);
    }

    private void CheckCurrentWord(string input, int wordIndex, Func<char, char, SolidColorBrush> colorPick)
    {
        var colors = input[..(_wikiContent[wordIndex].Length < input.Length ? _wikiContent[wordIndex].Length : input.Length)].Select((c, i) => colorPick(c, _wikiContent[wordIndex][i])).ToArray();

        var wordStyles = _wikiContent[wordIndex].Select(
            (character, index) =>
                new CharStylePack(
                    NewText: character.ToString(),
                    NewForeground: index < colors.Length ? colors[index] : null,
                    NewBackground: index == colors.Length ? Brushes.Purple : Brushes.Transparent,
                    NewTextDecorations: character == ' ' && index >= colors.Length ? null : TextDecorations.Underline
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
        int minutes = (int)Stopwatch.Elapsed.TotalMinutes;
        string minutesText = minutes != 0 ? minutes.ToString() + ":" : "";
        InfoForUser = $"Elapsed time: {minutesText}{Stopwatch.Elapsed.Seconds.ToString().PadLeft(2, '0')}";
    }

    [GeneratedRegex(" {2,}")]
    private partial Regex TwoOrMoreSpaces();
}
