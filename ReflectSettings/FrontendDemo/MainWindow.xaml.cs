using System;
using System.Linq;
using ReflectSettings.Factory;

namespace FrontendDemo
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var fac = new EditableConfigFactory();
            var result = fac.Produce(new ComplexConfiguration()).ToList();

            Console.WriteLine();
        }
    }
}
