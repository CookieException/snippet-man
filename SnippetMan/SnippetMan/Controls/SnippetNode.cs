using SnippetMan.Classes;
using SnippetMan.Interface;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SnippetMan.Controls
{
    class SnippetNode : ITreeNode
    {
        public string Title { get; set; }
        public ObservableCollection<ITreeNode> ChildNodes { get; set; } = new ObservableCollection<ITreeNode>();
        public SnippetInfo Tag { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsGroup { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public void setVisibility(Predicate<SnippetNode> visible)
        {
            this.IsVisible = visible(this);

            OnPropertyChanged(nameof(IsVisible));

            foreach (SnippetNode child in this.ChildNodes)
                child.setVisibility(visible);
        }
    }
}
