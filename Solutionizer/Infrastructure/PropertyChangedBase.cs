﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Solutionizer.Annotations;

namespace Solutionizer.Infrastructure {
    public abstract class PropertyChangedBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Refresh() {
            NotifyOfPropertyChange(String.Empty);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyOfPropertyChange([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [NotifyPropertyChangedInvocator]
        protected void NotifyOfPropertyChange<TProperty>(Expression<Func<TProperty>> property) {
            NotifyOfPropertyChange(GetMemberInfo(property).Name);
        }

        private static MemberInfo GetMemberInfo(Expression expression) {
            var lambdaExpression = (LambdaExpression)expression;
            return (!(lambdaExpression.Body is UnaryExpression) ? (MemberExpression)lambdaExpression.Body : (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand).Member;
        }
    }
}