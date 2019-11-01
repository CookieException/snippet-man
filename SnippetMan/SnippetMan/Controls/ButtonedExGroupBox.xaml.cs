using System.Windows;
using System.Windows.Controls;

namespace SnippetMan.Controls
{
    /// <summary>
    /// Interaktionslogik für ButtonedExGroupBox.xaml
    /// </summary>

    public partial class ButtonedExGroupBox : GroupBox
    {
        public static readonly DependencyProperty InnerContentProperty =
            DependencyProperty.Register("InnerContent", typeof(object), typeof(ButtonedExGroupBox),
                new UIPropertyMetadata(null));

        public object InnerContent
        {
            get { return GetValue(InnerContentProperty); }
            set { SetValue(InnerContentProperty, value); }
        }

        public static readonly DependencyProperty HeaderRightHandProperty =
            DependencyProperty.Register("HeaderRightHand", typeof(object), typeof(ButtonedExGroupBox),
                new UIPropertyMetadata(null));

        public object HeaderRightHand
        {
            get { return GetValue(HeaderRightHandProperty); }
            set { SetValue(HeaderRightHandProperty, value); }
        }


        public ButtonedExGroupBox()
        {
            InitializeComponent();
        }
    }
}
