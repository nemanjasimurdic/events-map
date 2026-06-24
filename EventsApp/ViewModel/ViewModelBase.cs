using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EventsApp.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors
            = new Dictionary<string, List<string>>();

        public bool HasErrors => _errors.Values.Any(v => v.Count > 0);

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            ValidateProperty(value, propertyName);
            return true;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.SelectMany(x => x.Value);

            List<string> errors;
            return _errors.TryGetValue(propertyName, out errors) && errors.Count > 0
                ? errors
                : Enumerable.Empty<string>();
        }

        protected void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            var list = errors?.ToList();
            if (list != null && list.Count > 0)
                _errors[propertyName] = list;
            else
                _errors.Remove(propertyName);

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }

        protected void ClearErrors(string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                var keys = _errors.Keys.ToList();
                _errors.Clear();
                foreach (var key in keys)
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(key));
            }
            else if (_errors.Remove(propertyName))
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }

            OnPropertyChanged(nameof(HasErrors));
        }

        protected virtual void ValidateProperty(object value, string propertyName)
        {
        }

        public void ValidateAllProperties()
        {
            foreach (var prop in GetType().GetProperties()
                .Where(p => p.CanRead && p.GetGetMethod()?.IsPublic == true))
            {
                ValidateProperty(prop.GetValue(this), prop.Name);
            }
        }
    }
}
