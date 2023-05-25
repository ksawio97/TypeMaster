using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Documents;

namespace TypeMaster.ViewModel;

public partial class TypeTestViewModel : AsyncViewModel
{
    #region observable properties
    [ObservableProperty]
    string? _userTypeInput;

    [ObservableProperty]
    private int _charLimit;

    [ObservableProperty]
    private string _infoForUser;
    #endregion
    public event EventHandler<InlinesElementChangedEventArgs> InlinesElementChanged;
    public event EventHandler<SetInlinesEventArgs> SetInlines;

    readonly INavigationService _navigationService;
    readonly CurrentPageService _currentPageService;
    readonly WikipediaService _wikipediaService;
    readonly ColorsService _colorsService;

    WikipediaPageInfo? _currWikiPageInfo;

    string[] _wikiContent;
    int _wordsCompleted;

    int _currWord;
    int _startIndex;
    readonly int _maxErrorCharsCount;

    readonly Timer _timeToStart;
    readonly Stopwatch _elapsedTypeTime;

    public TypeTestViewModel(WikipediaService wikipediaService, INavigationService navigationService, CurrentPageService currentPageService, ColorsService colorsService)
    {
        _navigationService = navigationService;
        _wikipediaService = wikipediaService;
        _currentPageService = currentPageService;
        _colorsService = colorsService;

        _wordsCompleted = 0;
        _currWord = 0;
        _startIndex = 0;
        _maxErrorCharsCount = 12;

        _elapsedTypeTime = new Stopwatch();
        _timeToStart = new Timer(100)
        {
            Enabled = false
        };
        _timeToStart.Elapsed += TimeToStart_Elapsed;

        _infoForUser = "Loading";
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "It is used to generate RelayCommand")]
    private async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        await Task.Run(async () =>
        {
            SearchResult? CurrWikiPageResult;
            (CurrWikiPageResult, string? content) = (await _currentPageService.TryGetPageResult(), await _currentPageService.TryGetPageContent(formatted: true, cutted: true));

            if (CurrWikiPageResult != null && content != null)
            {
                _wikiContent = content.Split(" ").Select(element => element + " ").ToArray();

                _currWikiPageInfo = _currentPageService.GetWikipediaPageInfo(_wikiContent.Length);

                await CountDownAsync(3);
                _timeToStart.Enabled = true;
                _elapsedTypeTime.Start();
            }
            else
            {
                Debug.WriteLine("couldn't get WikipediaPageInfo and content");
                _wikiContent = Array.Empty<string>();
            }
        });
        if(_wikiContent.Length != 0)
        {
            SetInlines(this, new SetInlinesEventArgs(_wikiContent.Select(word => new Run(word) { Foreground = _colorsService.TryGetColor("ForegroundColor") ?? Brushes.White })));
            SetCharLimit();
            CheckCurrentWord("", _wordsCompleted, (c1, c2) => _colorsService.TryGetColor("ForegroundColor") ?? Brushes.White);
        }

        IsBusy = false;
    }

    partial void OnUserTypeInputChanged(string? value)
    {
        string v = value ?? "";
        if (_wordsCompleted == _wikiContent.Length)
        {    
            _elapsedTypeTime.Stop();
            _timeToStart.Enabled = false;
            if (_currWikiPageInfo != null)
            {
                _currWikiPageInfo.SecondsSpent = (int)_elapsedTypeTime.Elapsed.TotalSeconds;
                _currWikiPageInfo.WPM = (int)((_wikiContent.Length / (double)_currWikiPageInfo.SecondsSpent) * 60);
                _wikipediaService.AddScore(_currWikiPageInfo);         
            }

            _navigationService.TryNavigateTo<ScoreboardViewModel>();
            return;
        }
        else if (value == _wikiContent[_wordsCompleted])
        {
            ReplaceInlineAt(_wordsCompleted, new[] { new CharStylePack(NewText: _wikiContent[_wordsCompleted++], NewForeground: _colorsService.TryGetColor("DarkCorrectColor") ?? Brushes.Green ) });
            _currWord++;
            UserTypeInput = "";
            if (_currWord != _wikiContent.Length)
                SetCharLimit();
        }
        else
        {
            CheckInput(v);
        }
    }
    
    private void SetCharLimit()
    {
        if (_currWord + 1 == _wikiContent.Length)
            CharLimit = _wikiContent[_currWord].Length;
        else
            CharLimit = _wikiContent[_currWord].Length + _maxErrorCharsCount;  
    }
    private void CheckInput(string input)
    {
        //go to next word
        if (input.Length > _startIndex + _wikiContent[_currWord].Length)
        {
            _startIndex += _wikiContent[_currWord].Length;
            _currWord++;

            CheckCurrentWord("", _currWord, (c1, c2) => _colorsService.TryGetColor("ForegroundColor") ?? Brushes.White);
        }
        //go one word back
        else if (_startIndex > input.Length)
        {
            _currWord--;
            _startIndex -= _wikiContent[_currWord].Length;

            ReplaceInlineAt(_currWord + 1, new[] { new CharStylePack(NewText: _wikiContent[_currWord + 1], NewForeground: _colorsService.TryGetColor("ForegroundColor") ?? Brushes.White) });
        }
        //if types word he isnt supposed to type color is yellow
        Func<char, char, SolidColorBrush> colorPick = _currWord == _wordsCompleted ? (c1, c2) => c1 == c2 ? _colorsService.TryGetColor("DarkCorrectColor") ?? Brushes.Green  : _colorsService.TryGetColor("DarkErrorColor") ?? Brushes.Yellow  : (c1, c2) => _colorsService.TryGetColor("DarkTypoColor") ?? Brushes.Yellow ;
        CheckCurrentWord(input[_startIndex..], _currWord, colorPick);
    }

    private void CheckCurrentWord(string input, int wordIndex, Func<char, char, SolidColorBrush> colorPick)
    {
        var colors = input[..(_wikiContent[wordIndex].Length < input.Length ? _wikiContent[wordIndex].Length : input.Length)].Select((c, i) => colorPick(c, _wikiContent[wordIndex][i])).ToArray();

        var wordStyles = _wikiContent[wordIndex].Select(
            (character, index) =>
                new CharStylePack(
                    NewText: character.ToString(),
                    NewForeground: index < colors.Length ? colors[index] : null,
                    NewBackground: index == colors.Length ? _colorsService.TryGetColor("DarkBackgroundColor") ?? Brushes.Purple : Brushes.Transparent,
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

    private void TimeToStart_Elapsed(object? sender, ElapsedEventArgs e)
    {
        int minutes = (int)_elapsedTypeTime.Elapsed.TotalMinutes;
        string minutesText = minutes != 0 ? minutes.ToString() + ":" : "";
        InfoForUser = $"Elapsed time: {minutesText}{_elapsedTypeTime.Elapsed.Seconds.ToString().PadLeft(2, '0')}";
    }

    [GeneratedRegex(" {2,}")]
    private partial Regex TwoOrMoreSpaces();
}
