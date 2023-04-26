using Microsoft.Xaml.Behaviors;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace TypeMaster.Behaviors;

public class InlinesBindingBehavior : Behavior<TextBlock>
{
    public static readonly DependencyProperty InlinesCollectionProperty =
        DependencyProperty.Register("InlinesCollection", typeof(Inline[]), typeof(InlinesBindingBehavior), new PropertyMetadata(null, OnInlinesListPropertyChanged));

    public Inline[] InlinesCollection
    {
        get { return (Inline[])GetValue(InlinesCollectionProperty); }
        set { SetValue(InlinesCollectionProperty, value); }
    }

    private static void OnInlinesListPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = d as InlinesBindingBehavior;
        if (behavior != null)
        {
            behavior.OnInlinesListChanged((Inline[])e.NewValue);
        }
    }

    private void OnInlinesListChanged(Inline[] inlines)
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