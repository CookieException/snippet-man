using System.Windows;
using System.Windows.Controls;
using HL.Interfaces;
using HL.Manager;
using SnippetMan.Controls.Utils;
using ICSharpCode.AvalonEdit;
using System.Windows.Controls.Primitives;
using SnippetMan.Classes.Database;
using SnippetMan.Classes;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using UserControl = System.Windows.Controls.UserControl;
using TextBox = System.Windows.Controls.TextBox;
using SnippetMan.Classes.Snippets;
using System.Threading.Tasks;

namespace SnippetMan.Controls
{
    /// <summary>
    /// Interaktionslogik für SnippetPage.xaml
    /// </summary>
    public partial class SnippetPage : UserControl
    {
        private TextEditor importEditor;
        private TextEditor codeEditor;
        private TextBox tb_title;
        private TextBox tb_description;
        private Label lbl_changeDate;
        private Button btn_copy_import;
        private Button btn_copy_code;
        private ComboBox combx_Lang;
        private Popup popup_import;
        private Popup popup_code;

        private IThemedHighlightingManager hlManager = ThemedHighlightingManager.Instance;

        public RangeObservableCollection<Tag> SnippetTags { get; } = new RangeObservableCollection<Tag>();

        public RangeObservableCollection<Tag> ShownSnippetCustomTagPickList { get; private set; } = new RangeObservableCollection<Tag>();

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
            tb_title = (TextBox)UIHelper.GetByUid(this, "tb_title");
            tb_description = (TextBox)UIHelper.GetByUid(this, "tb_description");
            lbl_changeDate = (Label)UIHelper.GetByUid(this, "lbl_date");
            btn_copy_import = (Button)UIHelper.GetByUid(this, "btn_copy_import");
            btn_copy_code = (Button)UIHelper.GetByUid(this, "btn_copy_code");
            combx_Lang = (ComboBox)UIHelper.GetByUid(this, "combx_Lang");
            popup_import = (Popup)UIHelper.GetByUid(this, "popup_import");
            popup_code = (Popup)UIHelper.GetByUid(this, "popup_code");


            /* Both events are needed to trigger the syntax highlighting adjustment on selecting another language for the snippet */
            combx_Lang.SelectionChanged += Combx_Lang_SelectionChanged;
            combx_Lang.DropDownClosed += Combx_Lang_DropDownClosed;
            combx_Lang.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(ComboBox_TextChanged));

            combx_Lang.ItemsSource = SQLiteDAO.Instance.GetTags("", TagType.TAG_PROGRAMMING_LANGUAGE);

            hlManager.SetCurrentTheme("VS2019_Dark"); // TODO "{ }" einfärben

            MainWindow.AnySnippetSaved += MainWindow_AnySnippetSaved;

            if (si != null)
                LoadInfo_Into_Control(si);

