using Microsoft.Xaml.Behaviors;

namespace TypeMaster.Behaviors;

public class TextBoxBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
    }

    //prevents text selection
    private void AssociatedObject_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (AssociatedObject.SelectionLength > 0)
            AssociatedObject.SelectionLength = 0;
    }
}
