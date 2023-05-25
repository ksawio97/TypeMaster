using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TypeMaster.ViewModel;

partial class ChooseTextLengthViewModel : AsyncViewModel
{
    [ObservableProperty]
    ButtonBindableArgs _shortButtonBindableArgs;

    [ObservableProperty]
    ButtonBindableArgs _mediumButtonBindableArgs;

    [ObservableProperty]
    ButtonBindableArgs _longButtonBindableArgs;

    [ObservableProperty]
    string? _wikiTitle;

    readonly INavigationService _navigationService;
    readonly CurrentPageService _currentPageService;

    public ChooseTextLengthViewModel(INavigationService navigationService, CurrentPageService currentPageService)
    {
        _navigationService = navigationService;
        _currentPageService = currentPageService;

        ShortButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Short };
        MediumButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Medium };
        LongButtonBindableArgs = new ButtonBindableArgs { IsEnabled = false, RepresentedLength = TextLength.Long };
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        string? content = await _currentPageService.TryGetPageContent(formatted: true);
        if (content != null)
        {
            WikiTitle = (await _currentPageService.TryGetPageResult())?.Title;
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
        _currentPageService.CurrentPageInfoArgs!.ProvidedTextLength = textLength;
        _navigationService.TryNavigateTo<TypeTestViewModel>();
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