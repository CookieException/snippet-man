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
using System.Linq;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using Clipboard = System.Windows.Clipboard;
using ComboBox = System.Windows.Controls.ComboBox;
using UserControl = System.Windows.Controls.UserControl;

namespace SnippetMan.Controls
{
    /// <summary>
    /// Interaktionslogik für SnippetPage.xaml
    /// </summary>
    public partial class SnippetPage : UserControl
    {
        //TODO: To be replaced by the snippets' actual tags
        private readonly string[] languages = { "C", "C++", "C#", "Java", "Python", "Javascript" };

        private TextEditor importEditor;
        private TextEditor codeEditor;
        private ToggleButton btn_edit_details;
        private ToggleButton btn_edit_import;
        private ToggleButton btn_edit_code;
        private Button btn_copy_import;
        private Button btn_copy_code;
        private Button btn_add_cmbx;
        private Button btn_del_cmbx;
        private WrapPanel wrapP_combx;

        private List<ComboBox> comboBoxes = new List<ComboBox>();

        IThemedHighlightingManager hlManager = ThemedHighlightingManager.Instance;
        public SnippetPage()
        {
            InitializeComponent();
            importEditor = (TextEditor)UIHelper.GetByUid(this, "ae_imports");
            codeEditor = (TextEditor)UIHelper.GetByUid(this, "ae_code");
            btn_edit_details = (ToggleButton)UIHelper.GetByUid(this, "btn_copy_details");
            btn_edit_import = (ToggleButton)UIHelper.GetByUid(this, "btn_edit_import");
            btn_edit_code = (ToggleButton)UIHelper.GetByUid(this, "btn_edit_code");
            btn_copy_import = (Button)UIHelper.GetByUid(this, "btn_copy_import");
            btn_copy_code = (Button)UIHelper.GetByUid(this, "btn_copy_code");
            btn_add_cmbx = (Button)UIHelper.GetByUid(this, "btn_add_cmbx");
            btn_del_cmbx = (Button)UIHelper.GetByUid(this, "btn_del_cmbx");
            wrapP_combx = (WrapPanel)UIHelper.GetByUid(this, "wrapP_combx");

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
            ComboBox comboTag = new ComboBox
            {
                Uid = "combx_Tag" + comboBoxes.Count,
                IsReadOnly = false,
                IsEditable = true,
                Width = 60
            };

            Button btn_delete = new Button()
            {
                Width = 16,
                Height = 16,
                Margin = new Thickness(0, 10, 10, 5),
                Tag = comboTag,
                Foreground = new SolidColorBrush(Colors.White),
                Content = new Image { Source = (DrawingImage)System.Windows.Application.Current.Resources["remove_24pxDrawingImage"] }
            };

            btn_delete.Click += Btn_delete_Click;

            foreach (string language in languages)
            {
                comboTag.Items.Add(language);
            }
            comboBoxes.Add(comboTag);

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

        #endregion

        #region Funktionen zum Interagieren mit der Datenbank
        private void LoadInfo_Into_Details()
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            SnippetInfo si = database.GetSnippetMetaById(1);
            //TODO: Add as many comboboxes as required by the tag list
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

            snippetInfo.Tags = comboBoxes.Select(c => c.SelectionBoxItem.ToString()).ToArray();

            database.saveSnippet(snippetInfo);
            database.CloseConnection();
        }

        private void SaveCode_Into_Database(SnippetCode snippetCode)
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            snippetCode.Imports = importEditor.Text;
            snippetCode.Code = codeEditor.Text;
            //database.saveSnippetCode(snippetCode);
            database.CloseConnection();
        }
        #endregion

    }
}