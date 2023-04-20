using System;
using System.Windows;

namespace TypeMaster;

public class InlineWrapPanel : Panel
{
    public static readonly DependencyProperty LineSpacingProperty = DependencyProperty.Register(
        "LineSpacing",
        typeof(double),
        typeof(InlineWrapPanel),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));

    public double LineSpacing
    {
        get { return (double)GetValue(LineSpacingProperty); }
        set { SetValue(LineSpacingProperty, value); }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // Measure each child, but constrain the available width to prevent overflow
        double width = 0;
        double height = 0;
        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
            if (width + child.DesiredSize.Width > availableSize.Width)
            {
                // If the child doesn't fit on the current line, wrap to a new line
                width = 0;
                height += child.DesiredSize.Height + LineSpacing; // add line spacing
            }
            width += child.DesiredSize.Width;
        }

        // Return the total desired size
        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        // Arrange each child on the appropriate line, wrapping as necessary
        double x = 0;
        double y = 0;
        double padding = 2; // set the desired padding between each inline element
        foreach (UIElement child in InternalChildren)
        {
            if (x + child.DesiredSize.Width > finalSize.Width)
            {
                x = 0;
                y += child.DesiredSize.Height + padding; // add padding between lines
            }
            child.Arrange(new Rect(x, y, child.DesiredSize.Width, child.DesiredSize.Height));
            x += child.DesiredSize.Width + padding; // add padding between inline elements
        }

        // Return the final arranged size
        return finalSize;
    }

}
