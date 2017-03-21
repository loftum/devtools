using System;
using System.ComponentModel;
using Pat.IO;
using Pat.Mapping;
using Pat.Models;
using Pat.ViewModels;

namespace Pat
{
    public partial class MainWindow
    {
        private readonly FileManager _fileManager = new FileManager();
        protected MainViewModel Vm => (MainViewModel) DataContext;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var settings = _fileManager.LoadJsonOrDefault<WindowSettings>() ?? new WindowSettings();
            settings.MapTo(this);
            Vm.Load();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var settings = this.MapTo<WindowSettings>();
            _fileManager.SaveJson(settings);
            Vm.Save();
            base.OnClosing(e);
        }
    }
}
