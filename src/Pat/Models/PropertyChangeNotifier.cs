using System.ComponentModel;
using Pat.ViewModels;

namespace Pat.Models
{
    public class PropertyChangeNotifier : IPropertyChangeNotifier
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}