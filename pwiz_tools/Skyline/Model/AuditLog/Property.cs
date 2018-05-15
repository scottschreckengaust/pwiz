using System;
using System.Reflection;
using pwiz.Common.DataBinding;

namespace pwiz.Skyline.Model.AuditLog
{
    public class Property
    {
        private readonly DiffAttributeBase diffAttribute;

        public static readonly Property ROOT_PROPERTY = new Property(null, null);

        public Property(PropertyInfo propertyInfo, DiffAttributeBase diffAttribute)
        {
            PropertyInfo = propertyInfo;
            this.diffAttribute = diffAttribute;
        }

        public PropertyInfo PropertyInfo { get; private set; }
        [Diff]
        public bool IsRoot { get { return PropertyInfo == null && diffAttribute == null; } }

        public string GetName(DiffTree tree, object parentObject)
        {
            var name = PropertyInfo.Name;
            if (diffAttribute.CustomLocalizer != null && parentObject != null)
            {
                var localizer = CustomPropertyLocalizer.CreateInstance(diffAttribute.CustomLocalizer);
                name = localizer.Localize(tree, parentObject) ?? name;
            }

            return string.Format("{{0:{0}}}", name); // Not L10N
        }
        [Diff]
        public string Name
        {
            get
            {
                return string.Format("{{0:{0}}}", PropertyInfo.Name); // Not L10N
            }
        }
        [Diff]
        public string ElementName
        {
            get
            {
                // if resource manager doesnt have resource
                var name = PropertyElementNames.ResourceManager.GetString(PropertyInfo.Name);
                return name == null ? null : string.Format("{{1:{0}}}", PropertyInfo.Name); // Not L10N
            }
        }
        [Diff]
        public bool IsTab { get { return diffAttribute.IsTab; } }
        [Diff]
        public bool IgnoreName { get { return diffAttribute.IgnoreName; } }
        [Diff]
        public bool DiffProperties { get { return diffAttribute.DiffProperties; } }
        [Diff]
        public Type CustomLocalizer { get { return diffAttribute.CustomLocalizer; } }

        // For Debugging
        public override string ToString()
        {
            return Reflector<Property>.ToString(this);
        }
    }
}