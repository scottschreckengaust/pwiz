/*
 * Original author: Tobias Rohde <tobiasr .at. uw.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2018 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using pwiz.Common.Collections;
using pwiz.Common.DataBinding;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Model.AuditLog
{
    public class Reflector<T> where T : class
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly List<Property> _properities;

        static Reflector()
        {
            _properities = new List<Property>();
            foreach (var property in typeof(T).GetProperties())
            {
                var attributes = property.GetCustomAttributes(false);
                var diffAttr = attributes.OfType<DiffAttributeBase>().FirstOrDefault();

                if (diffAttr != null)
                {
                    _properities.Add(new Property(property, diffAttr));
                }
            }
        }

        public static IList<Property> Properties
        {
            get { return ImmutableList<Property>.ValueOf(_properities); }
        }

        public static DiffTree BuildDiffTree(Property thisProperty, T oldObj, T newObj, DateTime? timeStamp)
        {
            return new DiffTree(BuildDiffTree(thisProperty, oldObj, newObj), timeStamp ?? DateTime.Now);
        }

        public static DiffNode BuildDiffTree(Property thisProperty, T oldObj, T newObj)
        {
            return BuildDiffTree(thisProperty, oldObj, newObj, PropertyPath.Root, null);
        }

        private static DiffNode BuildDiffTree(Property thisProperty, T oldObj, T newObj, PropertyPath propertyPath, object elementKey, bool expand = false)
        {
            if (ReferenceEquals(oldObj, newObj))
                return null;

            // thisProperty.PropertyInfo is null only upon the initial call, where Property.ROOT is passed.
            // Therefore the first object to diff can't be a collection.
            var collectionDiff = thisProperty.PropertyInfo != null &&
                CollectionInfo.ForType(thisProperty.PropertyInfo.PropertyType) != null;

            // The propertyPath does not contain the current property at this point
            if (collectionDiff)
                propertyPath = propertyPath.LookupByKey(elementKey.ToString());
            else if (thisProperty.PropertyInfo != null)
                propertyPath = propertyPath.Property(thisProperty.PropertyInfo.Name);         

            DiffNode resultNode = collectionDiff
                ? new ElementPropertyDiffNode(thisProperty, propertyPath, oldObj, newObj, elementKey, null, expand)
                : new PropertyDiffNode(thisProperty, propertyPath, oldObj, newObj, null, expand);

            if (!collectionDiff && newObj == null)
                return resultNode;

            foreach (var property in _properities)
            {
                var oldVal = oldObj == null ? null : property.PropertyInfo.GetValue(oldObj);
                var newVal = newObj == null ? null : property.PropertyInfo.GetValue(newObj);

                var valType = property.PropertyInfo.PropertyType;


                // If the objects should be treated as unequal, all comparisons are ignored
                if (!ReferenceEquals(oldVal, newVal) || expand)
                {
                    if (!property.DiffProperties)
                    {
                        var collectionInfo = CollectionInfo.ForType(valType);
                        if (collectionInfo != null)
                        {
                            // Here we just want to diff the collection elements and dont recurse furthermore,
                            // so we callback simply checks for equality
                            var nodes = CompareCollections(collectionInfo, property, propertyPath, oldVal, newVal, expand,
                                (type, prop, oldElem, newElem, propPath, key, exp) =>
                                {
                                    var unequal = !object.Equals(oldElem, newElem) || exp;
                                    if (unequal)
                                        return new ElementPropertyDiffNode(prop, propPath.LookupByKey(key.ToString()),
                                            oldElem, newElem, key, null, expand);
                                    else
                                        return null;
                                });

                            // Only if any elements are different we add a new diff node to the tree,
                            // In this case the current property name has to be added to the property path since we don't recurse futher
                            if (nodes.Any() || expand)
                                resultNode.Nodes.Add(new PropertyDiffNode(property,
                                    propertyPath.Property(property.PropertyInfo.Name), oldVal, newVal, nodes, expand));
                        }
                        else
                        {
                            // Properties that are not DiffParent's and not collections are simply compared
                            if (!object.Equals(oldVal, newVal) || expand)
                                resultNode.Nodes.Add(new PropertyDiffNode(property,
                                    propertyPath.Property(property.PropertyInfo.Name), oldVal, newVal, null, expand));
                        }
                    }
                    else
                    {
                        var propertyType = valType;
                        var collectionInfo = CollectionInfo.ForType(propertyType);

                        if (collectionInfo != null)
                        {
                            // Since the collection is DiffParent, we have to go into the elements and compare their properties
                            var nodes = CompareCollections(collectionInfo, property, propertyPath, oldVal, newVal, expand, Reflector.BuildDiffTree);

                            if (nodes.Any() || expand)
                                resultNode.Nodes.Add(new PropertyDiffNode(property, propertyPath.Property(property.PropertyInfo.Name), oldVal, newVal, nodes, expand));
                        }
                        else
                        {
                            var node = Reflector.BuildDiffTree(propertyType, property, oldVal, newVal, propertyPath, null, expand);
                            // Only add the node if the elements are unequal
                            if (node != null)
                                resultNode.Nodes.Add(node);
                        }
                    }
                }
            }

            // If there are child nodes, we always want to return the parent node,
            // but if it's just the root node or a diffparent (meaning only the object reference changed)
            // we return null (meaning the objects are equivalent)
            return resultNode.Nodes.Count != 0 || (thisProperty.PropertyInfo != null && !thisProperty.DiffProperties) ? resultNode : null;
        }

        // Determines whether two IAuditLogObjects (most likely from two different
        // collections match.
        private static bool Matches(Type type, IAuditLogObject obj1, IAuditLogObject obj2, bool diffProperties)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 == null || obj2 == null)
                return false;

            if (obj1.IsName)
                return obj1.AuditLogText == obj2.AuditLogText;
            else
            {
                var auditObj1 = obj1 as AuditLogObject;
                var auditObj2 = obj2 as AuditLogObject;

                object cmpObj1 = obj1;
                object cmpObj2 = obj2;

                if (auditObj1 != null && auditObj2 != null)
                {
                    cmpObj1 = auditObj1.Object;
                    cmpObj2 = auditObj2.Object;
                }

                return diffProperties
                    ? Reflector.Equals(type, cmpObj1, cmpObj2)
                    : object.Equals(cmpObj1, cmpObj2);
            }   
        }

        private static List<DiffNode> CompareCollections(ICollectionInfo collectionInfo, Property property, PropertyPath propertyPath, object oldVal, object newVal, bool expand, Func<Type, Property, object, object, PropertyPath, object, bool, DiffNode> func)
        {
            var result = new List<DiffNode>();

            var oldKeys = collectionInfo.GetKeys(oldVal).OfType<object>().ToArray();
            var newKeys = collectionInfo.GetKeys(newVal).OfType<object>().ToList();
            var removeIndices = new List<int>();

            propertyPath = propertyPath.Property(property.PropertyInfo.Name);

            // If we assume elements are unequal, we assume the old collection was empty
            // and all elements were added
            if (expand)
            {
                return newKeys.Select(k =>
                {
                    var value = collectionInfo.GetItemValueFromKey(newVal, k);
                    // We also want to know what properties were set in the new elements,
                    // So we call the diff function here too
                    var node = func(collectionInfo.ElementType, property, null,
                        value, propertyPath, k, true);

                    return (DiffNode) new ElementDiffNode(property,
                        propertyPath.LookupByKey(k.ToString()),
                        value, k, false,
                        node == null ? null : node.Nodes, true);
                }).ToList();
            }

            var newElemObjs = newKeys
                .Select(k => AuditLogObject.GetAuditLogObject(collectionInfo.GetItemValueFromKey(newVal, k), true)).ToList();

            foreach (var key in oldKeys)
            {
                var oldKey = key;
                var oldElem = collectionInfo.GetItemValueFromKey(oldVal, oldKey);

                var oldElemObj = AuditLogObject.GetAuditLogObject(oldElem, true);

                // Try to find a matching object
                var index = newElemObjs.FindIndex(o => Matches(collectionInfo.ElementType, o, oldElemObj, property.DiffProperties));

                if (index >= 0)
                {
                    var newElem = collectionInfo.GetItemValueFromKey(newVal, newKeys[index]);
                    var node = func(collectionInfo.ElementType, property, oldElem, newElem, propertyPath, newKeys[index], false);

                    // Make sure elements are unequal
                    if (node != null)
                        result.Add(node);

                    // This item has been dealt with, so we mark it for removal
                    removeIndices.Add(index);
                }
                else
                {
                    // If the element can't be found, we assume it was removed
                    result.Add(new ElementDiffNode(property, propertyPath.LookupByKey(key.ToString()), oldElem, key, true));
                }
            }

            // We order by descending so that the later indices are still valid
            // after removing one
            foreach (var i in removeIndices.OrderByDescending(i => i))
                newKeys.RemoveAt(i);

            // The keys that are left over are most likely elements that were added to the colletion
            result.AddRange(newKeys.Select(k =>
                new ElementDiffNode(property, propertyPath.LookupByKey(k.ToString()), collectionInfo.GetItemValueFromKey(newVal, k), k, false)));

            return result;
        }

        // Checks whether two collections are exactly equal, meaning all elements are the same and at the same
        // indices/keys.
        private static bool CollectionEquals(ICollectionInfo collectionInfo, object coll1, object coll2, Func<Type, object, object, bool> func)
        {
            var oldKeys = collectionInfo.GetKeys(coll1).OfType<object>().ToArray();
            var newKeys = collectionInfo.GetKeys(coll2).OfType<object>().ToArray();

            if (oldKeys.Length != newKeys.Length)
                return false;

            for (var i = 0; i < oldKeys.Length; ++i)
            {
                if (oldKeys[i] != newKeys[i])
                    return false;

                var val1 = collectionInfo.GetItemValueFromKey(coll1, oldKeys[i]);
                var val2 = collectionInfo.GetItemValueFromKey(coll2, newKeys[i]);

                if (!func(collectionInfo.ElementType, val1, val2))
                    return false;
            }

            return true;
        }

        // Compares two objects based on their properties which are marked with Diff attributes
        // Structurally similar to BuildDiffTree, but no nodes are created, it simply checks for equality
        public static bool Equals(T obj1, T obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
                return false;

            foreach (var property in _properities)
            {
                var val1 = property.PropertyInfo.GetValue(obj1);
                var val2 = property.PropertyInfo.GetValue(obj2);

                if (ReferenceEquals(val1, val2))
                    continue;

                if (ReferenceEquals(val1, null) || ReferenceEquals(val2, null))
                    return false;

                var valType = property.PropertyInfo.PropertyType;

                var collectionInfo = CollectionInfo.ForType(valType);
                if (!property.DiffProperties)
                {
                    if (collectionInfo != null)
                    {
                        if (!CollectionEquals(collectionInfo, val1, val2,
                            (type, elem1, elem2) => object.Equals(elem1, elem2)))
                            return false;
                    }
                    else if (!object.Equals(val1, val2))
                        return false;
                }
                else
                {
                    if (collectionInfo != null)
                    {
                        if (!CollectionEquals(collectionInfo, val1, val2, Reflector.Equals))
                            return false;
                    }
                    else if (!Reflector.Equals(valType, val1, val2))
                        return false;
                }
            }

            return true;
        }

        // Converts object into a string representation based on its properties
        // that are marked with Diff and DiffParent
        public static string ToString(T obj)
        {
            return ToString(obj, true);
        }

        private static string ToString(Type type, object obj, bool wrapProperties)
        {
            if (obj == null)
                return "{2:Missing}"; // Not L10N

            if (Reflector.HasToString(obj))
                return LogMessage.Quote(obj.ToString());

            return Reflector.ToString(type, obj, wrapProperties);
        }

        private static string ToString(T obj, bool wrapProperties)
        {
            var collectionInfo = CollectionInfo.ForType(typeof(T));

            string[] strings;
            string format;

            var auditLogObj = AuditLogObject.GetAuditLogObject(obj);

            string result;
            if (auditLogObj.IsName)
            {
                var name = LogMessage.Quote(auditLogObj.AuditLogText);
                if (collectionInfo != null || _properities.Count != 0)
                    result = string.Format("{0}: {{0}}", name); // Not L10N
                else
                    return name;
            }
            else
            {
                result = "{0}"; // Not L10N;
            }

            if (collectionInfo != null)
            {
                var keys = collectionInfo.GetKeys(obj).Cast<object>().ToArray();

                strings = new string[keys.Length];
                for (var i = 0; i < keys.Length; ++i)
                {
                    var element = collectionInfo.GetItemValueFromKey(obj, keys[i]);
                    strings[i] = ToString(collectionInfo.ElementType, element, wrapProperties);
                }

                format = "[ {0} ]"; // Not L10N
            }
            else
            {
                strings = new string[_properities.Count];
                for (var i = 0; i < _properities.Count; ++i)
                {
                    var property = _properities[i];
                    var value = property.PropertyInfo.GetValue(obj);
                    var type = property.PropertyInfo.PropertyType;

                    if (property.IgnoreName)
                    {
                        strings[i] = ToString(type, value, false);
                    }
                    else
                    {
                        strings[i] = property.Name + "="; // Not L10N
                        strings[i] += ToString(type, value, true);
                    }
                }

                format = wrapProperties ? "{{ {0} }}" : "{0}"; // Not L10N
            }

            return string.Format(result, string.Format(format, string.Join(", ", strings))); // Not L10N
        }
    }

    // Wrapper class for Reflector<T> that takes a Type as parameter in each of its function
    // and constructs a Reflector<T> object using reflection and calls the corresponding
    // function.
    public class Reflector
    {
        #region Wrapper functions
        public static IList<Property> GetProperties(Type type)
        {
            var reflectorType = typeof(Reflector<>).MakeGenericType(type);
            var property = reflectorType.GetProperty("Properties", BindingFlags.Public | BindingFlags.Static); // Not L10N

            Assume.IsNotNull(property);

            var reflector = Activator.CreateInstance(reflectorType);
            return (IList<Property>) property.GetValue(reflector);
        }

        public static DiffTree BuildDiffTree(Type objectType, Property thisProperty, object oldObj, object newObj, DateTime? timeStamp)
        {
            var type = typeof(Reflector<>).MakeGenericType(objectType);
            var buildDiffTree = type.GetMethod("BuildDiffTree", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Property), objectType, objectType, typeof(DateTime?) }, null); // Not L10N

            Assume.IsNotNull(buildDiffTree); 

            var reflector = Activator.CreateInstance(type);
            
            return (DiffTree) buildDiffTree.Invoke(reflector, new[] { thisProperty, oldObj, newObj, timeStamp });
        }

        public static DiffNode BuildDiffTree(Type objectType, Property thisProperty, object oldObj, object newObj, PropertyPath propertyPath, object elementKey, bool expand)
        {
            var type = typeof(Reflector<>).MakeGenericType(objectType);
            var buildDiffTree = type.GetMethod("BuildDiffTree", BindingFlags.NonPublic | BindingFlags.Static, null, new[] {typeof(Property), objectType, objectType, typeof(PropertyPath), typeof(object), typeof(bool) }, null); // Not L10N

            Assume.IsNotNull(buildDiffTree);

            var reflector = Activator.CreateInstance(type);
            return (DiffNode)buildDiffTree.Invoke(reflector, new[] { thisProperty, oldObj, newObj, propertyPath, elementKey, expand });
        }

        public static bool Equals(Type objectType, object obj1, object obj2)
        {
            var type = typeof(Reflector<>).MakeGenericType(objectType);
            var equals = type.GetMethod("Equals", BindingFlags.Public | BindingFlags.Static, null, new[] { objectType, objectType }, null); // Not L10N

            Assume.IsNotNull(equals);

            var reflector = Activator.CreateInstance(type);
            return (bool)equals.Invoke(reflector, new[] { obj1, obj2 });
        }

        public static string ToString(Type objectType, object obj, bool wrapProperties)
        {
            var type = typeof(Reflector<>).MakeGenericType(objectType);
            var toString = type.GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { objectType, typeof(bool) }, null); // Not L10N

            Assume.IsNotNull(toString);

            var reflector = Activator.CreateInstance(type);
            return (string)toString.Invoke(reflector, new[] { obj, wrapProperties });
        }

        public static string ToString(Type objectType, object obj)
        {
            var type = typeof(Reflector<>).MakeGenericType(objectType);
            var toString = type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Static, null, new[] { objectType }, null); // Not L10N

            Assume.IsNotNull(toString);

            var reflector = Activator.CreateInstance(type);
            return (string)toString.Invoke(reflector, new[] { obj });
        }
        #endregion

        public static bool HasToString(object obj)
        {
            if (obj == null)
                return false;

            var objectType = obj.GetType();
            var mi = objectType.GetMethod("ToString", new Type[] { }); // Not L10N
            return mi != null && (mi.DeclaringType != typeof(object) && mi.DeclaringType != typeof(XmlNamedElement) || objectType.IsEnum);
        }

        private static DiffNode FindParentNode(DiffNode root, DiffNode findNode)
        {
            if (ReferenceEquals(root, findNode))
                return null;

            if (root.Nodes.Contains(findNode))
                return root;

            return root.Nodes.Select(n => FindParentNode(n, findNode)).FirstOrDefault(n => n != null);
        }

        public static DiffNode ExpandDiffTree(DiffTree tree, DiffNode currentNode, object elementKey = null)
        {
            currentNode.Nodes.Clear();

            // Get parent node so that we can replace the current node in the diff tree
            var parentNode = FindParentNode(tree.Root, currentNode);
            if (parentNode == null)
                return null;

            var objects = currentNode.Objects.ToArray();

            Assume.IsTrue(objects.Any());

            var newObj = objects[0];
            var oldObj = objects.Length == 2 ? objects[1] : null;

            var type = currentNode.Property.PropertyInfo.PropertyType;
            if (elementKey != null)
                type = CollectionInfo.ForType(type).ElementType;

            var node = BuildDiffTree(type, currentNode.Property, oldObj, newObj, currentNode.PropertyPath.Parent, elementKey, true);

            if (node == null)
                return currentNode;

            // It's important that the node gets replaced here, otherwise the for loop
            // in FindAllLeafNodes would break
            var index = parentNode.Nodes.IndexOf(currentNode);
            parentNode.Nodes.RemoveAt(index);
            parentNode.Nodes.Insert(index, node);

            return node;
        }
    }

    public class AuditLogObject : IAuditLogObject
    {
        public AuditLogObject(object obj)
        {
            Object = obj;
        }

        public string AuditLogText
        {
            get
            {
                if (Object == null)
                    return null;

                if (Reflector.HasToString(Object))
                    return Object.ToString();

                return Reflector.ToString(Object.GetType(), Object);
            }
        }

        public bool IsName
        {
            get { return false; }
        }

        public object Object { get; private set; }

        public static IAuditLogObject GetAuditLogObject(object obj, bool allowNull = false)
        {
            bool usesReflection;
            return GetAuditLogObject(obj, out usesReflection, allowNull);
        }
        public static IAuditLogObject GetAuditLogObject(object obj, out bool usesReflection, bool allowNull = false)
        {
            var auditLogObj = obj as IAuditLogObject;
            usesReflection = auditLogObj == null && !Reflector.HasToString(obj) ;

            if (!allowNull && obj == null)
                return null;

            return auditLogObj ?? new AuditLogObject(obj);
        }
    }
}