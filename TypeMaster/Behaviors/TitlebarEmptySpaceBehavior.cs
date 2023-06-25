namespace TypeMaster.Behaviors;

public class TitlebarEmptySpaceBehavior : ResizingButtonBehavior
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_PreviewMouseLeftButtonDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.MouseLeftButtonDown -= AssociatedObject_PreviewMouseLeftButtonDown;
    }

    private void AssociatedObject_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                var pos = Mouse.GetPosition(AssociatedObject);
                Point offset = new Point(pos.X / Application.Current.MainWindow.Width, pos.Y / Application.Current.MainWindow.Height);

                Application.Current.MainWindow.WindowState = WindowState.Normal;

                Application.Current.MainWindow.Left = pos.X - Application.Current.MainWindow.Width * offset.X;
                Application.Current.MainWindow.Top = pos.Y - Application.Current.MainWindow.Height * offset.Y;

            }
            Application.Current.MainWindow.DragMove();
        }
        else
            ResizeWindow();
    }
}
