namespace TypeMaster.Behaviors;

public class InfoForUserBehavior : DynamicFontBehavior<TextBlock>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        grid = GetParentOfType<Grid>(AssociatedObject);

        AssociatedObject.Loaded += AssociatedObject_Loaded;
        AssociatedObject.SizeChanged += AssociatedObject_SizeChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.Loaded -= AssociatedObject_Loaded;
        AssociatedObject.SizeChanged -= AssociatedObject_SizeChanged;
    }

    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        if (grid != null)
        {
            Size containerSize = GetGridContainerSize(grid);
            if(containerSize.Width > 0 && containerSize.Height > 0)
                AssociatedObject.FontSize = ChangeFontSize(AssociatedObject.FontSize, AssociatedObject.FontFamily.Source, VisualTreeHelper.GetDpi(AssociatedObject), containerSize.Width * containerSize.Height, AssociatedObject.Text.Length, containerSize);
        }
    }

    private void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (grid != null && IsDifferenceBigEnough(e))
            AssociatedObject.FontSize = ChangeFontSize(AssociatedObject.FontSize, AssociatedObject.FontFamily.Source, VisualTreeHelper.GetDpi(AssociatedObject), e.NewSize.Width * e.NewSize.Height, AssociatedObject.Text.Length, e.NewSize);
    }
}
