using SnippetMan.Classes;
using SnippetMan.Classes.Database;
using SnippetMan.Interface;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;
using SnippetMan.Controls;
using System.Threading.Tasks;

namespace SnippetMan
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<ITreeNode> shownSnippetMetaListGroups = new ObservableCollection<ITreeNode>();

        public MainWindow()
        {
            InitializeComponent();

            // Initialize and open database
            SQLiteDAO.Instance.OpenConnection();

            // bind list to tree view 
            tv_snippetList.ItemsSource = shownSnippetMetaListGroups;

            // bind click event on tree item
            tv_snippetList.SelectedItemChanged += Tv_snippetList_SelectedItemChanged;

            // set automatic refresh after sorts
            tv_snippetList.Items.IsLiveSorting = true;
            tv_snippetList.Items.IsLiveFiltering = true;

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
                Header = si?.Titel ?? "Unnamed"
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

        private void SnippetPage_TitleChanged(string Title)
        {
            TabItem tabItem = (TabItem)tbc_pages.SelectedItem;
            tabItem.Header = Title;
        }

        private void SnippetPage_SnippetSaved(SnippetInfo snippetInfo)
        {
            // in case it was the title of a new snippet that was changed, refresh the tree view
            // TODO: Only refresh the new group or sth. like that for performance
            refreshNodesAsync().ConfigureAwait(false);
        }

        private void SaveTab()
        {
            //TODO: Soll Tab bei z.B. wechsel durch Tabs Speichern
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #region searchBar and TreeView

        private void Tv_snippetList_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue != null && !((SnippetNode)e.NewValue).IsGroup) // can happen e.g. on clearing the list
                addSnippetPage(((SnippetNode)e.NewValue).Tag.withSnippetCodeUpdate());
        }

        private bool shouldNodeHide(SnippetInfo s, String filter = "")
        {
            return filter == "" || s.Titel.Contains(filter);
        }

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
                node.setVisibility(n => n.IsGroup || shouldNodeHide(n.Tag, tb_search.Text));
        }

        /// <summary>
        /// build treeview and its inner groups
        /// //TODO: Also call this if there was a database call. Maybe over a custom event?
        /// </summary>
        private async Task refreshNodesAsync()
        {
            string filter = tb_search.Text;

            // Maybe a bit of wait time for the database - thus: asynchronous
            ObservableCollection<ITreeNode> newList = await Task.Run(() =>
            {
                newList = new ObservableCollection<ITreeNode>();
                foreach (SnippetInfo s in SQLiteDAO.Instance.GetSnippetMetaList())
                {
                    ITreeNode currentGroup = newList.FirstOrDefault(n => n.Title == s.ProgrammingLanguage);
                    // if a top node contains programming language..
                    if (currentGroup == null)
                    {
                        currentGroup = new SnippetNode() { Title = s.ProgrammingLanguage, IsGroup = true };
                        newList.Add(currentGroup);
                    }

                    // add the new node to that one if it shouldn't be hidden
                    currentGroup.ChildNodes.Add(new SnippetNode() { Title = s.Titel, Tag = s, IsVisible = shouldNodeHide(s, filter) });
                }
                return newList;
            });

            // clear bound list before refreshing it
            shownSnippetMetaListGroups.Clear();

            // refresh it - assignment wouldn't work since the data binding would break
            foreach (ITreeNode n in newList)
                shownSnippetMetaListGroups.Add(n);

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