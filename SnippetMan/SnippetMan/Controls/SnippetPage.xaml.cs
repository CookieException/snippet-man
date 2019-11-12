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
using System.Windows.Media;
using System.Collections.Generic;

namespace SnippetMan.Controls
{
    /// <summary>
    /// Interaktionslogik für SnippetPage.xaml
    /// </summary>
    public partial class SnippetPage : UserControl
    {
        public TextEditor importEditor;
        public TextEditor codeEditor;
        public ToggleButton btn_edit_details;
        public ToggleButton btn_edit_import;
        public ToggleButton btn_edit_code;
        public Button btn_copy_import;
        public Button btn_copy_code;
        public Button btn_add_cmbx;
        public ComboBox combx_ProgLang;
        public ComboBox combx_Tag0;
        public WrapPanel wrapP_combx;

        public List<ComboBox> comboBoxes = new List<ComboBox>();
        public List<Button> del_btns = new List<Button>();
        
        IThemedHighlightingManager hlManager = ThemedHighlightingManager.Instance;
        public SnippetPage()
        {
            InitializeComponent();
            importEditor = ((TextEditor)UIHelper.GetByUid(this, "ae_imports"));
            codeEditor = ((TextEditor)UIHelper.GetByUid(this, "ae_code"));
            btn_edit_details = ((ToggleButton)UIHelper.GetByUid(this, "btn_copy_details"));
            btn_edit_import = ((ToggleButton)UIHelper.GetByUid(this, "btn_edit_import"));
            btn_edit_code = ((ToggleButton)UIHelper.GetByUid(this, "btn_edit_code"));
            btn_copy_import = ((Button)UIHelper.GetByUid(this, "btn_copy_import"));
            btn_copy_code = ((Button)UIHelper.GetByUid(this, "btn_copy_code"));
            btn_add_cmbx = ((Button)UIHelper.GetByUid(this, "btn_add_cmbx"));
            combx_ProgLang = ((ComboBox)UIHelper.GetByUid(this, "combx_ProgLang"));
            combx_Tag0 = ((ComboBox)UIHelper.GetByUid(this, "combx_Tag0"));
            wrapP_combx = ((WrapPanel)UIHelper.GetByUid(this, "wrapP_combx"));

            comboBoxes.Add(combx_Tag0);

            hlManager.SetCurrentTheme("VS2019_Dark"); // TODO "{ }" einfärben

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

        private void Button_Copy_Clicked(object sender, System.Windows.RoutedEventArgs e)
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

        private void Button_Add_Combx_Clicked(object sender, RoutedEventArgs e)
        {
            ComboBox comboTag = new ComboBox();
            comboTag.Uid = "combx_Tag" + comboBoxes.Count;
            comboTag.Items.Add("Tag1");
            comboTag.Items.Add("Tag2");
            comboBoxes.Add(comboTag);

            Button btn_del_combx = new Button();
            btn_del_combx.Uid = "btn_del_combxTag" + del_btns.Count;
            btn_del_combx.Margin = new Thickness(-5, 10, 10, 5);
            btn_del_combx.Width = 10;
            btn_del_combx.Click += Button_Del_Combx_Clicked;
            del_btns.Add(btn_del_combx);

            wrapP_combx.Children.Remove(btn_add_cmbx);
            wrapP_combx.Children.Add(comboBoxes[comboBoxes.Count - 1]);
            wrapP_combx.Children.Add(del_btns[del_btns.Count - 1]);
            wrapP_combx.Children.Add(btn_add_cmbx);
        }

        private void Button_Del_Combx_Clicked(object sender, RoutedEventArgs e)
        {
            wrapP_combx.Children.Remove(del_btns[del_btns.Count - 1]);
            wrapP_combx.Children.Remove(comboBoxes[comboBoxes.Count - 1]);

            del_btns.Remove(del_btns[del_btns.Count - 1]);
            comboBoxes.Remove(comboBoxes[comboBoxes.Count - 1]);
        }
        #endregion

        #region Funktionen zum Interagieren mit der Datenbank
        private void LoadInfo_Into_Details()
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            SnippetInfo si = database.GetSnippetMetaById(1);
            combx_ProgLang.Text = si.Tags;
            combx_Tag0.Text = si.Tags;
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
            snippetInfo.Tags = combx_ProgLang.Text += combx_Tag0.Text;

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