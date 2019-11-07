using System;
using System.Windows;
using System.Windows.Controls;
using HL.Interfaces;
using HL.Manager;
using SnippetMan.Controls.Utils;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Controls.Primitives;
using SnippetMan.Classes.Database;
using SnippetMan.Classes;

namespace SnippetMan.Controls
{
    /// <summary>
    /// Interaktionslogik für SnippetPage.xaml
    /// </summary>
    public partial class SnippetPage : UserControl
    {
        public TextEditor importEditor;
        public TextEditor codeEditor;
        public ToggleButton btn_edit_import;
        public ToggleButton btn_edit_code;
        public Button btn_copy_import;
        public Button btn_copy_code;
        public ComboBox combx_ProgLang;
        public ComboBox combx_Tag1;

        IThemedHighlightingManager hlManager = ThemedHighlightingManager.Instance;
        public SnippetPage()
        {
            InitializeComponent();
            importEditor = ((TextEditor)UIHelper.GetByUid(this, "ae_imports"));
            codeEditor = ((TextEditor)UIHelper.GetByUid(this, "ae_code"));
            btn_edit_import = ((ToggleButton)UIHelper.GetByUid(this, "btn_edit_import"));
            btn_edit_code = ((ToggleButton)UIHelper.GetByUid(this, "btn_edit_code"));
            btn_copy_import = ((Button)UIHelper.GetByUid(this, "btn_copy_import"));
            btn_copy_code = ((Button)UIHelper.GetByUid(this, "btn_copy_code"));
            combx_ProgLang = ((ComboBox)UIHelper.GetByUid(this, "combx_ProgLang"));
            combx_Tag1 = ((ComboBox)UIHelper.GetByUid(this, "combx_Tag1"));

            hlManager.SetCurrentTheme("VS2019_Dark"); //TODO "{ }" einfärben

            importEditor.SyntaxHighlighting = hlManager.CurrentTheme.GetDefinitionByExtension(".cs");
            codeEditor.SyntaxHighlighting = hlManager.CurrentTheme.GetDefinitionByExtension(".cs");                   
        }
        
        #region Events für Buttons
        private void ToggleButton_Edit_CheckChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == btn_edit_import)
            {
                importEditor.IsReadOnly = !btn_edit_import.IsChecked == true;
            }
            else if (sender == btn_edit_code)
            {
                codeEditor.IsReadOnly = !btn_edit_code.IsChecked == true;
            }
        }   // TODO Farbe des Icons (GeometryDrawing) soll sich ändern

        private void ToggleButton_Copy_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == btn_copy_import)
            {
                Clipboard.SetText(importEditor.Text);
            }
            else if (sender == btn_copy_code)
            {
                Clipboard.SetText(codeEditor.Text);
            }
        }
        #endregion

        #region Funktionen zum Interagieren mit der Datenbank
        private void LoadInfo_Into_Details()
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            SnippetInfo si = database.GetSnippetMetaById(1);
            combx_ProgLang.Text = si.Tags;
            combx_Tag1.Text = si.Tags;
            database.CloseConnection();
        }

        private void LoadData_Into_Editors()
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            SnippetInfo si = database.GetSnippetMetaById(1);
            SnippetCode snippetCode = database.GetSnippetCode(si);
            importEditor.Text = snippetCode.Imports;
            codeEditor.Text = snippetCode.Code;           
            database.CloseConnection();
        }

        private void SaveInfo_Into_Database(SnippetInfo snippetInfo)
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            snippetInfo.Tags = combx_ProgLang.Text += combx_Tag1.Text;

            database.saveSnippet(snippetInfo);
            database.CloseConnection();
        }

        private void SaveCode_Into_Database(SnippetCode snippetCode)
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            snippetCode.Imports = importEditor.Text;
            snippetCode.Code = codeEditor.Text;
            database.saveSnippetCode(snippetCode);
            database.CloseConnection();
        }
        #endregion
    }
}