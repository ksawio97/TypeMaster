using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace TypeMaster.Behaviors;

public class InlinesBindingBehavior : Behavior<TextBlock>
{
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
            viewModel.SetInlines += ViewModel_SetInlines;
        }
    }

    private void ViewModel_InlinesElementChanged(object? sender, InlinesElementChangedEventArgs e)
    {
        Inline oldInline = AssociatedObject.Inlines.ElementAt(e.OldInlineIndex);
        Span? word = oldInline is Span ? (Span)oldInline : null;

        if (word != null && word.Inlines.Count == e.NewInlineStyles.Length)
        {
            if (e.NewInlineStyles.All(style => style.NewForeground == e.NewInlineStyles[0].NewForeground) && e.NewInlineStyles.All(style => style.NewBackground == e.NewInlineStyles[0].NewBackground))
            {
                ReplaceInline(oldInline, new Run
                {
                    Text = string.Concat(e.NewInlineStyles.Select(style => style.NewText)),
                    Foreground = e.NewInlineStyles[0].NewForeground,
                    Background = e.NewInlineStyles[0].NewBackground,
                    TextDecorations = e.NewInlineStyles[0].NewTextDecorations
                });
                return;
            }

            Run currInline = (Run)word.Inlines.FirstInline;
            
            for (int i = 0; i < e.NewInlineStyles.Length; i++, currInline = (Run)currInline.NextInline)
            {
                if (currInline.Text != e.NewInlineStyles[i].NewText)
                    currInline.Text = e.NewInlineStyles[i].NewText;

                if (currInline.Foreground != e.NewInlineStyles[i].NewForeground)
                    currInline.Foreground = e.NewInlineStyles[i].NewForeground;

                if (currInline.Background != e.NewInlineStyles[i].NewBackground)
                    currInline.Background = e.NewInlineStyles[i].NewBackground;

                if (currInline.TextDecorations != e.NewInlineStyles[i].NewTextDecorations)
                    currInline.TextDecorations = e.NewInlineStyles[i].NewTextDecorations;
            }
            return;
        }
        word = new Span();
        word.Inlines.AddRange(e.NewInlineStyles.Select(style =>
            new Run
            {
                Text = style.NewText,
                Foreground = style.NewForeground,
                Background = style.NewBackground,
                TextDecorations = style.NewTextDecorations
            }
        ));
        ReplaceInline(oldInline, word);
    }

    private void ReplaceInline(Inline toReplace, Inline NewValue)
    {
        AssociatedObject.Inlines.InsertBefore(toReplace, NewValue);
        AssociatedObject.Inlines.Remove(toReplace);
    }

    private void ViewModel_SetInlines(object? sender, SetInlinesEventArgs e)
    {
        AssociatedObject.Inlines.AddRange(e.Inlines);
    }
}

public class InlinesElementChangedEventArgs : EventArgs
{
    public int OldInlineIndex { get; }
    public CharStylePack[] NewInlineStyles { get; }

    public InlinesElementChangedEventArgs(int OldInlineIndex, CharStylePack[] NewInlineStyles)
    {
        this.OldInlineIndex = OldInlineIndex;
        this.NewInlineStyles = NewInlineStyles;
    }
}

public class SetInlinesEventArgs : EventArgs
{
    public IEnumerable<Inline> Inlines { get; }

    public SetInlinesEventArgs(IEnumerable<Inline> Inlines)
    {
        this.Inlines = Inlines;
    }
}