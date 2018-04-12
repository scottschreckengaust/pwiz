using System.Collections.Generic;
using System.Linq;
using pwiz.Common.Collections;

namespace pwiz.Skyline.Model.ElementLocators
{
    public class ElementRefs
    {
        private static readonly ImmutableList<ElementRef> PROTOTYPES = ImmutableList.ValueOf(new ElementRef[]
        {
            DocumentRef.PROTOTYPE, MoleculeGroupRef.PROTOTYPE, MoleculeRef.PROTOTYPE, PrecursorRef.PROTOTYPE, TransitionRef.PROTOTYPE,
            ReplicateRef.PROTOTYPE, ResultFileRef.PROTOTYPE,
            MoleculeResultRef.PROTOTYPE, PrecursorResultRef.PROTOTYPE, TransitionResultRef.PROTOTYPE
        });
        private static readonly ImmutableList<NodeRef> NODEREFPROTOTYPES 
            = ImmutableList.ValueOf(PROTOTYPES.OfType<NodeRef>());

        private static readonly IDictionary<string, ElementRef> _prototypes =
            PROTOTYPES.ToDictionary(element => element.ElementType);

        private readonly IDictionary<IdentityPath, NodeRef> _nodeRefs 
            = new Dictionary<IdentityPath, NodeRef>();
        private readonly IDictionary<IdentityPath, NodeRef[]> _siblings 
            = new Dictionary<IdentityPath, NodeRef[]>();

        public ElementRefs(SrmDocument document)
        {
            Document = document;
        }

        public SrmDocument Document { get; private set; }

        public NodeRef GetNodeRef(IdentityPath identityPath)
        {
            if (identityPath.IsRoot)
            {
                return DocumentRef.PROTOTYPE;
            }
            lock (this)
            {
                NodeRef nodeRef;
                if (_nodeRefs.TryGetValue(identityPath, out nodeRef))
                {
                    return nodeRef;
                }
                var parentIdentityPath = identityPath.Parent;
                DocNodeParent parentNode;
                try
                {
                    parentNode = (DocNodeParent) Document.FindNode(parentIdentityPath);
                }
                catch (IdentityNotFoundException)
                {
                    return null;
                }
                int childIndex = parentNode.FindNodeIndex(identityPath.Child);
                if (childIndex < 0)
                {
                    return null;
                }
                NodeRef[] siblings;
                if (!_siblings.TryGetValue(parentIdentityPath, out siblings))
                {
                    var parentRef = GetNodeRef(parentIdentityPath);
                    if (parentRef == null)
                    {
                        return null;
                    }
                    var siblingRef = NODEREFPROTOTYPES[identityPath.Length].ChangeParent(parentRef);
                    siblings = siblingRef.ListChildrenOfParent(Document).Cast<NodeRef>().ToArray();
                    _siblings.Add(parentIdentityPath, siblings);
                }
                var result = siblings[childIndex];
                _nodeRefs.Add(identityPath, result);
                return result;
            }
        }
        public static ElementRef FromObjectReference(ElementLocator objectReference)
        {
            var prototype = _prototypes[objectReference.Type];
            return prototype.ChangeElementLocator(objectReference);
        }
    }
}
