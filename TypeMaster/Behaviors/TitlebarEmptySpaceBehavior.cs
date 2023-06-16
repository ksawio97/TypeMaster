namespace TypeMaster.Behaviors;

public class TitlebarEmptySpaceBehavior : ResizingButtonBehavior
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseDoubleClick += AssociatedObject_PreviewMouseDoubleClick;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseDoubleClick -= AssociatedObject_PreviewMouseDoubleClick;
    }

    private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if(Application.Current.MainWindow.WindowState == WindowState.Maximized)
        {
            var pos = Mouse.GetPosition(AssociatedObject);
            Point offset = new Point(pos.X / Application.Current.MainWindow.Width, pos.Y / Application.Current.MainWindow.Height);

            Application.Current.MainWindow.WindowState = WindowState.Normal;

            Application.Current.MainWindow.Left = pos.X - Application.Current.MainWindow.Width * offset.X;
            Application.Current.MainWindow.Top = pos.Y - Application.Current.MainWindow.Height * offset.Y;
        }

        Application.Current.MainWindow.DragMove();
    }

    private void AssociatedObject_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            ResizeWindow();
    }
}
