using System;
using System.Linq;
using pwiz.Common.DataBinding;

namespace pwiz.Skyline.Model.AuditLog
{
    public abstract class CustomPropertyLocalizer
    {
        protected CustomPropertyLocalizer(PropertyPath path, bool relative)
        {
            Path = path;
            Relative = relative;
        }

        private string[] PropertyPathToArray(PropertyPath path)
        {
            if (path.IsRoot)
                return new string[0];
            else
                return PropertyPathToArray(path.Parent).Concat(new[] { path.Name }).ToArray();
        }

        protected object FindObjectByPath(string[] pathArray, int index, object obj)
        {
            if (index == pathArray.Length)
                return obj;

            foreach (var property in Reflector.GetProperties(obj.GetType()))
            {
                if (property.PropertyInfo.Name == pathArray[index])
                {
                    var val = property.PropertyInfo.GetValue(obj);
                    return FindObjectByPath(pathArray, ++index, val);
                }         
            }

            return 0;
        }

        public string Localize(DiffTree tree, object parentObj)
        {
            var pathArrary = PropertyPathToArray(Path);

            object obj;
            if (!Relative)
            {
                var current = tree.Root.Objects.FirstOrDefault();
                if (current == null)
                    return null;

                obj = FindObjectByPath(pathArrary, 0, current);
            }
            else
            {
                obj = FindObjectByPath(pathArrary, 0, parentObj);
            }

            return Localize(obj);
        }

        protected abstract string Localize(object obj);

        public static CustomPropertyLocalizer CreateInstance(Type localizerType)
        {
            return (CustomPropertyLocalizer)Activator.CreateInstance(localizerType);
        }

        public PropertyPath Path { get; protected set; }
        public bool Relative { get; protected set; }

        // Test support
        public abstract string[] PossibleResourceNames { get; }
    }
}
