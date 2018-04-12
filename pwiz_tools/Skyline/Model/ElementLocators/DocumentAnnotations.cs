
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using pwiz.Common.DataBinding;
using pwiz.Common.SystemUtil;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.Hibernate;
using pwiz.Skyline.Model.Results;
using pwiz.Skyline.Util.Extensions;

namespace pwiz.Skyline.Model.ElementLocators
{
    public class DocumentAnnotations
    {
        public const string COLUMN_LOCATOR = "ElementLocator";
        public const string COLUMN_ANNOTATION_NAME = "AnnotationName";
        public const string COLUMN_ANNOTATION_VALUE = "AnnotationValue";
        private readonly ElementRefs _elementRefs;
        private readonly IDictionary<NodeRef, IdentityPath> _identityPaths 
            = new Dictionary<NodeRef, IdentityPath>();
        public DocumentAnnotations(SrmDocument document)
        {
            _elementRefs = new ElementRefs(document);
            Document = document;
            CultureInfo = CultureInfo.InvariantCulture;
        }

        public CultureInfo CultureInfo { get; set; }

        public SrmDocument Document { get; private set; }

        public void WriteAnnotationsToFile(CancellationToken cancellationToken, string filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                SaveAnnotations(cancellationToken, writer, TextUtil.SEPARATOR_CSV);
            }
        }
        
        public void SaveAnnotations(CancellationToken cancellationToken, TextWriter writer, char separator)
        {
            WriteAllAnnotations(cancellationToken, writer, separator);
        }
        
