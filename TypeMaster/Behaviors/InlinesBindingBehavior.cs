using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace TypeMaster.Behaviors;

public class InlinesBindingBehavior : Behavior<TextBlock>
{
    public static readonly DependencyProperty InlinesProperty =
        DependencyProperty.Register(nameof(Inlines), typeof(List<Inline>), typeof(InlinesBindingBehavior), new PropertyMetadata(null, InlinesCollectionPropertyChanged));

    public List<Inline> Inlines
    {
        get { return (List<Inline>)GetValue(InlinesProperty); }
        set { SetValue(InlinesProperty, value); }
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;

    }

    private void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var viewModel = AssociatedObject.DataContext as TypeTestViewModel;

        if (viewModel != null)
        {
            viewModel.InlinesElementChanged += ViewModel_InlinesElementChanged;
        }
    }

    private void ViewModel_InlinesElementChanged(object? sender, InlinesElementChangedEventArgs e)
    {
        AssociatedObject.Inlines.InsertBefore(e.OldInline, e.NewInline);
        AssociatedObject.Inlines.Remove(e.OldInline);
    }

    private static void InlinesCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = d as InlinesBindingBehavior;
        if (behavior != null)
        {
            behavior.InlinesCollectionChanged((List<Inline>)e.NewValue);
        }
    }

    private void InlinesCollectionChanged(List<Inline> inlines)
    {

        if (AssociatedObject.Inlines.Count != inlines.Count)
        {
            AssociatedObject.Inlines.Clear();
            if (inlines != null)
            {
                AssociatedObject.Inlines.AddRange(inlines);
            }
        }
    }
}

public class InlinesElementChangedEventArgs : EventArgs
{
    public Inline OldInline { get; }
    public Inline NewInline { get; }

    public InlinesElementChangedEventArgs(Inline toChange, Inline NewInline)
    {
        OldInline = toChange;
        this.NewInline = NewInline;
    }
}