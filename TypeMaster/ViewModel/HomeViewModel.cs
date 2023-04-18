using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace TypeMaster.ViewModel;

public partial class HomeViewModel : BaseViewModel
{
    [RelayCommand]
    public void ChangePage()
    {
        Page viewModel = new TypeTestView();
        var mainWindowViewModel = (MainViewModel)Application.Current.MainWindow.DataContext;
        mainWindowViewModel.WindowContent = viewModel;
    }
}
