namespace TypeMaster.DataModels;

public struct CharStylePack
{
    public readonly string NewText;
    public readonly Brush NewForeground;
    public readonly Brush NewBackground;
    public readonly TextDecorationCollection? NewTextDecorations;

    public CharStylePack(string NewText, Brush? NewForeground = null, Brush? NewBackground = null, TextDecorationCollection? NewTextDecorations = null)
    {
        this.NewText = NewText;
        this.NewForeground = NewForeground == null ? Brushes.Black : NewForeground;
        this.NewBackground = NewBackground == null ? Brushes.Transparent : NewBackground;
        this.NewTextDecorations = NewTextDecorations;
    }
}