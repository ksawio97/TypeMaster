using Microsoft.Xaml.Behaviors;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace TypeMaster.Behaviors;

public class InlinesBindingBehavior : Behavior<TextBlock>
{
    public static readonly DependencyProperty InlinesCollectionProperty =
        DependencyProperty.Register(nameof(InlinesCollection), typeof(Inline[]), typeof(InlinesBindingBehavior), new PropertyMetadata(null, InlinesCollectionPropertyChanged));

    public Inline[] InlinesCollection
    {
        get { return (Inline[])GetValue(InlinesCollectionProperty); }
        set { SetValue(InlinesCollectionProperty, value); }
    }

    private static void InlinesCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = d as InlinesBindingBehavior;
        if (behavior != null)
        {
            behavior.InlinesCollectionChanged((Inline[])e.NewValue);
        }
    }

    private void InlinesCollectionChanged(Inline[] inlines)
    {

        if (AssociatedObject.Inlines.Count != inlines.Length)
        {
            AssociatedObject.Inlines.Clear();
            if (inlines != null)
            {
                foreach (var inline in inlines)
                {
                    AssociatedObject.Inlines.Add(inline);
                }
            }
        }
        else
        {
            for(int i = 0; i < inlines.Length; i++)
            {
                Inline word = AssociatedObject.Inlines.ElementAt(i);
                if (!word.Equals(inlines[i]))
                {
                    AssociatedObject.Inlines.InsertBefore(word, inlines[i]);
                    AssociatedObject.Inlines.Remove(word);
                }
            }
        }
    }
}