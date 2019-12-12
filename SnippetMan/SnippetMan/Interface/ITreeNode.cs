using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SnippetMan.Interface
{
    interface ITreeNode : INotifyPropertyChanged
    {
        string Title { get; set; }
        bool IsGroup { get; set; }
        bool IsVisible { get; set; }
        ObservableCollection<ITreeNode> ChildNodes { get; set; }


    }
}
