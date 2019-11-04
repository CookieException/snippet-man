using System;
using System.Windows;
using System.Windows.Controls;
using HL.Interfaces;
using HL.Manager;
using SnippetMan.Controls.Utils;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Controls.Primitives;

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

            hlManager.SetCurrentTheme("VS2019_Dark");

            importEditor.SyntaxHighlighting = hlManager.CurrentTheme.GetDefinitionByExtension(".cs");
            codeEditor.SyntaxHighlighting = hlManager.CurrentTheme.GetDefinitionByExtension(".cs");                   
        }

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
        }

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
    }
}