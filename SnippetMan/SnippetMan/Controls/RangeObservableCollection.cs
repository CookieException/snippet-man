using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

/// <summary>
/// Like ObservableCollection, but with only one CollectionChanged Event
/// for a bulk adding process in order to improve performance.
/// Taken from https://peteohanlon.wordpress.com/2008/10/22/bulk-loading-in-observablecollection/ and extended by AddRange(list, clear)
/// </summary>
namespace SnippetMan.Controls
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool _suppressNotification = false;
        private SynchronizationContext context = SynchronizationContext.Current;

        public RangeObservableCollection() { }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Adds a list of items and clears the list before, if wanted
        /// </summary>
        public void AddRange(IEnumerable<T> list, bool clearBeforeAdd = false)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            _suppressNotification = true;

            if (clearBeforeAdd)
                Clear();

            foreach (T item in list)
            {
                Add(item);
            }
            _suppressNotification = false;

            // because this could theoretically be called in a non-ui thread: invoke notification on the correct thread

            if (System.Windows.Application.Current.Dispatcher != null)
                System.Windows.Application.Current.Dispatcher.Invoke(() => { OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)); });
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
