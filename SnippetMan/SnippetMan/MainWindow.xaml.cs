using SnippetMan.Classes;
using SnippetMan.Classes.Database;
using SnippetMan.Interface;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;
using SnippetMan.Controls;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using BlurMessageBox;
using System.Collections.Generic;

namespace SnippetMan
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RangeObservableCollection<ITreeNode> shownSnippetMetaListGroups = new RangeObservableCollection<ITreeNode>();
        private const string TITLE_UNNAMED = "Untitled";

        public delegate void SnippetSavedHandler(SnippetInfo snippetInfo);
        public static event SnippetSavedHandler AnySnippetSaved;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize and open database
            SQLiteDAO.Instance.OpenConnection();

            // bind list to tree view 
            tv_snippetList.ItemsSource = shownSnippetMetaListGroups;

            tv_snippetList.SelectedItemChanged += Tv_snippetList_SelectedItemChanged;

            // set automatic refresh after sorts
            tv_snippetList.Items.IsLiveSorting = true;
            tv_snippetList.Items.IsLiveFiltering = true;

            // connect event handler on switching tabs to update the tree view selection
            tbc_pages.SelectionChanged += Tbc_pages_SelectionChanged;

            // trigger snippet tab changed event to get window title adjusted
            tbc_pages.SelectedIndex = 0;

            // initialize with unfiltered values
            refreshNodesAsync().ConfigureAwait(false);
        }

        #region Titlebar

        private void Btn_minapp_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Btn_toggleapp_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                ((FrameworkElement)Content).Margin = new Thickness(8); // in maximized mode, WPF forms seem to tend to be slightly too large - thus: increase margin
                this.WindowState = WindowState.Maximized;
                btn_toggleapp.Content = new Image { Source = (DrawingImage)Application.Current.Resources["window_restoreDrawingImage"] };
            }
            else
            {
                ((FrameworkElement)Content).Margin = new Thickness(0); // reset previously increased margin
                this.WindowState = WindowState.Normal;
                btn_toggleapp.Content = new Image { Source = (DrawingImage)Application.Current.Resources["window_maximizeDrawingImage"] };
            }
        }

        private void Btn_closeapp_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Save unsaved changes
            Application.Current.Shutdown();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #endregion Titlebar

        #region Tab Control
        private void Tbc_pages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedTab = tbc_pages.SelectedItem as TabItem;

            // due to a bug in wpf, we have to unselect the current item here already
            if (tv_snippetList.SelectedItem != null)
                (tv_snippetList.SelectedItem as SnippetNode).IsSelected = false;

            if (selectedTab?.Content is SnippetPage p)
            {
                // search for snippet in two levels
                SnippetNode snippetPosInTree = (SnippetNode)shownSnippetMetaListGroups.SelectMany(d => d.ChildNodes).FirstOrDefault(innernode => (innernode as SnippetNode).Tag == p.ShownSnippet);

                // if snippet was found..
                if (snippetPosInTree != null)
                {
                    // .. and unselect every node
                    shownSnippetMetaListGroups.ToList().ForEach(node => node.deselectAll());

                    // .. and set found node as selected.
                    snippetPosInTree.IsSelected = true;
                }

                if (String.IsNullOrEmpty(p.ShownSnippet.Titel))
                    lbl_title_snippetName.Content = TITLE_UNNAMED;
                else
                    lbl_title_snippetName.Content = p.ShownSnippet.Titel;
            }
            else
            {
                lbl_title_snippetName.Content = selectedTab?.Header ?? "";
            }

            // Adjust window title to show currently edited snippet name
            this.Title = "Snippet Man - " + lbl_title_snippetName.Content;
        }

        private void Ti_add_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            addSnippetPage();
            e.Handled = true;
        }

        /// <summary>
        /// Adds a new tab with the given snippet to the tabs
        /// </summary>
        /// <param name="si">Snippet that is to be opened. If this is null, a new one gets created by the SnippetPage</param>
        /// <param name="switchTo">If true, the new tab gets focused immediately</param>
        private void addSnippetPage(SnippetInfo si = null, bool switchTo = true)
        {
            TabItem tabItem = new TabItem
            {
                Header = si?.Titel ?? TITLE_UNNAMED
            };

            SnippetPage snippetPage = new SnippetPage(si);
            snippetPage.TitleChanged += SnippetPage_TitleChanged;
            snippetPage.SnippetSaved += SnippetPage_SnippetSaved;
            tabItem.Content = snippetPage;
            tbc_pages.Items.Remove(ti_add);
            tbc_pages.Items.Add(tabItem);
            tbc_pages.Items.Add(ti_add);

            if (switchTo)
                tbc_pages.SelectedIndex = tbc_pages.Items.Count - 2;
        }


        private void TB_del_Btn_Click(object sender, RoutedEventArgs e)
        {
            TabItem selectedTab = tbc_pages.SelectedItem as TabItem;

            // Mind. 1 offener Tab bleibt da
            if (tbc_pages.Items.Count - 2 != 0)
            {
                tbc_pages.Items.Remove(selectedTab);
                tbc_pages.SelectedIndex = tbc_pages.Items.Count - 2;
                e.Handled = true;
            }
            else
            {
                tbc_pages.Items.Remove(selectedTab);
                tbc_pages.Items.Remove(ti_add);
                tbc_pages.Items.Add(TB_Welcome);
                tbc_pages.Items.Add(ti_add);
                tbc_pages.SelectedIndex = tbc_pages.Items.Count - 2;
                e.Handled = true;
            }
        }
        #endregion Tab Control

        #region Changes inside the tab pages
        private void SnippetPage_TitleChanged(string Title)
        {
            TabItem tabItem = (TabItem)tbc_pages.SelectedItem;
            tabItem.Header = Title;

            // Adjust window title to show currently edited snippet name
            lbl_title_snippetName.Content = Title;
            this.Title = "Snippet Man - " + lbl_title_snippetName.Content;
        }

        private void SnippetPage_SnippetSaved(SnippetInfo snippetInfo)
        {
            // Relay event to any handler that is interested in changes
            AnySnippetSaved.Invoke(snippetInfo);

            // in case it was the title of a new snippet that was changed, refresh the tree view
            // TODO: Only refresh the new group or sth. like that for performance
            refreshNodesAsync().ConfigureAwait(false);

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        #endregion

        #region TreeView Nodes
        private void Tv_snippetList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SnippetNode selectedNode = (SnippetNode)e.NewValue;

            if (selectedNode == null)
                return;

            // check if page is already open, if so: Select it
            TabItem openTab = tbc_pages.Items.Cast<TabItem>().FirstOrDefault(t => t.Content is SnippetPage p && p.ShownSnippet == selectedNode.Tag);
            if (openTab != null)
            {
                openTab.IsSelected = true;
                return;
            }

            if (selectedNode != null && !selectedNode.IsGroup) // can happen e.g. on clearing the list
                addSnippetPage(selectedNode.Tag.withSnippetCodeUpdate());
        }

        private void Btn_delete_item_Click(object sender, RoutedEventArgs e)
        {
            // First: Ask the user if he is reaaaally sure
            if (this.MessageBoxShow("Do you want to delete this snippet?", "Confirmation", Buttons.YesNo, Icons.Warning, AnimateStyle.FadeIn) == System.Windows.Forms.DialogResult.No)
                return;

            SnippetNode selectedNode = ((Button)sender).DataContext as SnippetNode;

            TabItem openTab = tbc_pages.Items.Cast<TabItem>().FirstOrDefault(t => t.Content is SnippetPage p && p.ShownSnippet == selectedNode.Tag);

            if (openTab != null)
            {
                /* Check if to-be-deleted tab is currently selected..*/
                if ((tbc_pages.SelectedContent as SnippetPage).ShownSnippet == selectedNode.Tag)
                {
                    /* .. and select the previous one if that is the case */
                    tbc_pages.SelectedIndex -= 1;
                }

                /* Afterwards: Remove it */
                tbc_pages.Items.Remove(openTab);
            }

            /* .. and manually delete snippet from tree instead of refreshing the whole tree */
            shownSnippetMetaListGroups.FirstOrDefault(node => node.ChildNodes.Contains(selectedNode)).ChildNodes.Remove(selectedNode);

            SQLiteDAO.Instance.deleteSnippet(selectedNode.Tag);
        }

        private void btn_like_item_Click(object sender, RoutedEventArgs e)
        {
            SnippetNode selectedNode = ((ToggleButton)sender).DataContext as SnippetNode;
            selectedNode.Tag.Favorite = ((ToggleButton)sender).IsChecked ?? false;
            SQLiteDAO.Instance.saveSnippet(selectedNode.Tag);

            refreshNodesAsync().ConfigureAwait(false);
        }
        #endregion

        #region searchBar and TreeView itself
        private void Tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(tb_search.Text))
            {
                if (btn_eraseInput.Visibility == Visibility.Visible)
                    btn_eraseInput.Visibility = Visibility.Hidden;

            }
            else
            {
                if (btn_eraseInput.Visibility != Visibility.Visible)
                    btn_eraseInput.Visibility = Visibility.Visible;
            }

            foreach (SnippetNode node in shownSnippetMetaListGroups)
                node.setVisibility(n => n.IsGroup || shouldNodeShow(n.Tag, tb_search.Text));

            // hide empty nodes, e.g. if a search query doesn't match any item in a programming language
            foreach (SnippetNode node in shownSnippetMetaListGroups)
            {
                if (node.ChildNodes.Count(n => n.IsVisible) == 0)
                    node.setVisibility(n => false);
            }
        }

        private bool shouldNodeShow(SnippetInfo s, String filter = "")
        {
            return filter == "" || s.Titel.Contains(filter) || s.Tags.Any(t => t.Title.Contains(filter)) || s.Beschreibung.Contains(filter);
        }

        /// <summary>
        /// build treeview and its inner groups
        /// //TODO: Also call this if there was a database call. Maybe over a custom event?
        /// </summary>
        private async Task refreshNodesAsync()
        {
            string filter = tb_search.Text;

            // Maybe a bit of wait time for the database - thus: asynchronous
            List<ITreeNode> newList = await Task.Run(() =>
            {
                newList = new List<ITreeNode>();
                foreach (SnippetInfo s in SQLiteDAO.Instance.GetSnippetMetaList())
                {
                    ITreeNode currentGroup;
                    if (String.IsNullOrEmpty(s.ProgrammingLanguage))
                        currentGroup = newList.FirstOrDefault(n => n.HasEmptyTitle);
                    else
                        currentGroup = newList.FirstOrDefault(n => n.Title == s.ProgrammingLanguage);

                    // if a top node contains programming language..
                    if (currentGroup == null)
                    {
                        currentGroup = new SnippetNode() { Title = s.ProgrammingLanguage, IsGroup = true };


                        newList.Add(currentGroup);
                    }

                    // add the new node to that one if it shouldn't be hidden
                    currentGroup.ChildNodes.Add(new SnippetNode() { Title = s.Titel, Tag = s, IsVisible = shouldNodeShow(s, filter) });
                }

                // sort all group content
                foreach (ITreeNode group in newList)
                    group.ChildNodes = new ObservableCollection<ITreeNode>(group.ChildNodes.OrderByDescending(node => ((SnippetNode)node).Tag?.Favorite).ThenByDescending(node => ((SnippetNode)node).Tag?.LastEditDate));

                return newList;
            });

            // clear bound list before refreshing it and then
            // refresh it - assignment wouldn't work since the data binding would break
            shownSnippetMetaListGroups.AddRange(newList, true);

            // after refreshing, lookup if the currently selected tab needs to get a matching partner in the tree view again since all selections are cleared
            Tbc_pages_SelectionChanged(tv_snippetList, null);
        }

        private void Btn_eraseInput_Click(object sender, RoutedEventArgs e)
        {
            foreach (SnippetNode group in shownSnippetMetaListGroups)
                group.setVisibility(n => true);

            tb_search.Clear();
            btn_eraseInput.Focus();
        }
        #endregion
    }
}