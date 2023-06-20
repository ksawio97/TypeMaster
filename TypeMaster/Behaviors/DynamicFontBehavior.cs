using Microsoft.Xaml.Behaviors;
using System;
using System.Globalization;

namespace TypeMaster.Behaviors;

public class DynamicFontBehavior<T> : Behavior<T> where T : FrameworkElement
{
    protected Grid? grid { get; set; }
    protected Vector2 difference;
    protected readonly int minDifference;

    public DynamicFontBehavior()
    {
        difference = difference = new Vector2(0, 0);
        minDifference = 15;
    }

    protected bool IsDifferenceBigEnough(SizeChangedEventArgs e)
    {
        difference.X += e.NewSize.Width - e.PreviousSize.Width;
        difference.Y += e.NewSize.Height - e.PreviousSize.Height;
        return e.PreviousSize.Width != 0 && e.PreviousSize.Height != 0 && (Math.Abs(difference.X) > minDifference || Math.Abs(difference.Y) > minDifference);
    }

    bool IsCharacterSizeValid(Size containerSize, Size characterSize, double idealCharSize) => characterSize.Width < containerSize.Width && characterSize.Height < containerSize.Height && characterSize.Width * characterSize.Height < idealCharSize;
    protected double ChangeFontSize(double fontSize, string fontFamily, DpiScale dpi, double boxSize, int textLength, Size containerSize, bool oneLiner = false)
    {
        Size characterSize = CalculateCharSize(fontSize, fontFamily, dpi);
        double idealCharSize = boxSize / (textLength == 0 ? 1 : textLength);

        bool valid = IsCharacterSizeValid(containerSize, characterSize, idealCharSize);

        while (IsCharacterSizeValid(containerSize, characterSize, idealCharSize) == valid)
        {
            if (valid)
                fontSize += 1;
            else
                fontSize -= 1;

            characterSize = CalculateCharSize(fontSize, fontFamily, dpi);
        }
        return valid ? fontSize - 1 : fontSize + 1;
    }

    protected Size CalculateCharSize(double fontSize, string fontFamily, DpiScale dpi)
    {
        var formattedText = new FormattedText(
            "A",
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(fontFamily),
            fontSize,
            Brushes.Black,
            new NumberSubstitution(),
            TextFormattingMode.Ideal,
            dpi.PixelsPerDip);

        return new Size(formattedText.Width, formattedText.Height);
    }

    protected Size GetGridContainerSize(Grid grid)
    {
        int row = Grid.GetRow(AssociatedObject);
        int col = Grid.GetColumn(AssociatedObject);
        double width, height;

        if (grid.ColumnDefinitions.Count > 1 && col != -1)
            width = grid.ColumnDefinitions[col].ActualWidth;
        else
            width = grid.ActualWidth;

        if (grid.RowDefinitions.Count > 1 && row != -1)
            height = grid.RowDefinitions[row].ActualHeight;
        else
            height = grid.ActualHeight;

        return new Size(width, height);
    }

    protected T? GetParentOfType<T>(DependencyObject child) where T : DependencyObject
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);

        if (parent == null) return null;

        if (parent is T) return parent as T;

        return GetParentOfType<T>(parent);
    }
}