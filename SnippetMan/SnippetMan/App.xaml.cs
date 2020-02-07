using SnippetMan.Classes.Database;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SnippetMan
{
    /// <summary>
    /// Interaktionslogik für "App.xaml" und für den Programmstart
    /// </summary>
    public partial class App : Application
    {
        public static IDatabaseDAO DatabaseInstance { get; set; } = new SQLiteDAO();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(Int32.MaxValue));
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(500)); // set's the initial delay in ms
            // Initialize and open database
            DatabaseInstance.OpenConnection();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Cleanup and close database
            DatabaseInstance.CloseConnection();
        }

    }
}
