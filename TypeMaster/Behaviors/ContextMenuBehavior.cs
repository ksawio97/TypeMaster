using Microsoft.Xaml.Behaviors;

namespace TypeMaster.Behaviors;

public class ContextMenuBehavior : Behavior<Button>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.Click += AssociatedObject_Click;
        AssociatedObject.ContextMenu.Closed += ContextMenu_Closed;
    }

    private void AssociatedObject_Click(object sender, RoutedEventArgs e)
    {
        AssociatedObject.ContextMenu.PlacementTarget = AssociatedObject;
        AssociatedObject.ContextMenu.IsOpen = true;
    }

    private void ContextMenu_Closed(object sender, RoutedEventArgs e)
    {
        var scope = FocusManager.GetFocusScope(AssociatedObject);
        FocusManager.SetFocusedElement(scope, null);
        Keyboard.ClearFocus();
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.Click -= AssociatedObject_Click;
    }
}
