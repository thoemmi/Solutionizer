using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Solutionizer.Framework {
    public class ValidationPropertyChangedBase : PropertyChangedBase, INotifyDataErrorInfo {
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable GetErrors(string propertyName) {
            if (!String.IsNullOrEmpty(propertyName)) {
                List<string> propertyErrors;
                return _errors.TryGetValue(propertyName, out propertyErrors) ? propertyErrors : null;
            } else {
                return _errors.SelectMany(err => err.Value.ToList());
            }
        }

        public bool HasErrors {
            get { return _errors.Any(); }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected virtual void OnErrorsChanged(string propertyName) {
            var handler = ErrorsChanged;
            if (handler != null) {
                handler(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        protected void Validate<TProperty>(bool isValid, Expression<Func<TProperty>> property, string error) {
            Validate(isValid, GetMemberName(property), error);
        }

        protected void Validate(bool isValid, string propertyName, string error) {
            if (isValid) {
                RemoveError(propertyName, error);
            } else {
                AddError(propertyName, error);
            }
        }

        protected void AddError<TProperty>(Expression<Func<TProperty>> property, string error) {
            AddError(GetMemberName(property), error);
        }

        protected void AddError(string propertyName, string error) {
            List<string> propertyErrors;
            if (!_errors.TryGetValue(propertyName, out propertyErrors)) {
                propertyErrors = new List<string>();
                _errors.Add(propertyName, propertyErrors);
            }
            if (!propertyErrors.Contains(error)) {
                propertyErrors.Add(error);
                OnErrorsChanged(propertyName);
            }
        }

        protected void RemoveError<TProperty>(Expression<Func<TProperty>> property, string error) {
            RemoveError(GetMemberName(property), error);
        }

        protected void RemoveError(string propertyName, string error) {
            List<string> propertyErrors;
            if (_errors.TryGetValue(propertyName, out propertyErrors)) {
                if (propertyErrors.Contains(error)) {
                    propertyErrors.Remove(error);
                    if (!propertyErrors.Any()) {
                        _errors.Remove(propertyName);
                    }
                    OnErrorsChanged(propertyName);
                }
            }
        }
    }
}