            handleLanguageChange();
        }

        private void MainWindow_AnySnippetSaved(SnippetInfo snippetInfo)
        {
            /* if any snippet was saved, chances are that there has a new tag been saved. So: Refresh here the one not bound to the list */

            // save currently selected language as every database call lateron takes time
            Tag currentLanguage = combx_Lang.SelectedItem as Tag;
            // get new list
            combx_Lang.ItemsSource = SQLiteDAO.Instance.GetTags("", TagType.TAG_PROGRAMMING_LANGUAGE);
            // reset previous selected item
            combx_Lang.SelectedItem = currentLanguage;

            ShownSnippetCustomTagPickList.AddRange(SQLiteDAO.Instance.GetTags("", TagType.TAG_WITHOUT_TYPE), true);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => TitleChanged?.Invoke(((TextBox)sender).Text);

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e) => handleLanguageChange();
        private void Combx_Lang_DropDownClosed(object sender, System.EventArgs e) => handleLanguageChange();
        private void Combx_Lang_SelectionChanged(object sender, SelectionChangedEventArgs e) => handleLanguageChange();

        private void handleLanguageChange()
        {
            // SelectedItem can be null if the text is entered, but not a saved tag yet.
            string chosenLanguage = combx_Lang.SelectedItem?.ToString() ?? combx_Lang.Text;
            if (string.IsNullOrEmpty(chosenLanguage))
                return;

            importEditor.SyntaxHighlighting = LanguageThemeTranslator.GetHighlighterByLanguageName(hlManager.CurrentTheme, chosenLanguage);
            codeEditor.SyntaxHighlighting = LanguageThemeTranslator.GetHighlighterByLanguageName(hlManager.CurrentTheme, chosenLanguage);
        }

        #region Button Events

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

        private void Button_Add_Combx_Clicked(object sender, RoutedEventArgs e) => SnippetTags.Add(Classes.Snippets.Tag.EMPTY);

        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            // removing it from tag/cbx list causes the save call to not include the tag
            SnippetTags.Remove(((Button)sender).DataContext as Tag);

            SaveInfo_Into_DatabaseAsync(ShownSnippet).ConfigureAwait(false);
        }

        #endregion Button Events

        #region Funktionen zum Interagieren mit der Datenbank

        private void LoadInfo_Into_Control(SnippetInfo si)
        {
            /* load snippet information */
            tb_title.Text = si.Titel;
            tb_description.Text = si.Beschreibung;


            lbl_changeDate.Content = si.LastEditDate.ToString("dd.MM.yyyy H:mm");

            importEditor.Text = si.SnippetCode.Imports;
            codeEditor.Text = si.SnippetCode.Code;

            /* Prepare tag comboboxes */
            SnippetTags.AddRange(si.Tags.Where(t => t.Type != TagType.TAG_PROGRAMMING_LANGUAGE), true);
            ShownSnippetCustomTagPickList.AddRange(SQLiteDAO.Instance.GetTags("", TagType.TAG_WITHOUT_TYPE), true);

            if (si.Tags.Count != 0)
                combx_Lang.SelectedItem = si.Tags.FirstOrDefault(t => t.Type == TagType.TAG_PROGRAMMING_LANGUAGE);

            ShownSnippet = si;

            // notify the tab control that we have changed the title automatically
            TextBox_TextChanged(tb_title, null);
        }

        private bool pendingChange = false;
        private bool alreadyRunning = false;
        private async Task SaveInfo_Into_DatabaseAsync(SnippetInfo snippetInfo)
        {
            if (tb_title.Text == "")
                return;

            // prevent multiple parallel save operations on the same snippet, but always make sure the latest changes are saved
            // if there was a save request during the current, it will be handled after the current save task

            pendingChange = true; // request a rerun but..

            if (alreadyRunning) //.. don't start a simultaneous new task
            {
                return;
            }

            alreadyRunning = true;
            while (pendingChange)
            {
                pendingChange = false;

                snippetInfo.Titel = tb_title.Text;
                snippetInfo.Beschreibung = tb_description.Text;
                snippetInfo.Tags = new List<Tag>() { new Tag() { Title = combx_Lang.Text, Type = TagType.TAG_PROGRAMMING_LANGUAGE } };

                snippetInfo.Tags.AddRange(SnippetTags);
                snippetInfo.SnippetCode = new SnippetCode(null, importEditor.Text, codeEditor.Text);

                // Async because database access can make the gui be stuck for a moment
                snippetInfo = await Task.Run(() =>
                {
                    // override current page-local snippet info with potentially new info from the db (e.g. id), but not if something went wrong
                    snippetInfo = SQLiteDAO.Instance.saveSnippet(snippetInfo) ?? snippetInfo;
                    return snippetInfo;
                });

                // adjust change date to now
                lbl_changeDate.Content = snippetInfo.LastEditDate.ToString("dd.MM.yyyy H:mm");
            }
            alreadyRunning = false;

            SnippetSaved?.Invoke(snippetInfo);
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveInfo_Into_DatabaseAsync(ShownSnippet).ConfigureAwait(false);
        }

        #endregion Funktionen zum Interagieren mit der Datenbank
    }
}