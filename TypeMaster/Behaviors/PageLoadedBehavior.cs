using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace TypeMaster.Behaviors;

public class PageLoadedBehavior : Behavior<Page>
{
    public static readonly DependencyProperty OnLoadAsyncCommandProperty =
        DependencyProperty.Register(nameof(OnLoadAsyncCommand), typeof(ICommand), typeof(PageLoadedBehavior), new PropertyMetadata(null, OnLoadAsyncCommandPropertyChanged));

    public ICommand OnLoadAsyncCommand
    {
        get { return (ICommand)GetValue(OnLoadAsyncCommandProperty); }
        set { SetValue(OnLoadAsyncCommandProperty, value); }
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
            OnLoadAsyncCommand.Execute(null);
        };
    }
}
