namespace TypeMaster.Behaviors;

public class TitlebarResizeButtonBehavior : ResizingButtonBehavior
{
    string _minimalizeIcon = "";
    string _maximalizeIcon = "";

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_PreviewMouseLeftButtonUp;
        Application.Current.MainWindow.StateChanged += MainWindow_StateChanged;
    }

    private void AssociatedObject_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        ResizeWindow();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_PreviewMouseLeftButtonUp;
    }

    private void MainWindow_StateChanged(object? sender, System.EventArgs e)
    {
        //works if fontfamily is Segoe MDL2 Assets
        if(AssociatedObject.FontFamily.FamilyNames.Values.Contains("Segoe MDL2 Assets"))
            AssociatedObject.Content = Application.Current.MainWindow.WindowState == WindowState.Maximized ? _minimalizeIcon : _maximalizeIcon;
    }
}
