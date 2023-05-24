using Microsoft.Xaml.Behaviors;

namespace TypeMaster.Behaviors;

public class TitlebarMinimalizeButtonBehavior : Behavior<Button>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
    }

    private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Application.Current.MainWindow.WindowState = WindowState.Minimized;
    }
}
