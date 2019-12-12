using SnippetMan.Classes.Database;
using System.Windows;

namespace SnippetMan
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SQLiteDAO.Instance.CloseConnection();
        }
    }
}
