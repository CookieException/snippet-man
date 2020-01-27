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
        private Button btn_copy_import;
        private Button btn_copy_code;
        private Button btn_add_cmbx;
        private Button btn_del_cmbx;
        private WrapPanel wrapP_combx;
        private ComboBox combx_Lang;
        private Popup popup_import;
        private Popup popup_code;

        private List<ComboBox> tagComboBoxes = new List<ComboBox>();
        private List<ComboBox> comboBoxesLang = new List<ComboBox>();

        private IThemedHighlightingManager hlManager = ThemedHighlightingManager.Instance;

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
            importEditor = (TextEditor)UIHelper.GetByUid(this, "ae_imports");
            codeEditor = (TextEditor)UIHelper.GetByUid(this, "ae_code");
            tb_title = (TextBox)UIHelper.GetByUid(this, "tb_title");
            tb_description = (TextBox)UIHelper.GetByUid(this, "tb_description");
            btn_copy_import = (Button)UIHelper.GetByUid(this, "btn_copy_import");
            btn_copy_code = (Button)UIHelper.GetByUid(this, "btn_copy_code");
            btn_add_cmbx = (Button)UIHelper.GetByUid(this, "btn_add_cmbx");
            btn_del_cmbx = (Button)UIHelper.GetByUid(this, "btn_del_cmbx");
            wrapP_combx = (WrapPanel)UIHelper.GetByUid(this, "wrapP_combx");
            combx_Lang = (ComboBox)UIHelper.GetByUid(this, "combx_Lang");
            popup_import = (Popup)UIHelper.GetByUid(this, "popup_import");
            popup_code = (Popup)UIHelper.GetByUid(this, "popup_code");


            /* Both events are needed to trigger the syntax highlighting adjustment on selecting another language for the snippet */
            combx_Lang.SelectionChanged += Combx_Lang_SelectionChanged;
            combx_Lang.DropDownClosed += Combx_Lang_DropDownClosed;
            combx_Lang.AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(ComboBox_TextChanged));

            combx_Lang.ItemsSource = SQLiteDAO.Instance.GetTags("", TagType.TAG_PROGRAMMING_LANGUAGE);
            comboBoxesLang.Add(combx_Lang);

            hlManager.SetCurrentTheme("VS2019_Dark"); // TODO "{ }" einfärben

            MainWindow.AnySnippetSaved += MainWindow_AnySnippetSaved;

            if (si != null)
                LoadInfo_Into_Control(si);

            handleLanguageChange();
        }

        private void MainWindow_AnySnippetSaved(SnippetInfo snippetInfo)
        {
            /* if any snippet was saved, chances are that there has a new tag been saved. So: Refresh here */

            /* language tag */

            // save currently selected language as every database call lateron takes time
            Tag currentLanguage = combx_Lang.SelectedItem as Tag;
            // get new list
            combx_Lang.ItemsSource = SQLiteDAO.Instance.GetTags("", TagType.TAG_PROGRAMMING_LANGUAGE);
            // reset previous selected item
            combx_Lang.SelectedItem = currentLanguage;

            /* other tags */

            // load currently possible tags
            List<Tag> tagList = SQLiteDAO.Instance.GetTags("", TagType.TAG_WITHOUT_TYPE);

            foreach (ComboBox cbx in tagComboBoxes)
            {
                Tag currentTag = cbx.SelectedItem as Tag;

                cbx.ItemsSource = tagList;

                cbx.SelectedItem = currentTag;
            }
        }

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (handle) handleLanguageChange();
            handle = true;
        }

        private bool handle = true;
        private void Combx_Lang_DropDownClosed(object sender, System.EventArgs e)
        {
            if (handle) handleLanguageChange();
            handle = true;
        }

        private void Combx_Lang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            handle = !cmb.IsDropDownOpen;
            handleLanguageChange();
        }

        private void handleLanguageChange()
        {
            // SelectedItem can be null if the text is entered, but not a saved tag yet.
            string chosenLanguage = combx_Lang.SelectedItem?.ToString() ?? combx_Lang.Text;
            if (string.IsNullOrEmpty(chosenLanguage))
                return;

            importEditor.SyntaxHighlighting = LanguageThemeTranslator.GetHighlighterByLanguageName(hlManager.CurrentTheme, chosenLanguage);
            codeEditor.SyntaxHighlighting = LanguageThemeTranslator.GetHighlighterByLanguageName(hlManager.CurrentTheme, chosenLanguage);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TitleChanged?.Invoke(((TextBox)sender).Text);
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

        private void Button_Add_Combx_Clicked(object sender, RoutedEventArgs e)
        {
            ComboBox comboTag = new ComboBox
            {
                Uid = "combx_Tag" + tagComboBoxes.Count,
                IsReadOnly = false,
                IsEditable = true
            };

            Button btn_delete = new Button()
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(-8, 0, 10, 3),
                Tag = comboTag,
                Foreground = new SolidColorBrush(Colors.White),
                Content = new Image { Source = (DrawingImage)System.Windows.Application.Current.Resources["remove_24pxDrawingImage"] }
            };

            btn_delete.Click += Btn_delete_Click;

            comboTag.ItemsSource = SQLiteDAO.Instance.GetTags("", TagType.TAG_WITHOUT_TYPE);

            tagComboBoxes.Add(comboTag);

            wrapP_combx.Children.Remove(btn_add_cmbx);
            wrapP_combx.Children.Add(comboTag);
            wrapP_combx.Children.Add(btn_delete);
            wrapP_combx.Children.Add(btn_add_cmbx);
        }

        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            wrapP_combx.Children.Remove((ComboBox)((Button)sender).Tag);

            wrapP_combx.Children.Remove((Button)sender);
        }

        #endregion Button Events

        #region Funktionen zum Interagieren mit der Datenbank

        private void LoadInfo_Into_Control(SnippetInfo si)
        {
            tb_title.Text = si.Titel;
            tb_description.Text = si.Beschreibung;

            for (int i = 1; i < si.Tags.Count; i++)
            {
                ComboBox comboTag = new ComboBox
                {
                    Uid = "combx_Tag" + tagComboBoxes.Count,
                    IsReadOnly = false,
                    IsEditable = true
                };

                comboTag.ItemsSource = SQLiteDAO.Instance.GetTags("", TagType.TAG_WITHOUT_TYPE);
                comboTag.SelectedItem = si.Tags[i];

                tagComboBoxes.Add(comboTag);


                Button btn_delete = new Button()
                {
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(-8, 0, 10, 3),
                    Tag = comboTag,
                    Foreground = new SolidColorBrush(Colors.White),
                    Content = new Image { Source = (DrawingImage)System.Windows.Application.Current.Resources["remove_24pxDrawingImage"] }
                };

                btn_delete.Click += Btn_delete_Click;

                wrapP_combx.Children.Remove(btn_add_cmbx);
                wrapP_combx.Children.Add(comboTag);
                wrapP_combx.Children.Add(btn_delete);
                wrapP_combx.Children.Add(btn_add_cmbx);
            }

            if (si.Tags.Count != 0)
                combx_Lang.SelectedItem = si.Tags.FirstOrDefault(t => t.Type == TagType.TAG_PROGRAMMING_LANGUAGE);

            importEditor.Text = si.SnippetCode.Imports;
            codeEditor.Text = si.SnippetCode.Code;

            ShownSnippet = si;

            // notify the tab control that we have changed the title automatically
            TextBox_TextChanged(tb_title, null);
        }

        private bool pendingChange = false;
        private bool alreadyRunning = false;
        private async Task SaveInfo_Into_DatabaseAsync(SnippetInfo snippetInfo)
        {
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
                snippetInfo.Tags = comboBoxesLang.Select(c => new Tag() { Title = c.Text, Type = TagType.TAG_PROGRAMMING_LANGUAGE }).ToList();
                snippetInfo.Tags.AddRange(tagComboBoxes.Select(c => new Tag() { Title = c.Text, Type = TagType.TAG_WITHOUT_TYPE }));
                snippetInfo.SnippetCode = new SnippetCode(null, importEditor.Text, codeEditor.Text);

                // Async because database access can make the gui be stuck for a moment
                snippetInfo = await Task.Run(() =>
                {
                    // override current page-local snippet info with potentially new info from the db (e.g. id), but not if something went wrong
                    snippetInfo = SQLiteDAO.Instance.saveSnippet(snippetInfo) ?? snippetInfo;
                    return snippetInfo;
                });
            }
            alreadyRunning = false;

            SnippetSaved?.Invoke(snippetInfo);
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (tb_title.Text != "")
                SaveInfo_Into_DatabaseAsync(ShownSnippet).ConfigureAwait(false);
        }

        #endregion Funktionen zum Interagieren mit der Datenbank
    }
}