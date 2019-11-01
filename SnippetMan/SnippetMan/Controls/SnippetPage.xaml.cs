using System;
using System.Windows.Controls;
using HL.Interfaces;
using HL.Manager;
using SnippetMan.Controls.Utils;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace SnippetMan.Controls
{
    /// <summary>
    /// Interaktionslogik für SnippetPage.xaml
    /// </summary>
    public partial class SnippetPage : UserControl
    {
        IThemedHighlightingManager hlManager = ThemedHighlightingManager.Instance;
        public SnippetPage()
        {
            InitializeComponent();

            var importEditor = ((TextEditor) UIHelper.GetByUid(this, "ae_imports"));
            var codeEditor = ((TextEditor) UIHelper.GetByUid(this, "ae_code"));

            importEditor.SyntaxHighlighting = hlManager.CurrentTheme.GetDefinitionByExtension(".cs");
            codeEditor.SyntaxHighlighting = hlManager.CurrentTheme.GetDefinitionByExtension(".cs");

        }
    }
}
