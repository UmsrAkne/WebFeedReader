using System.Windows;
using System.Windows.Input;

namespace WebFeedReader.Controls
{
    public partial class CustomDesignToggleButton
    {
        public readonly static DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool?),
                typeof(CustomDesignToggleButton),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public readonly static DependencyProperty LabelProperty =
            DependencyProperty.Register(
                nameof(Label),
                typeof(string),
                typeof(CustomDesignToggleButton),
                new PropertyMetadata("Default Label"));

        public readonly static DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(CustomDesignToggleButton));

        public CustomDesignToggleButton()
        {
            InitializeComponent();
        }

        public bool? IsChecked { get => (bool?)GetValue(IsCheckedProperty); set => SetValue(IsCheckedProperty, value); }

        public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

        public ICommand Command { get => (ICommand)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    }
}