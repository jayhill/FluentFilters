using System.Windows;
using System.Windows.Controls;

namespace FluentFilters
{
    /// <summary>
    /// Interaction logic for LabeledContent.xaml
    /// </summary>
    public partial class LabeledContent : ContentControl
    {
        public LabeledContent()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
            "LabelText", typeof (string), typeof (LabeledContent), new PropertyMetadata(default(string)));

        public string LabelText
        {
            get { return (string) GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }
    }
}
