namespace TypeMaster.Behaviors;

public class TextBoxBehavior : DynamicFontBehavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        grid = GetParentOfType<Grid>(AssociatedObject);

        AssociatedObject.SizeChanged += AssociatedObject_SizeChanged;
        AssociatedObject.Loaded += AssociatedObject_Loaded;
        AssociatedObject.IsEnabledChanged += AssociatedObject_IsEnabledChanged;
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.SizeChanged -= AssociatedObject_SizeChanged;
        AssociatedObject.Loaded -= AssociatedObject_Loaded;
        AssociatedObject.IsEnabledChanged -= AssociatedObject_IsEnabledChanged;
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
    }

    private void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (grid != null && IsDifferenceBigEnough(e))
            AssociatedObject.FontSize = ChangeFontSize(AssociatedObject.FontSize, AssociatedObject.FontFamily.Source, VisualTreeHelper.GetDpi(AssociatedObject), e.NewSize.Width * e.NewSize.Height, AssociatedObject.Text.Length, e.NewSize);
    }

    public void CheckFocus(bool textBoxEnabled)
    {
        if (textBoxEnabled != AssociatedObject.IsFocused)
        {
            if (!AssociatedObject.IsFocused)
                AssociatedObject.Focus();
            else
                Keyboard.ClearFocus();
        }
    }

    private void AssociatedObject_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        CheckFocus(AssociatedObject.IsEnabled);
    }

    //prevents text selection
    private void AssociatedObject_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject.SelectionLength > 0)
            AssociatedObject.SelectionLength = 0;
    }

    private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
    {
        if (grid != null)
        {
            Size containerSize = GetGridContainerSize(grid);
            if (containerSize.Width > 0 && containerSize.Height > 0)
                AssociatedObject.FontSize = ChangeFontSize(AssociatedObject.FontSize, AssociatedObject.FontFamily.Source, VisualTreeHelper.GetDpi(AssociatedObject), containerSize.Width * containerSize.Height, AssociatedObject.Text.Length, containerSize);
        }
    }

}
