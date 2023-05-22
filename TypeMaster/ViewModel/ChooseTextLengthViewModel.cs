using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class ChooseTextLengthViewModel : AsyncViewModel
{
    [ObservableProperty]
    ButtonBindableArgs shortButtonBindableArgs;

    [ObservableProperty]
    ButtonBindableArgs mediumButtonBindableArgs;

    [ObservableProperty]
    ButtonBindableArgs longButtonBindableArgs;

    [ObservableProperty]
    string? wikiTitle;
    INavigationService NavigationService { get; }
    CurrentPageService CurrentPageService { get; }

    public ChooseTextLengthViewModel(INavigationService navigationService, CurrentPageService currentPageService)
    {
        NavigationService = navigationService;
        CurrentPageService = currentPageService;

        ShortButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Short };
        MediumButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Medium };
        LongButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Long };

        //CheckIfCanBeEnabled(0);
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        string? content = await CurrentPageService.TryGetPageContent(formatted: true);
        if (content != null)
        {
            WikiTitle = (await CurrentPageService.TryGetPageResult())?.Title;
            CheckIfCanBeEnabled(content.Length);
        }
        else
        {
            Debug.WriteLine("Couldn't get page content!");
        }

        IsBusy = false;
    }

    [RelayCommand]
    void NavigateToTypeTest(TextLength textLength)
    {
        CurrentPageService.CurrentPageInfoArgs!.ProvidedTextLength = textLength;
        NavigationService.TryNavigateTo<TypeTestViewModel>();
    }

    void CheckIfCanBeEnabled(int length)
    {
        ShortButtonBindableArgs.IsEnabled = (int)TextLength.Short <= length;
        MediumButtonBindableArgs.IsEnabled = (int)TextLength.Medium <= length;
        LongButtonBindableArgs.IsEnabled = (int)TextLength.Long <= length;
    }
}

public partial class ButtonBindableArgs : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Visibility))]
    bool isEnabled;

    [ObservableProperty]
    TextLength representedLength;
    public Visibility Visibility => IsEnabled ? Visibility.Visible : Visibility.Hidden;
}