        private void WriteAllAnnotations(CancellationToken cancellationToken, TextWriter textWriter, char separator)
        {
            var elementRefTexts = new Dictionary<ElementRef, string>();
            string strSeparator = new string(separator, 1);
            textWriter.WriteLine(string.Join(strSeparator, COLUMN_LOCATOR, COLUMN_ANNOTATION_NAME, COLUMN_ANNOTATION_VALUE));
            foreach (var annotationDef in Document.Settings.DataSettings.AnnotationDefs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var tuple in GetAnnotationValues(annotationDef))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var elementRef = tuple.Item1;
                    string locatorText;
                    if (!elementRefTexts.TryGetValue(elementRef, out locatorText))
                    {
                        locatorText = elementRef.ToElementLocator().ToString();
                        elementRefTexts.Add(elementRef, locatorText);
                    }
                    textWriter.WriteLine(string.Join(strSeparator,
                        DsvWriter.ToDsvField(separator, locatorText),
                        DsvWriter.ToDsvField(separator, annotationDef.Name),
                        DsvWriter.ToDsvField(separator, ValueToString(tuple.Item2))
                    ));
                }
            }
        }

        public IEnumerable<Tuple<ElementRef, object>> GetAnnotationValues(AnnotationDef annotationDef)
        {
            return GetAllNodeAnnotations(annotationDef)
                .Concat(GetAllReplicateAnnotations(annotationDef))
                .Concat(GetAllResultAnnotations(annotationDef));
        }

        private IEnumerable<Tuple<ElementRef, object>> GetAllNodeAnnotations(AnnotationDef annotationDef)
        {
            var result = Enumerable.Empty<Tuple<ElementRef, object>>();
            var targets = annotationDef.AnnotationTargets;
            if (targets.Contains(AnnotationDef.AnnotationTarget.protein))
            {
                result = result.Concat(GetNodeAnnotations(
                    Document.MoleculeGroups.Select(ToIdentityPathTuple)
                    , annotationDef));
            }
            if (targets.Contains(AnnotationDef.AnnotationTarget.peptide))
            {
                result = result.Concat(GetNodeAnnotations(
                    EnumerateMolecules().Select(ToIdentityPathTuple),
                    annotationDef));
            }
            if (targets.Contains(AnnotationDef.AnnotationTarget.precursor))
            {
                result = result.Concat(GetNodeAnnotations(
                    EnumeratePrecursors().Select(ToIdentityPathTuple),
                    annotationDef));
            }
            if (targets.Contains(AnnotationDef.AnnotationTarget.transition))
            {
                result = result.Concat(GetNodeAnnotations(
                    EnumerateTransitions().Select(ToIdentityPathTuple),
                    annotationDef));
            }
            return result;
        }

        private IEnumerable<Tuple<ElementRef, object>> GetAllReplicateAnnotations(AnnotationDef annotationDef)
        {
            var measuredResults = Document.MeasuredResults;
            if (measuredResults == null)
            {
                yield break;
            }
            var targets = annotationDef.AnnotationTargets;
            if (targets.Contains(AnnotationDef.AnnotationTarget.replicate))
            {
                foreach (var chromatogramSet in measuredResults.Chromatograms)
                {
                    var value = GetAnnotationValue(chromatogramSet.Annotations, annotationDef);
                    if (value != null)
                    {
                        yield return Tuple.Create((ElementRef)
                            ReplicateRef.FromChromatogramSet(chromatogramSet), value);
                    }
                }
            }
        }

        private IEnumerable<Tuple<ElementRef, object>> GetAllResultAnnotations(AnnotationDef annotationDef)
        {
            var result = Enumerable.Empty<Tuple<ElementRef, object>>();
            var targets = annotationDef.AnnotationTargets;
            if (targets.Contains(AnnotationDef.AnnotationTarget.precursor_result))
            {
                foreach (var tuple in EnumeratePrecursors())
                {
                    var precursor = tuple.Item3;
                    if (null == precursor.Results)
                    {
                        continue;
                    }
                    var precursorRef = (PrecursorRef) _elementRefs.GetNodeRef(ToIdentityPathTuple(tuple).Item1);
                    var precursorResultRef =
                        (PrecursorResultRef) PrecursorResultRef.PROTOTYPE.ChangeParent(precursorRef);
                    result = result.Concat(GetNodeResultAnnotations(
                        precursorResultRef,
                        precursor.Results, annotationDef));
                }
            }
            if (targets.Contains(AnnotationDef.AnnotationTarget.transition_result))
            {
                foreach (var tuple in EnumerateTransitions())
                {
                    var transition = tuple.Item4;
                    if (null == transition.Results)
                    {
                        continue;
                    }
                    var transitionRef = (TransitionRef) _elementRefs.GetNodeRef(ToIdentityPathTuple(tuple).Item1);
                    var transitionResultRef =
                        (TransitionResultRef) TransitionResultRef.PROTOTYPE.ChangeParent(transitionRef);
                    result = result.Concat(GetNodeResultAnnotations(transitionResultRef, transition.Results,
                        annotationDef));
                }
            }
            return result;
        }

        private IEnumerable<Tuple<PeptideGroupDocNode, PeptideDocNode>> EnumerateMolecules()
        {
            return Document.MoleculeGroups.SelectMany(group => group.Molecules.Select(mol => Tuple.Create(group, mol)));
        }

        private IEnumerable<Tuple<PeptideGroupDocNode, PeptideDocNode, TransitionGroupDocNode>> 
            EnumeratePrecursors()
        {
            return EnumerateMolecules().SelectMany(tuple =>
                tuple.Item2.TransitionGroups.Select(precursor => Tuple.Create(tuple.Item1, tuple.Item2, precursor)));
        }

        private IEnumerable<Tuple<PeptideGroupDocNode, PeptideDocNode, TransitionGroupDocNode, TransitionDocNode>>
            EnumerateTransitions()
        {
            return EnumeratePrecursors().SelectMany(tuple => tuple.Item3.Transitions.Select(
                transition => Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item3, transition)));
        }

        private IEnumerable<Tuple<ElementRef, object>> GetNodeAnnotations<TNode>(
            IEnumerable<Tuple<IdentityPath, TNode>> identityPaths, AnnotationDef annotationDef) where TNode: DocNode
        {
            foreach (var tuple in identityPaths)
            {
                var identityPath = tuple.Item1;
                var docNode = tuple.Item2;
                object value = GetAnnotationValue(docNode.Annotations, annotationDef);
                if (value == null)
                {
                    continue;
                }
                yield return Tuple.Create((ElementRef) _elementRefs.GetNodeRef(identityPath), value);
            }
        }

        private IEnumerable<Tuple<ElementRef, object>> GetNodeResultAnnotations<TDocNode, TChromInfo>(
            ResultRef<TDocNode, TChromInfo> resultRef,
            Results<TChromInfo> results,
            AnnotationDef annotationDef) where TDocNode : DocNode where TChromInfo : ChromInfo
        {
            if (null == results)
            {
                yield break;
            }
            for (int i = 0; i < results.Count; i++)
            {
                var chromatogramSet = Document.Settings.MeasuredResults.Chromatograms[i];
                foreach (var chromInfo in results[i])
                {
                    var annotations = resultRef.GetAnnotations(chromInfo);
                    if (annotations == null)
                    {
                        continue;
                    }
                    var value = GetAnnotationValue(annotations, annotationDef);
                    if (value == null)
                    {
                        continue;
                    }
                    yield return Tuple.Create((ElementRef) resultRef.ChangeChromInfo(chromatogramSet, chromInfo), value);
                }
            }
        }

        private object GetAnnotationValue(Annotations annotations, AnnotationDef annotationDef)
        {
            var value = annotations.GetAnnotation(annotationDef);
            if (false.Equals(value))
            {
                return null;
            }
            return value;
        }

        private string ValueToString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (value is double)
            {
                return ((double) value).ToString(Formats.RoundTrip, CultureInfo);
            }
            return value.ToString();
        }

        private static Tuple<IdentityPath, PeptideGroupDocNode> ToIdentityPathTuple(PeptideGroupDocNode peptideGroupDocNode)
        {
            return Tuple.Create(new IdentityPath(peptideGroupDocNode.Id), peptideGroupDocNode);
        }

        private static Tuple<IdentityPath, PeptideDocNode> ToIdentityPathTuple(
            Tuple<PeptideGroupDocNode, PeptideDocNode> tuple)
        {
            return Tuple.Create(new IdentityPath(tuple.Item1.Id, tuple.Item2.Id), tuple.Item2);
        }

        private static Tuple<IdentityPath, TransitionGroupDocNode> ToIdentityPathTuple(
            Tuple<PeptideGroupDocNode, PeptideDocNode, TransitionGroupDocNode> tuple)
        {
            return Tuple.Create(new IdentityPath(tuple.Item1.Id, tuple.Item2.Id, tuple.Item3.Id), tuple.Item3);
        }

        private static Tuple<IdentityPath, TransitionDocNode> ToIdentityPathTuple(
            Tuple<PeptideGroupDocNode, PeptideDocNode, TransitionGroupDocNode, TransitionDocNode> tuple)
        {
            return Tuple.Create(new IdentityPath(tuple.Item1.Id, tuple.Item2.Id, tuple.Item3.Id, tuple.Item4.Id), tuple.Item4);
        }

        public SrmDocument ReadAnnotationsFromFile(CancellationToken cancellationToken, string filename)
        {
            using (var streamReader = new StreamReader(filename))
            {
                var dsvReader = new DsvFileReader(streamReader, TextUtil.SEPARATOR_CSV);
                return ReadAllAnnotations(cancellationToken, dsvReader);
            }
        }

        public SrmDocument ReadAllAnnotations(CancellationToken cancellationToken, DsvFileReader fileReader)
        {
            var elementRefs = new Dictionary<string, ElementRef>();
            var document = Document;
            var annotationDefs = document.Settings.DataSettings.AnnotationDefs
                .ToDictionary(annotationDef => annotationDef.Name);
            int icolLocator = EnsureColumn(fileReader, COLUMN_LOCATOR);
            int icolAnnotationName = EnsureColumn(fileReader, COLUMN_ANNOTATION_NAME);
            int icolAnnotationValue = EnsureColumn(fileReader, COLUMN_ANNOTATION_VALUE);
            string[] row;
            while ((row = fileReader.ReadLine()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string locatorText = row[icolLocator];
                ElementRef elementRef;
                if (!elementRefs.TryGetValue(locatorText, out elementRef))
                {
                    var elementLocator = ElementLocator.Parse(locatorText);
                    elementRef = ElementRefs.FromObjectReference(elementLocator);
                    elementRefs.Add(locatorText, elementRef);
                }

                string annotationName = row[icolAnnotationName];
                AnnotationDef annotationDef;
                if (!annotationDefs.TryGetValue(annotationName, out annotationDef))
                {
                    throw new InvalidDataException(string.Format("No such annotation '{0}'", annotationName));
                }
                if (annotationDef.AnnotationTargets.Intersect(elementRef.AnnotationTargets).IsEmpty)
                {
                    throw AnnotationDoesNotApplyException(annotationDef, elementRef);
                }
                string annotationValue = row[icolAnnotationValue];
                document = SetAttribute(document, elementRef, annotationDef, annotationValue);
            }
            return document;
        }

        public SrmDocument SetAttribute(SrmDocument document, ElementRef elementRef, AnnotationDef annotationDef, string annotationValue)
        {
            var nodeRef = elementRef as NodeRef;
            if (nodeRef != null)
            {
                return SetDocNodeAttribute(document, nodeRef, annotationDef, annotationValue);
            }
            var replicateRef = elementRef as ReplicateRef;
            if (replicateRef != null)
            {
                return SetReplicateAttribute(document, replicateRef, annotationDef, annotationValue);
            }
            var resultRef = elementRef as ResultRef;
            if (resultRef != null)
            {
                return SetResultAttribute(document, resultRef, annotationDef, annotationValue);
            }
            throw AnnotationDoesNotApplyException(annotationDef, elementRef);
        }

        public SrmDocument SetDocNodeAttribute(SrmDocument document, NodeRef nodeRef, AnnotationDef annotationDef,
            string annotationValue)
        {
            var identityPath = ToIdentityPath(nodeRef);
            var docNode = document.FindNode(identityPath);
            docNode = docNode.ChangeAnnotations(SetAnnotationValue(docNode.Annotations, annotationDef, annotationValue));
            document = (SrmDocument) document.ReplaceChild(identityPath.Parent, docNode);
            return document;
        }

        public SrmDocument SetReplicateAttribute(SrmDocument document, ReplicateRef replicateRef,
            AnnotationDef annotationDef, string annotationValue)
        {
            var measuredResults = document.MeasuredResults;
            if (measuredResults == null)
            {
                throw ElementNotFoundException(replicateRef);
            }
            for (int i = 0; i < measuredResults.Chromatograms.Count; i++)
            {
                var chromSet = measuredResults.Chromatograms[i];
                if (replicateRef.Matches(chromSet))
                {
                    chromSet = chromSet.ChangeAnnotations(SetAnnotationValue(chromSet.Annotations, annotationDef,
                        annotationValue));
                    var newChromatograms = measuredResults.Chromatograms.ToArray();
                    newChromatograms[i] = chromSet;
                    return document.ChangeMeasuredResults(measuredResults.ChangeChromatograms(newChromatograms));
                }
            }
            throw ElementNotFoundException(replicateRef);
        }

        public SrmDocument SetResultAttribute(SrmDocument document, ResultRef resultRef, AnnotationDef annotationDef,
            string annotationValue)
        {
            var measuredResults = document.MeasuredResults;
            if (measuredResults == null)
            {
                throw ElementNotFoundException(resultRef);
            }
            var nodeRef = (NodeRef)resultRef.Parent;
            var identityPath = ToIdentityPath(nodeRef);
            var docNode = document.FindNode(identityPath);
            for (int replicateIndex = 0; replicateIndex < measuredResults.Chromatograms.Count; replicateIndex++)
            {
                var chromSet = measuredResults.Chromatograms[replicateIndex];
                if (resultRef.Matches(chromSet))
                {
                    var transitionGroup = docNode as TransitionGroupDocNode;
                    if (transitionGroup != null)
                    {
                        var results = transitionGroup.Results.ToArray();
                        results[replicateIndex] = SetChromInfoAnnotation(chromSet, (PrecursorResultRef) resultRef,
                            results[replicateIndex], annotationDef, annotationValue);
                        docNode = ((TransitionGroupDocNode) docNode).ChangeResults(
                            new Results<TransitionGroupChromInfo>(results));
                    }
                    else
                    {
                        var transition = docNode as TransitionDocNode;
                        if (transition != null)
                        {
                            var results = transition.Results.ToArray();
                            results[replicateIndex] = SetChromInfoAnnotation(chromSet, (TransitionResultRef) resultRef,
                                results[replicateIndex], annotationDef, annotationValue);
                            docNode = ((TransitionDocNode) docNode).ChangeResults(
                                new Results<TransitionChromInfo>(results));
                        }
                        else
                        {
                            throw AnnotationDoesNotApplyException(annotationDef, resultRef);
                        }
                    }
                    return (SrmDocument) document.ReplaceChild(identityPath.Parent, docNode);
                }
            }
            throw ElementNotFoundException(resultRef);
        }

        private ChromInfoList<TChromInfo> SetChromInfoAnnotation<TDocNode, TChromInfo>(ChromatogramSet chromatogramSet,
            ResultRef<TDocNode, TChromInfo> resultRef, ChromInfoList<TChromInfo> chromInfoList,
            AnnotationDef annotationDef, string value) where TDocNode : DocNode where TChromInfo : ChromInfo
        {
            var chromFileInfo = resultRef.FindChromFileInfo(chromatogramSet);
            for (int i = 0; i < chromInfoList.Count; i++)
            {
                var chromInfo = chromInfoList[i];
                if (!ReferenceEquals(chromInfo.FileId, chromFileInfo.Id))
                {
                    continue;
                }
                if (resultRef.GetOptimizationStep(chromInfo) != 0)
                {
                    continue;
                }
                var annotations = resultRef.GetAnnotations(chromInfo);
                annotations = SetAnnotationValue(annotations, annotationDef, value);
                chromInfo = resultRef.ChangeAnnotations(chromInfo, annotations);
                return chromInfoList.ChangeAt(i, chromInfo);
            }
            throw ElementNotFoundException(resultRef);
        }

        private int EnsureColumn(DsvFileReader fileReader, string name)
        {
            int icol = fileReader.GetFieldIndex(name);
            if (icol < 0)
            {
                throw new InvalidDataException(string.Format("Missing column '{0}'.", name));
            }
            return icol;
        }

        private IdentityPath ToIdentityPath(NodeRef nodeRef)
        {
            IdentityPath identityPath;
            if (_identityPaths.TryGetValue(nodeRef, out identityPath))
            {
                return identityPath;
            }
            identityPath = nodeRef.ToIdentityPath(Document);
            if (identityPath == null)
            {
                throw ElementNotFoundException(nodeRef);
            }
            _identityPaths.Add(nodeRef, identityPath);
            return identityPath;
        }

        private Exception ElementNotFoundException(ElementRef elementRef)
        {
            return new InvalidDataException(string.Format("Could not find element '{0}'.", elementRef));
        }

        private Exception AnnotationDoesNotApplyException(AnnotationDef annotationDef, ElementRef elementRef)
        {
            return new InvalidDataException(string.Format("Annotation '{0}' does not apply to element '{1}'.",
                annotationDef.Name, elementRef));
        }

        private Annotations SetAnnotationValue(Annotations annotations, AnnotationDef annotationDef, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return annotations.RemoveAnnotation(annotationDef.Name);
            }
            string persistedValue;
            switch (annotationDef.Type)
            {
                case AnnotationDef.AnnotationType.number:
                    persistedValue = annotationDef.ToPersistedString(double.Parse(value, CultureInfo));
                    break;
                case AnnotationDef.AnnotationType.true_false:
                    persistedValue = annotationDef.ToPersistedString(true);
                    break;
                default:
                    persistedValue = value;
                    break;
            }
            return annotations.ChangeAnnotation(annotationDef.Name, persistedValue);
        }
    }
}
