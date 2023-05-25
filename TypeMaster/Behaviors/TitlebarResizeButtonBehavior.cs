namespace TypeMaster.Behaviors;

public class TitlebarResizeButtonBehavior : ResizingButtonBehavior
{
    string _minimalizeIcon = "";
    string _maximalizeIcon = "";

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
        Application.Current.MainWindow.StateChanged += MainWindow_StateChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
    }

    private void MainWindow_StateChanged(object? sender, System.EventArgs e)
    {
        //works if fontfamily is Segoe MDL2 Assets
        if(AssociatedObject.FontFamily.FamilyNames.Values.Contains("Segoe MDL2 Assets"))
            AssociatedObject.Content = Application.Current.MainWindow.WindowState == WindowState.Maximized ? _minimalizeIcon : _maximalizeIcon;
    }

    private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ResizeWindow();
    }
}
