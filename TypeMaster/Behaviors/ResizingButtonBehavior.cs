using Microsoft.Xaml.Behaviors;

namespace TypeMaster.Behaviors;

public class ResizingButtonBehavior : Behavior<Button>
{
    protected void ResizeWindow()
    {
        if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        else
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
    }
}
