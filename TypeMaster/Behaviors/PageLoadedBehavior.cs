using Microsoft.Xaml.Behaviors;

namespace TypeMaster.Behaviors;

public class PageLoadedBehavior : Behavior<Page>
{
    public static readonly DependencyProperty OnPageLoadCommandProperty =
        DependencyProperty.Register(nameof(OnPageLoadAsyncCommand), typeof(ICommand), typeof(PageLoadedBehavior), new PropertyMetadata(null, OnLoadAsyncCommandPropertyChanged));

    public ICommand OnPageLoadAsyncCommand
    {
        get { return (ICommand)GetValue(OnPageLoadCommandProperty); }
        set { SetValue(OnPageLoadCommandProperty, value); }
    }

    private static void OnLoadAsyncCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = d as PageLoadedBehavior;
        if (behavior != null)
        {
            behavior.OnLoadAsyncCommandChanged();
        }
    }

    private void OnLoadAsyncCommandChanged()
    {
        AssociatedObject.Loaded += (s, e) =>
        {
            OnPageLoadAsyncCommand.Execute(null);
        };
    }
}
