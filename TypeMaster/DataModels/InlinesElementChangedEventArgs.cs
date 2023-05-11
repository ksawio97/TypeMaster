using System;

namespace TypeMaster.DataModels;

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
