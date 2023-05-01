using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace TypeMaster.Behaviors;

public class TextBoxBehavior : Behavior<TextBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.IsEnabledChanged += AssociatedObject_IsEnabledChanged;
        AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.IsEnabledChanged -= AssociatedObject_IsEnabledChanged;
        AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
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
}
