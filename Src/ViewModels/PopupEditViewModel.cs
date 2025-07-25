using PDM.Src.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDM.Src.ViewModels
{
    class PopupEditViewModel : INotifyPropertyChanged
    {
        public Phone TargetPhone { get; }
        public string FieldName { get; }

        public IEnumerable<object>? Options { get; private set; } // Used for ComboBox
        public object? CurrentValue
        {
            get => GetCurrentValue();
            set
            {
                SetCurrentValue(value);
                OnPropertyChanged(nameof(CurrentValue));
            }
        }

        public PopupEditViewModel(Phone phone, string fieldName)
        {
            TargetPhone = phone;
            FieldName = fieldName;

            // Decide if it's an enum type and load options
            var prop = typeof(Phone).GetProperty(fieldName);
            if (prop != null && prop.PropertyType.IsEnum)
            {
                Options = Enum.GetValues(prop.PropertyType).Cast<object>().ToList();
            }
        }

        private object? GetCurrentValue()
        {
            var prop = typeof(Phone).GetProperty(FieldName);
            return prop?.GetValue(TargetPhone);
        }

        private void SetCurrentValue(object? value)
        {
            var prop = typeof(Phone).GetProperty(FieldName);
            if (prop != null && value != null)
            {
                prop.SetValue(TargetPhone, Convert.ChangeType(value, prop.PropertyType));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


    }
}
