using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace TypeMaster.ViewModel;

public partial class TypeTestViewModel : BaseViewModel
{
    string? _wikiContent;

    [ObservableProperty]
    string? _userTypeInput;

    WikipediaService wikipediaService;

    public ObservableCollection<Inline> Inlines { get; set; }


    public TypeTestViewModel()
    {
        wikipediaService = new WikipediaService();
        Inlines = new ObservableCollection<Inline>();

        LoadDataAsync();
    }
    private async Task LoadDataAsync()
    {
        IsBusy = true;

        await Task.Run(async () =>
        {
            _wikiContent = await wikipediaService.TryGetWikipediaPageAsync(600, "en");
        });

        UpdateInline();
        IsBusy = false;
    }
    //TO DO make it more efficent you do not need to update whole text only chars in words
    partial void OnUserTypeInputChanged(string? value)
    {
        UpdateInline(value);
    }

    void UpdateInline(string? value = null)
    {
        string textInputed = value ?? string.Empty;
        string wikiContent = _wikiContent ?? string.Empty;
        if (0 < Inlines.Count)
            Inlines.Clear();
        Span word = new Span();
        for (int i = 0; i < wikiContent.Length; i++)
        {
            Brush foreground = Brushes.Black;
            if (i < textInputed.Length)
            {
                foreground = textInputed[i] == wikiContent[i] ? Brushes.Green : Brushes.Red;
            }

            Span character = new Span(new Run(wikiContent[i].ToString()) { Foreground = foreground, FontSize = 20 });
            if (wikiContent[i] == ' ')
            {
                Inlines.Add(word);
                Inlines.Add(character);
                word = new Span();

            }
            else
            {
                word.Inlines.Add(character);
            }
        }
        if (!word.Equals(new Span()))
            Inlines.Add(word);
    }
}
