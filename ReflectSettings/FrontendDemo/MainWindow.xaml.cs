using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using JetBrains.Annotations;
using Newtonsoft.Json;
using ReflectSettings;
using ReflectSettings.EditableConfigs;

namespace FrontendDemo
{
    public partial class MainWindow
    {
        private readonly ComplexConfiguration _complexConfiguration;
        [UsedImplicitly]
        public ObservableCollection<IEditableConfig> Editables { get; set; }

        private const string JsonFilePath = "Config.json";

        public MainWindow()
        {
            DataContext = this;
            Editables = new ObservableCollection<IEditableConfig>();
            InitializeComponent();
            var fac = new SettingsFactory();
            _complexConfiguration = new ComplexConfiguration();
            if (File.Exists(JsonFilePath))
                _complexConfiguration = JsonConvert.DeserializeObject<ComplexConfiguration>(File.ReadAllText(JsonFilePath));
            
            var changeTrackingManager = new ChangeTrackingManager();
            var editableConfigs = fac.Reflect(_complexConfiguration, changeTrackingManager);

            foreach (var config in editableConfigs)
            {
                Editables.Add(config);
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var asJson = JsonConvert.SerializeObject(_complexConfiguration);
            File.WriteAllText(JsonFilePath, asJson);
        }
    }
}
