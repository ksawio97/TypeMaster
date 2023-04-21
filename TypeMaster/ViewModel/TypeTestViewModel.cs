using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
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

    private InlineCollection Inlines { get; set; }

    WikipediaService wikipediaService;

    string[] _wikiContent;
    int wordsCompleted = 0;

    public TypeTestViewModel(InlineCollection inlines)
    {
        wikipediaService = new WikipediaService();
        Inlines = inlines;

        LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            _wikiContent = (await wikipediaService.TryGetWikipediaPageAsync(600, "en")).Split(" ").Select(word => word + " ").ToArray();
        });

        SetInlines();
        IsBusy = false;
    }

    private void SetInlines()
    {
        foreach (var word in _wikiContent)
        {
            Inlines.Add(new Run(word) { Foreground = Brushes.Black });
        }
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
        if (ReplaceInlineAt(wordsCompleted, word))
            new IndexOutOfRangeException();
    }

    private bool ReplaceInlineAt(int index, Inline newElement)
    {
        var iter = Inlines.GetEnumerator();

        while (0 <= index--)
        {
            if (!iter.MoveNext())
                return false;
        }
        var newInlines = Inlines.Select(word => word == iter.Current ? newElement : word).ToArray();
        Inlines.Clear();
        Inlines.AddRange(newInlines);

        return true;
    }
}
