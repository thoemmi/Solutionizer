using System;

namespace Solutionizer.Framework {
    public static class ViewLocator {
        public static Func<Type, Type> GetViewTypeFromViewModelType;
        public static Func<string, string> GetViewTypeNameFromViewModelTypeName;

        static ViewLocator() {
            GetViewTypeNameFromViewModelTypeName = viewModeltypeName => viewModeltypeName.Replace("ViewModel", "View");
            GetViewTypeFromViewModelType = type => {
                var viewModelTypeName = type.FullName;
                var viewTypeName = GetViewTypeNameFromViewModelTypeName(viewModelTypeName);
                var viewType = type.Assembly.GetType(viewTypeName);
                return viewType;
            };
        }
    }
}