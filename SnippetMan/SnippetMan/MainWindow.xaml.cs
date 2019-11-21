﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SnippetMan
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

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

        private void Ti_add_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TabItem tabItem = new TabItem();
            tabItem.Header = "unnamed";
            Controls.SnippetPage snippetPage = new Controls.SnippetPage();
            snippetPage.TitleChanged += SnippetPage_TitleChanged;
            tabItem.Content = snippetPage;
            tbc_pages.Items.Remove(ti_add);
            tbc_pages.Items.Add(tabItem);
            tbc_pages.Items.Add(ti_add);
            tbc_pages.SelectedIndex = tbc_pages.Items.Count - 2;
            e.Handled = true;
        }

        private void SnippetPage_TitleChanged(string Title)
        {
            TabItem tabItem = (TabItem)tbc_pages.SelectedItem;
            tabItem.Header = Title;
        }

        private void SaveTab()
        {
            //TODO: Soll Tab bei z.B. wechsel durch Tabs Speichern
        }
    }
}
