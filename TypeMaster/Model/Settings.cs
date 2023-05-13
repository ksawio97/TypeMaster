using System;

namespace TypeMaster.Model;

[Serializable]
public class Settings
{
    public TextLength ProvidedTextLength { get; set; }
    public string CurrentLanguage { get; set; }
}
