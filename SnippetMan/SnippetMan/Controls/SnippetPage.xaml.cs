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
using TextBox = System.Windows.Controls.TextBox;
using System.Windows.Media.Animation;

namespace SnippetMan.Controls
{
    /// <summary>
    /// Interaktionslogik für SnippetPage.xaml
    /// </summary>
    public partial class SnippetPage : UserControl
    {
        //TODO: To be replaced by the snippets' actual tags
        private readonly string[] languages = { "C", "C++", "C#", "Java", "Python", "Javascript" };

        private readonly string[] tags = { "Windows", "Linux", "App", "Android App", "Gaming", "Cybersecurity", "Malware", "Bildverarbeitung", "EinTagNameDerWirklichGanzLangIst" };

        private TextEditor importEditor;
        private TextEditor codeEditor;
        private TextBox tb_title;
        private TextBox tb_description;
        private ToggleButton btn_edit_details;
        private ToggleButton btn_edit_import;
        private ToggleButton btn_edit_code;
        private Button btn_copy_import;
        private Button btn_copy_code;
        private Button btn_add_cmbx;
        private Button btn_del_cmbx;
        private WrapPanel wrapP_combx;
        private ComboBox combx_Tag0;
        private Popup popup_import;
        private Popup popup_code;

        public delegate void TitleChangedHandler(string Title);

        public event TitleChangedHandler TitleChanged;

        private List<ComboBox> comboBoxes = new List<ComboBox>();

        private IThemedHighlightingManager hlManager = ThemedHighlightingManager.Instance;

        public SnippetPage()
        {
            InitializeComponent();
            importEditor = (TextEditor)UIHelper.GetByUid(this, "ae_imports");
            importEditor.TextArea.Caret.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            codeEditor = (TextEditor)UIHelper.GetByUid(this, "ae_code");
            codeEditor.TextArea.Caret.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            tb_title = (TextBox)UIHelper.GetByUid(this, "tb_title");
            tb_description = (TextBox)UIHelper.GetByUid(this, "tb_description");
            btn_edit_details = (ToggleButton)UIHelper.GetByUid(this, "btn_edit_details");
            btn_edit_import = (ToggleButton)UIHelper.GetByUid(this, "btn_edit_import");
            btn_edit_code = (ToggleButton)UIHelper.GetByUid(this, "btn_edit_code");
            btn_copy_import = (Button)UIHelper.GetByUid(this, "btn_copy_import");
            btn_copy_code = (Button)UIHelper.GetByUid(this, "btn_copy_code");
            btn_add_cmbx = (Button)UIHelper.GetByUid(this, "btn_add_cmbx");
            btn_del_cmbx = (Button)UIHelper.GetByUid(this, "btn_del_cmbx");
            wrapP_combx = (WrapPanel)UIHelper.GetByUid(this, "wrapP_combx");
            combx_Tag0 = (ComboBox)UIHelper.GetByUid(this, "combx_Tag0");
            popup_import = (Popup)UIHelper.GetByUid(this, "popup_import");
            popup_code = (Popup)UIHelper.GetByUid(this, "popup_code");

            //Erste Combobox mit ProgLang
            foreach (string language in languages)
            {
                combx_Tag0.Items.Add(language);
            }
            comboBoxes.Add(combx_Tag0);

            hlManager.SetCurrentTheme("VS2019_Dark"); // TODO "{ }" einfärben

            importEditor.SyntaxHighlighting = hlManager.CurrentTheme.GetDefinitionByExtension(".cs");
            codeEditor.SyntaxHighlighting = hlManager.CurrentTheme.GetDefinitionByExtension(".cs");
        }

        #region Button Events

