using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SnippetMan.Interface
{
    interface ITreeNode : INotifyPropertyChanged
    {
        /// <summary>
        /// Returns the display title of the node
        /// If the given title was empty, this might be a placeholder
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Returns whether this node is a group that doesn't contain real information beside a category
        /// </summary>
        bool IsGroup { get; set; }

        /// <summary>
        /// Returns whether the node should be displayed in the implementing tree view
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Returns whether the node is currently selected by the user
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Returns true if the originally given title was empty
        /// </summary>
        bool HasEmptyTitle { get; }

        /// <summary>
        /// Returns all child nodes this node contains
        /// </summary>
        ObservableCollection<ITreeNode> ChildNodes { get; set; }

        /// <summary>
        /// Unselects this node and every other child node this node has
        /// </summary>
        void deselectAll();
    }
}
