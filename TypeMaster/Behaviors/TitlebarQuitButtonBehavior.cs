using Microsoft.Xaml.Behaviors;

namespace TypeMaster.Behaviors;

public class TitlebarQuitButtonBehavior : Behavior<Button>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
    }

    private void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        Application.Current.Shutdown();
    }
}
