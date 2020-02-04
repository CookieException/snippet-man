using SnippetMan.Classes.Database;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SnippetMan
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(500)); // set's the initial delay in ms
            // Initialize and open database
            SQLiteDAO.Instance.OpenConnection();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Cleanup and close database
            SQLiteDAO.Instance.CloseConnection();
        }

    }
}