        private void ToggleButton_Edit_CheckChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == btn_edit_details)
            {
                if (btn_edit_details.IsChecked == true)
                {
                    tb_title.IsReadOnly = false;
                    tb_description.IsReadOnly = false;
                }
                else
                {
                    tb_title.IsReadOnly = true;
                    tb_description.IsReadOnly = true;
                }
            }
            else if (sender == btn_edit_import)
            {
                if (btn_edit_import.IsChecked == true)
                {
                    importEditor.IsReadOnly = false;
                    importEditor.TextArea.Caret.CaretBrush = null;
                }
                else
                {
                    importEditor.IsReadOnly = true;
                    importEditor.TextArea.Caret.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                }
            }
            else if (sender == btn_edit_code)
            {
                if (btn_edit_code.IsChecked == true)
                {
                    codeEditor.IsReadOnly = false;
                    codeEditor.TextArea.Caret.CaretBrush = null;
                }
                else
                {
                    codeEditor.IsReadOnly = true;
                    codeEditor.TextArea.Caret.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                }
            }
        }   // TODO Farbe des Icons (GeometryDrawing) soll sich ändern

        private void Button_Copy_Clicked(object sender, System.Windows.RoutedEventArgs e)
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
                Uid = "combx_Tag" + comboBoxes.Count,
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

            foreach (string tag in tags)
            {
                comboTag.Items.Add(tag);
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

        #endregion Button Events

        #region Keyboard Events

        private void CodeEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
            {
                if (btn_edit_code.IsChecked == false)
                {
                    SolidColorBrush grayBrush = new SolidColorBrush(Color.FromRgb(44, 44, 44));
                    SolidColorBrush blueBrush = new SolidColorBrush(Colors.LightBlue);
                    ColorAnimation animation = new ColorAnimation();
                    animation.From = grayBrush.Color;
                    animation.To = blueBrush.Color;
                    animation.Duration = new Duration(TimeSpan.FromSeconds(0.25));
                    animation.AutoReverse = true;
                    animation.RepeatBehavior = new RepeatBehavior(3);
                    animation.FillBehavior = FillBehavior.Stop;

                    Storyboard sb = new Storyboard();
                    Storyboard.SetTarget(animation, btn_edit_code);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("Background.(SolidColorBrush.Color)"));
                    sb.Children.Add(animation);
                    sb.Begin();
                }
            }
        }

        private void ImportEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
            {
                if (btn_edit_import.IsChecked == false)
                {
                    SolidColorBrush grayBrush = new SolidColorBrush(Color.FromRgb(44, 44, 44));
                    SolidColorBrush blueBrush = new SolidColorBrush(Colors.LightBlue);
                    ColorAnimation animation = new ColorAnimation();
                    animation.From = grayBrush.Color;
                    animation.To = blueBrush.Color;
                    animation.Duration = new Duration(TimeSpan.FromSeconds(0.25));
                    animation.AutoReverse = true;
                    animation.RepeatBehavior = new RepeatBehavior(3);
                    animation.FillBehavior = FillBehavior.Stop;

                    Storyboard sb = new Storyboard();
                    Storyboard.SetTarget(animation, btn_edit_import);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("Background.(SolidColorBrush.Color)"));
                    sb.Children.Add(animation);
                    sb.Begin();
                }
            }
        }

        private void TB_Details_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
            {
                if (btn_edit_details.IsChecked == false)
                {
                    SolidColorBrush grayBrush = new SolidColorBrush(Color.FromRgb(44, 44, 44));
                    SolidColorBrush blueBrush = new SolidColorBrush(Colors.LightBlue);
                    ColorAnimation animation = new ColorAnimation();
                    animation.From = grayBrush.Color;
                    animation.To = blueBrush.Color;
                    animation.Duration = new Duration(TimeSpan.FromSeconds(0.25));
                    animation.AutoReverse = true;
                    animation.RepeatBehavior = new RepeatBehavior(3);
                    animation.FillBehavior = FillBehavior.Stop;

                    Storyboard sb = new Storyboard();
                    Storyboard.SetTarget(animation, btn_edit_details);
                    Storyboard.SetTargetProperty(animation, new PropertyPath("Background.(SolidColorBrush.Color)"));
                    sb.Children.Add(animation);
                    sb.Begin();
                }
            }
        }

        #endregion Keyboard Events

        #region Funktionen zum Interagieren mit der Datenbank

        private void LoadInfo_Into_Details()
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            SnippetInfo si = database.GetSnippetMetaById(1);
            tb_title.Text = si.Titel;
            tb_description.Text = si.Beschreibung;

            //TODO: Add as many comboboxes as required by the tag list
            //TODO: DONE?
            //combx_Tag0.Text = si.Tags[0].ToString();
            //for (int i = 1; i < si.Tags.Length; i++)
            //{
            //    ComboBox comboTag = new ComboBox
            //    {
            //        Uid = "combx_Tag" + comboBoxes.Count,
            //        IsReadOnly = false,
            //        IsEditable = true
            //    };
            //    comboTag.Text = si.Tags[i].ToString();
            //    comboBoxes.Add(comboTag);
            //}

            //database.CloseConnection();
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
            //snippetInfo.Tags = comboBoxes.Select(c => c.SelectionBoxItem.ToString()).ToArray();
            database.saveSnippet(snippetInfo);
            database.CloseConnection();
        }

        private void SaveCode_Into_Database(SnippetCode snippetCode, SnippetInfo snippetInfo)
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            //snippetCode.Imports = importEditor.Text;
            //snippetCode.Code = codeEditor.Text;
            //database.saveSnippetCode(snippetCode, snippetInfo);
            database.CloseConnection();
        }

        #endregion Funktionen zum Interagieren mit der Datenbank

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TitleChanged?.Invoke(((TextBox)sender).Text);
        }
    }
}