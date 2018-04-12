using System.Collections.Generic;
using System.Linq;
using pwiz.Common.Collections;

namespace pwiz.Skyline.Model.ElementLocators
{
    public class ElementRefCache
    {
        private static readonly ImmutableList<NodeRef> NODEREFPROTOTYPES = ImmutableList.ValueOf(new NodeRef[]
        {
            DocumentRef.PROTOTYPE, MoleculeGroupRef.PROTOTYPE, MoleculeRef.PROTOTYPE, PrecursorRef.PROTOTYPE, TransitionRef.PROTOTYPE
        });

        private readonly IDictionary<IdentityPath, NodeRef> _nodeRefs 
            = new Dictionary<IdentityPath, NodeRef>();
        private readonly IDictionary<IdentityPath, NodeRef[]> _siblings 
            = new Dictionary<IdentityPath, NodeRef[]>();
        public ElementRefCache(SrmDocument document)
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
    }
}
