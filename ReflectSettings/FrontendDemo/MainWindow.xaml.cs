using ReflectSettings.Factory;

namespace FrontendDemo
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var fac = new EditableConfigFactory();
            fac.Produce(new ComplexConfiguration());
        }
    }
}
