using SnippetMan.Classes;
using SnippetMan.Interface;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SnippetMan.Controls
{
    class SnippetNode : ITreeNode
    {
        private bool _isSelected;

        public string Title { get; set; }
        public ObservableCollection<ITreeNode> ChildNodes { get; set; } = new ObservableCollection<ITreeNode>();
        public SnippetInfo Tag { get; set; }
        public bool IsVisible { get; set; } = true;
        public bool IsGroup { get; set; } = false;

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public void setVisibility(Predicate<SnippetNode> visible, bool setOnChildNodes = true)
        {
            this.IsVisible = visible(this);

            OnPropertyChanged(nameof(IsVisible));

            if (!setOnChildNodes)
                return;

            foreach (SnippetNode child in this.ChildNodes)
                child.setVisibility(visible);
        }

        public void deselectAll()
        {
            this.IsSelected = false;

            foreach (SnippetNode child in this.ChildNodes)
                child.deselectAll();
        }

        public SnippetNode FindNode(Predicate<SnippetNode> predicate)
        {
            if (predicate(this))
                return this;

            foreach (SnippetNode child in this.ChildNodes)
            {
                if (child.FindNode(predicate) != default)
                {
                    return child;
                }
            }

            return default;
        }
    }
}
