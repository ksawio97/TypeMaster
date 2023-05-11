using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace TypeMaster.DataModels;

public class SetInlinesEventArgs : EventArgs
{
    public IEnumerable<Inline> Inlines { get; }

    public SetInlinesEventArgs(IEnumerable<Inline> Inlines)
    {
        this.Inlines = Inlines;
    }
}