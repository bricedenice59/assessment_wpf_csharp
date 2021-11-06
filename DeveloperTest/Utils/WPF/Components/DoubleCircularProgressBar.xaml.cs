using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DeveloperTest.Utils.WPF.Components
{
    /// <summary>
    /// Interaction logic for DoubleCircularProgressBar.xaml
    /// </summary>
    public partial class DoubleCircularProgressBar : UserControl
    {
        public Brush StrokeColor
        {
            get { return (Brush)GetValue(StrokeColorProperty); }
            set { SetValue(StrokeColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CancelExportCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeColorProperty =
            DependencyProperty.Register("StrokeColor", typeof(Brush), typeof(DoubleCircularProgressBar), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0xBC, 0xC6, 0xD2))));

        public DoubleCircularProgressBar()
        {
            InitializeComponent();
        }
    }
}
