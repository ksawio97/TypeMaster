﻿using CommunityToolkit.Mvvm.Input;
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

    INavigationService NavigationService { get; }
    CurrentPageService CurrentPageService { get; }
    WikipediaService WikipediaService { get; }
    ColorsService ColorsService { get; }

    WikipediaPageInfo? CurrWikiPageInfo;

    string[] _wikiContent;
    int wordsCompleted;

    int currWord;
    int startIndex;
    int MaxErrorCharsCount { get; }

    Timer Timer { get; }
    Stopwatch Stopwatch { get; }

    public TypeTestViewModel(WikipediaService wikipediaService, INavigationService navigationService, CurrentPageService currentPageService, ColorsService colorsService)
    {
        NavigationService = navigationService;
        WikipediaService = wikipediaService;
        CurrentPageService = currentPageService;
        ColorsService = colorsService;

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
        if (IsBusy) return;
        IsBusy = true;

        await Task.Run(async () =>
        {
            SearchResult? CurrWikiPageResult;
            (CurrWikiPageResult, string? content) = (await CurrentPageService.TryGetPageResult(), await CurrentPageService.TryGetPageContent(formatted: true, cutted: true));

            if (CurrWikiPageResult != null && content != null)
            {
                _wikiContent = content.Split(" ").Select(element => element + " ").ToArray();

                CurrWikiPageInfo = CurrentPageService.GetWikipediaPageInfo(_wikiContent.Length);

                await CountDownAsync(3);
                Timer.Enabled = true;
                Stopwatch.Start();
            }
            else
            {
                Debug.WriteLine("couldn't get WikipediaPageInfo and content");
                _wikiContent = Array.Empty<string>();
            }
        });
        if(_wikiContent.Length != 0)
        {
            SetInlines(this, new SetInlinesEventArgs(_wikiContent.Select(word => new Run(word) { Foreground = ColorsService.TryGetColor("ForegroundColor") ?? Brushes.White })));
            SetCharLimit();
            CheckCurrentWord("", wordsCompleted, (c1, c2) => ColorsService.TryGetColor("ForegroundColor") ?? Brushes.White);
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
                CurrWikiPageInfo.SecondsSpent = (int)Stopwatch.Elapsed.TotalSeconds;
                CurrWikiPageInfo.WPM = (int)((_wikiContent.Length / (double)CurrWikiPageInfo.SecondsSpent) * 60);
                WikipediaService.AddScore(CurrWikiPageInfo);         
            }

            NavigationService.TryNavigateTo<ScoreboardViewModel>();
            return;
        }
        else if (value == _wikiContent[wordsCompleted])
        {
            ReplaceInlineAt(wordsCompleted, new[] { new CharStylePack(NewText: _wikiContent[wordsCompleted++], NewForeground: ColorsService.TryGetColor("DarkCorrectColor") ?? Brushes.Green ) });
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

            CheckCurrentWord("", currWord, (c1, c2) => ColorsService.TryGetColor("ForegroundColor") ?? Brushes.White);
        }
        //go one word back
        else if (startIndex > input.Length)
        {
            currWord--;
            startIndex -= _wikiContent[currWord].Length;

            ReplaceInlineAt(currWord + 1, new[] { new CharStylePack(NewText: _wikiContent[currWord + 1], NewForeground: ColorsService.TryGetColor("ForegroundColor") ?? Brushes.White) });
        }
        //if types word he isnt supposed to type color is yellow
        Func<char, char, SolidColorBrush> colorPick = currWord == wordsCompleted ? (c1, c2) => c1 == c2 ? ColorsService.TryGetColor("DarkCorrectColor") ?? Brushes.Green  : ColorsService.TryGetColor("DarkErrorColor") ?? Brushes.Yellow  : (c1, c2) => ColorsService.TryGetColor("DarkTypoColor") ?? Brushes.Yellow ;
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
                    NewBackground: index == colors.Length ? ColorsService.TryGetColor("DarkBackgroundColor") ?? Brushes.Purple : Brushes.Transparent,
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
