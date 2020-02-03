using System.Windows;
using System.Windows.Controls;
using HL.Interfaces;
using HL.Manager;
using SnippetMan.Controls.Utils;
using ICSharpCode.AvalonEdit;
using System.Windows.Controls.Primitives;
using SnippetMan.Classes.Database;
using SnippetMan.Classes;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using UserControl = System.Windows.Controls.UserControl;
using TextBox = System.Windows.Controls.TextBox;
using SnippetMan.Classes.Snippets;
using System.Threading.Tasks;
using System;

namespace SnippetMan.Controls
{
    /// <summary>
    /// Interaktionslogik für SnippetPage.xaml
    /// </summary>
    public partial class SnippetPage : UserControl
    {
        private TextEditor importEditor;
        private TextEditor codeEditor;
        private Button btn_copy_import;
        private Button btn_copy_code;
        private ComboBox combx_Lang;
        private Popup popup_import;
        private Popup popup_code;

        private readonly IThemedHighlightingManager hlManager = ThemedHighlightingManager.Instance;

        public RangeObservableCollection<Tag> SnippetTags { get; } = new RangeObservableCollection<Tag>();

        public RangeObservableCollection<Tag> ShownSnippetCustomTagPickList { get; private set; } = new RangeObservableCollection<Tag>();
        public RangeObservableCollection<Tag> ShownSnippetLanguagePickList { get; private set; } = new RangeObservableCollection<Tag>();

        public delegate void TitleChangedHandler(string Title);
        public event TitleChangedHandler TitleChanged;

        public delegate void SnippetSavedHandler(SnippetInfo snippetInfo);
        public event SnippetSavedHandler SnippetSaved;

        private SnippetInfo shownSnippet;
        public SnippetInfo ShownSnippet
        {
            get
            {
                // if no snippet is currently loaded (aka. tab is empty), create a new one
                if (shownSnippet == null)
                    shownSnippet = new SnippetInfo();

                return shownSnippet;
            }

            set => shownSnippet = value;
        }

        /// <summary>
        /// Constructor for the snippet page
        /// </summary>
        /// <param name="si">If not null, the page gets initialized with this snippet. Otherwise it creates a new one</param>
        public SnippetPage(SnippetInfo si = null)
        {
            InitializeComponent();
            DataContext = this; // this has to be set manually, otherwise the binding on this instance isn't set

            importEditor = (TextEditor)UIHelper.GetByUid(this, "ae_imports");
            codeEditor = (TextEditor)UIHelper.GetByUid(this, "ae_code");
            btn_copy_import = (Button)UIHelper.GetByUid(this, "btn_copy_import");
            btn_copy_code = (Button)UIHelper.GetByUid(this, "btn_copy_code");
            combx_Lang = (ComboBox)UIHelper.GetByUid(this, "combx_Lang");
            popup_import = (Popup)UIHelper.GetByUid(this, "popup_import");
            popup_code = (Popup)UIHelper.GetByUid(this, "popup_code");


            hlManager.SetCurrentTheme("VS2019_Dark"); // TODO "{ }" einfärben

            MainWindow.AnySnippetSaved += MainWindow_AnySnippetSaved;

            if (si != null)
                LoadInfo_Into_Control(si);

            handleLanguageChange();
        }

        private void MainWindow_AnySnippetSaved(SnippetInfo snippetInfo)
        {
            /* if any snippet was saved, chances are that there has a new tag been saved. So: Refresh here the one not bound to the list */

            ShownSnippetCustomTagPickList.AddRange(SQLiteDAO.Instance.GetTags("", TagType.TAG_WITHOUT_TYPE), true);
            ShownSnippetLanguagePickList.AddRange(SQLiteDAO.Instance.GetTags("", TagType.TAG_PROGRAMMING_LANGUAGE), true);
        }

