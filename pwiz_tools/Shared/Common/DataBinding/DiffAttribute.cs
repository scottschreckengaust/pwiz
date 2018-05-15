using System;

namespace pwiz.Common.DataBinding
{
    public interface IAuditLogObject
    {
        string AuditLogText { get; }
        bool IsName { get; }
    }

    public interface INamedObject
    {
        string Name { get; }
    }

    public abstract class DiffAttributeBase : Attribute
    {
        protected DiffAttributeBase(bool isTab, bool ignoreName, Type customLocalizer)
        {
            IsTab = isTab;
            IgnoreName = ignoreName;
            CustomLocalizer = customLocalizer;

            /*if (customLocalizer != null && !customLocalizer.IsSubclassOf(typeof(CustomPropertyLocalizer)))
                throw new ArgumentException();*/
        }

        public bool IsTab { get; protected set; }
        public bool IgnoreName { get; protected set; }

        public virtual bool DiffProperties { get { return false; } }

        public Type CustomLocalizer;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DiffAttribute : DiffAttributeBase
    {
        public DiffAttribute(bool isTab = false,
            bool ignoreName = false,
            Type customLocalizer = null)
            : base(isTab, ignoreName, customLocalizer) { }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DiffParentAttribute : DiffAttributeBase
    {
        public DiffParentAttribute(bool isTab = false,
            bool ignoreName = false,
            Type customLocalizer = null)
            : base(isTab, ignoreName, customLocalizer) { }

        public override bool DiffProperties { get { return true; } }
    }
}