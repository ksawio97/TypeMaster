using System;

namespace TypeMaster.DataModels;

public class InlinesElementChangedEventArgs : EventArgs
{
    public readonly int OldInlineIndex;
    public readonly CharStylePack[] NewInlineStyles;

    public InlinesElementChangedEventArgs(int OldInlineIndex, CharStylePack[] NewInlineStyles)
    {
        this.OldInlineIndex = OldInlineIndex;
        this.NewInlineStyles = NewInlineStyles;
    }
}
