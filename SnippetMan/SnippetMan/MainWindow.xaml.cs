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

namespace SnippetMan
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IDatabaseDAO database = new SQLiteDAO();
        private readonly ObservableCollection<ITreeNode> shownSnippetMetaListGroups = new ObservableCollection<ITreeNode>();

        public MainWindow()
        {
            InitializeComponent();

            // bind list to tree view 
            tv_snippetList.ItemsSource = shownSnippetMetaListGroups;

            // set automatic refresh after sorts
            tv_snippetList.Items.IsLiveSorting = true;
            tv_snippetList.Items.IsLiveFiltering = true;

            // initialize with unfiltered values
            refreshNodes();
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
            TabItem tabItem = new TabItem
            {
                Header = "unnamed"
            };
            Controls.SnippetPage snippetPage = new Controls.SnippetPage();
            snippetPage.TitleChanged += SnippetPage_TitleChanged;
            tabItem.Content = snippetPage;
            tbc_pages.Items.Remove(ti_add);
            tbc_pages.Items.Add(tabItem);
            tbc_pages.Items.Add(ti_add);
            tbc_pages.SelectedIndex = tbc_pages.Items.Count - 2;
            e.Handled = true;
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
        private bool shouldNodeHide(SnippetInfo s)
        {
            return tb_search.Text == "" || s.Titel.Contains(tb_search.Text);
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
                node.setVisibility(n => n.IsGroup || shouldNodeHide(n.Tag));
        }

        /// <summary>
        /// build treeview and its inner groups
        /// //TODO: Also call this if there was a database call. Maybe over a custom event?
        /// </summary>
        private void refreshNodes()
        {
            // clear bound list before
            shownSnippetMetaListGroups.Clear();

            database.OpenConnection();

            foreach (SnippetInfo s in database.GetSnippetMetaList())
            {
                ITreeNode currentGroup = shownSnippetMetaListGroups.FirstOrDefault(n => n.Title == s.ProgrammingLanguage);
                // if a top node contains programming language..
                if (currentGroup == null)
                {
                    currentGroup = new SnippetNode() { Title = s.ProgrammingLanguage, IsGroup = true };
                    shownSnippetMetaListGroups.Add(currentGroup);
                }

                // add the new node to that one if it shouldn't be hidden
                currentGroup.ChildNodes.Add(new SnippetNode() { Title = s.Titel, Tag = s, IsVisible = shouldNodeHide(s) });
            }

            database.CloseConnection();
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