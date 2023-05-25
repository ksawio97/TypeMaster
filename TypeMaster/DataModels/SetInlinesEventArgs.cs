using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace TypeMaster.DataModels;

public class SetInlinesEventArgs : EventArgs
{
    public readonly IEnumerable<Inline> Inlines;

    public SetInlinesEventArgs(IEnumerable<Inline> Inlines)
    {
        this.Inlines = Inlines;
    }
}