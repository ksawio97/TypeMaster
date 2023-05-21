using Microsoft.Xaml.Behaviors;

namespace TypeMaster.Behaviors;

public class ContextMenuBehavior : Behavior<Button>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.Click += AssociatedObject_Click;
    }

    private void AssociatedObject_Click(object sender, RoutedEventArgs e)
    {
        AssociatedObject.ContextMenu.PlacementTarget = AssociatedObject;
        AssociatedObject.ContextMenu.IsOpen = true;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.MouseLeftButtonDown -= AssociatedObject_Click;
    }
}
