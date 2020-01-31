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
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SQLiteDAO.Instance.CloseConnection();
        }

    }
}
