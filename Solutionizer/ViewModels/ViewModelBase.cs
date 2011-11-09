using System;
using System.Linq.Expressions;

namespace Solutionizer.ViewModels {
    public abstract class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase {
        public void RaisePropertyChanged<T>(Expression<Func<T>> propertySelector) {
            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression != null) {
                RaisePropertyChanged(memberExpression.Member.Name);
            }
        }
    }
}