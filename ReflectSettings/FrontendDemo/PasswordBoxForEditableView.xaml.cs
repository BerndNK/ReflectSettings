using System.Windows;
using System.Windows.Controls;
using ReflectSettings.EditableConfigs;

namespace FrontendDemo
{
    /// <summary>
    /// Interaction logic for PasswordBoxForEditable.xaml
    /// </summary>
    public partial class PasswordBoxForEditableView : UserControl
    {
        public PasswordBox PasswordBox { get; set; }

        public static readonly DependencyProperty EditableSecureStringProperty = DependencyProperty.Register(
            "EditableSecureString", typeof(EditableSecureString), typeof(PasswordBoxForEditableView), new PropertyMetadata(default(EditableSecureString)));
        
        public EditableSecureString EditableSecureString
        {
            get => (EditableSecureString) GetValue(EditableSecureStringProperty);
            set => SetValue(EditableSecureStringProperty, value);
        }

        public PasswordBoxForEditableView()
        {
            PasswordBox = new PasswordBox();
            PasswordBox.PasswordChanged += PasswordBoxOnPasswordChanged;
            InitializeComponent();
        }

        private void PasswordBoxOnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (EditableSecureString == null)
                return;

            EditableSecureString.Value = PasswordBox.SecurePassword;
        }
    }
}
