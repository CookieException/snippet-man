using SnippetMan.Classes;
using SnippetMan.Interface;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SnippetMan.Controls
{
    class SnippetNode : ITreeNode
    {
        private const string TITLE_UNNAMED = "Untitled";
        #region Properties

        #region ITreeNode properties
        private string _title;
        public string Title
        {
            get => HasEmptyTitle ? TITLE_UNNAMED : _title;
            set => _title = value;
        }

        public ObservableCollection<ITreeNode> ChildNodes { get; set; } = new ObservableCollection<ITreeNode>();

        public bool IsVisible { get; set; } = true;
        public bool IsGroup { get; set; } = false;

        private bool _isSelected;
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

        public bool HasEmptyTitle
        {
            get { return String.IsNullOrEmpty(_title); }
        }

        #endregion

        /// <summary>
        /// Contains the snippet information associated with this tree node
        /// </summary>
        public SnippetInfo Tag { get; set; }

        #endregion

        #region Event handlers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        protected void OnPropertyChanged(string propertyName) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        #endregion

        /// <summary>
        /// Sets the visibility of this node and, if given, all child nodes
        /// </summary>
        /// <param name="visible">True if node should be visible</param>
        /// <param name="setOnChildNodes">True if the change should be applied to all child nodes</param>
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

        /// <summary>
        /// Returns this node or a child node if it matches the given predicate
        /// </summary>
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