        private void handleLanguageChange()
        {
            // SelectedItem can be null if the text is entered, but not a saved tag yet.
            string chosenLanguage = combx_Lang.SelectedItem?.ToString() ?? combx_Lang.Text;
            if (string.IsNullOrEmpty(chosenLanguage))
                return;

            importEditor.SyntaxHighlighting = LanguageThemeTranslator.GetHighlighterByLanguageName(hlManager.CurrentTheme, chosenLanguage);
            codeEditor.SyntaxHighlighting = LanguageThemeTranslator.GetHighlighterByLanguageName(hlManager.CurrentTheme, chosenLanguage);

            SaveInfo_Into_DatabaseAsync(ShownSnippet).ConfigureAwait(false);
        }

        #region UI Events

        private void Button_Copy_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender == btn_copy_import)
            {
                Clipboard.SetText(importEditor.Text);
                popup_import.IsOpen = true;
            }
            else if (sender == btn_copy_code)
            {
                Clipboard.SetText(codeEditor.Text);
                popup_code.IsOpen = true;
            }
        }

        private void Button_Add_Combx_Clicked(object sender, RoutedEventArgs e) => ShownSnippet.Tags.Add(new Tag());

        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            ShownSnippet.Tags.Remove(((Button)sender).DataContext as Tag);

            SaveInfo_Into_DatabaseAsync(ShownSnippet).ConfigureAwait(false);
        }

        private void ComboBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            // only fire save if the new focus is not inside the same combobox area and not somewhere the updated list
            // forced it to be
            // this fires on: Left text box
            if (!(e.NewFocus is TextBox tb && tb.Parent is Grid g && g.TemplatedParent == e.OriginalSource) && !(e.NewFocus is MainWindow))
                SaveInfo_Into_DatabaseAsync(ShownSnippet).ConfigureAwait(false);
        }
        
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => TitleChanged?.Invoke(((TextBox)sender).Text);

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e) => handleLanguageChange();
        private void Combx_Lang_DropDownClosed(object sender, EventArgs e) => handleLanguageChange();
        private void Combx_Lang_SelectionChanged(object sender, SelectionChangedEventArgs e) => handleLanguageChange();
        
        private void UserControl_LostFocus(object sender, RoutedEventArgs e) => SaveInfo_Into_DatabaseAsync(ShownSnippet).ConfigureAwait(false);
        #endregion Button Events

        #region Funktionen zum Interagieren mit der Datenbank

        private void LoadInfo_Into_Control(SnippetInfo si)
        {
            ShownSnippet = si;

            /* load snippet information that can not be bound by xaml */
            importEditor.Text = si.SnippetCode.Imports;
            codeEditor.Text = si.SnippetCode.Code;

            /* Prepare tag combobox pick possibilities */
            ShownSnippetCustomTagPickList.AddRange(SQLiteDAO.Instance.GetTags("", TagType.TAG_WITHOUT_TYPE), true);
            ShownSnippetLanguagePickList.AddRange(SQLiteDAO.Instance.GetTags("", TagType.TAG_PROGRAMMING_LANGUAGE), true);

            // notify the tab control that we have changed the title automatically
            TextBox_TextChanged(ShownSnippet.Titel, null);
        }

        private bool pendingChange = false;
        private bool alreadyRunning = false;
        private async Task SaveInfo_Into_DatabaseAsync(SnippetInfo snippetInfo)
        {
            if (snippetInfo.Titel == "")
                return;

            // prevent multiple parallel save operations on the same snippet, but always make sure the latest changes are saved
            // if there was a save request during the current, it will be handled after the current save task

            pendingChange = true; // request a rerun but..

            if (alreadyRunning) //.. don't start a simultaneous new task
                return;

            alreadyRunning = true;
            while (pendingChange)
            {
                try
                {
                    pendingChange = false;
                    
                    // Assign snippet code manually because it's not possible to bind it by default
                    snippetInfo.SnippetCode = new SnippetCode(null, importEditor.Text, codeEditor.Text);

                    // Async because database access can make the gui be stuck for a moment
                    // Also: override current page-local snippet info with potentially new info from the db (e.g. id), but not if something went wrong
                    snippetInfo = await Task.Run(() => snippetInfo.save() ?? snippetInfo);
                }
                catch (Exception)
                {

                }
            }
            alreadyRunning = false;

            SnippetSaved?.Invoke(snippetInfo);
        }

        #endregion Funktionen zum Interagieren mit der Datenbank
    }
}