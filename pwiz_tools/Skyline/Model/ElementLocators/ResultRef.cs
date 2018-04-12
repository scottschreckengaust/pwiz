using System;
using System.Collections.Generic;
using System.Linq;
using pwiz.Skyline.Model.Results;

namespace pwiz.Skyline.Model.ElementLocators
{
    public abstract class ResultRef : ElementRef
    {
        private const string ATTR_FILENAME = "filename";
        private const string ATTR_FILEPATH = "filepath";

        protected ResultRef(NodeRef parent) : base(parent)
        {
            
        }

        public int OptimizationStep { get; private set; }

        public ResultRef ChangeOptimizationStep(int optimizationStep)
        {
            return ChangeProp(ImClone(this), im => im.OptimizationStep = optimizationStep);
        }
        public string ResultFileName { get; private set; }

        public ResultRef ChangeResultFileName(string resultFileName)
        {
            return ChangeProp(ImClone(this), im => im.ResultFileName = resultFileName);
        }
        public MsDataFileUri ResultFilePath { get; private set; }

        public ResultRef ChangeResultFilePath(MsDataFileUri resultFilePath)
        {
            return ChangeProp(ImClone(this), im => im.ResultFilePath = resultFilePath);
        }

        private bool Matches(MsDataFileUri msDataFilePath)
        {
            if (ResultFileName != null && !Equals(ResultFileName, ResultFileRef.GetName(msDataFilePath)))
            {
                return false;
            }
            if (ResultFilePath != null && !Equals(ResultFilePath, msDataFilePath))
            {
                return false;
            }
            return true;
        }

        public ChromFileInfo FindChromFileInfo(SrmDocument document)
        {
            var measuredResults = document.Settings.MeasuredResults;
            if (measuredResults == null)
            {
                return null;
            }
            var chromatogramSet = measuredResults.Chromatograms.FirstOrDefault(chromSet => chromSet.Name == Name);
            if (chromatogramSet == null)
            {
                return null;
            }
            return chromatogramSet.MSDataFileInfos.FirstOrDefault(info => Matches(info.FilePath));
        }

        protected bool Equals(ResultRef other)
        {
            return base.Equals(other) && OptimizationStep == other.OptimizationStep && string.Equals(ResultFileName, other.ResultFileName) && Equals(ResultFilePath, other.ResultFilePath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ResultRef) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ OptimizationStep;
                hashCode = (hashCode * 397) ^ (ResultFileName != null ? ResultFileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ResultFilePath != null ? ResultFilePath.GetHashCode() : 0);
                return hashCode;
            }
        }
        protected override IEnumerable<KeyValuePair<string, string>> GetAttributes()
        {
            var result = base.GetAttributes();
            if (ResultFileName != null)
            {
                result = new[] { new KeyValuePair<string, string>(ATTR_FILENAME, ResultFileName) }
                    .Concat(result);
            }
            if (ResultFilePath != null)
            {
                result = new[] { new KeyValuePair<string, string>(ATTR_FILEPATH, ResultFilePath.ToString()) }
                    .Concat(result);
            }
            return result;
        }

    }
    public abstract class ResultRef<TDocNode, TChromInfo> : ResultRef
        where TDocNode : DocNode
        where TChromInfo : ChromInfo
    {
        protected ResultRef(NodeRef<TDocNode> parent)
            : base(parent)
        {

        }

        public new NodeRef<TDocNode> Parent { get { return (NodeRef<TDocNode>)base.Parent; } }

        protected override IEnumerable<ElementRef> EnumerateSiblings(SrmDocument document)
        {
            var measuredResults = document.Settings.MeasuredResults;
            var parentDocNode = (TDocNode)Parent.FindNode(document);
            if (parentDocNode == null || measuredResults == null)
            {
                yield break;
            }

            foreach (var replicateIndexChromInfo in EnumerateChromInfos(parentDocNode))
            {
                var chromatogramSet = measuredResults.Chromatograms[replicateIndexChromInfo.Item1];
                yield return ChangeChromInfo(chromatogramSet, replicateIndexChromInfo.Item2);
            }
        }

        protected abstract IEnumerable<Tuple<int, TChromInfo>> EnumerateChromInfos(TDocNode parent);
        protected abstract int GetOptimizationStep(TChromInfo chromInfo);


        public ResultRef<TDocNode, TChromInfo> ChangeChromInfo(ChromatogramSet chromatogramSet, TChromInfo chromInfo)
        {
            var result = (ResultRef<TDocNode, TChromInfo>)ChangeName(chromatogramSet.Name);
            if (chromatogramSet.FileCount > 1)
            {
                var chromFileInfo = chromatogramSet.GetFileInfo(chromInfo.FileId);
                if (ResultFileRef.UseFullPath(chromatogramSet))
                {
                    result = (ResultRef<TDocNode, TChromInfo>)result.ChangeResultFilePath(chromFileInfo.FilePath);
                }
                else
                {
                    result = (ResultRef<TDocNode, TChromInfo>)result.ChangeResultFileName(ResultFileRef.GetName(chromFileInfo.FilePath));
                }
            }
            return result;
        }
    }

    public class TransitionResultRef : ResultRef<TransitionDocNode, TransitionChromInfo>
    {
        public static readonly TransitionResultRef PROTOTYPE = new TransitionResultRef();

        private TransitionResultRef()
            : base(TransitionRef.PROTOTYPE)
        {

        }

        public override string DocKeyType
        {
            get { return "TransitionResult"; }
        }

        protected override IEnumerable<Tuple<int, TransitionChromInfo>> EnumerateChromInfos(TransitionDocNode parent)
        {
            if (null == parent.Results)
            {
                yield break;
            }
            for (int i = 0; i < parent.Results.Count; i++)
            {
                foreach (var chromInfo in parent.Results[i])
                {
                    yield return Tuple.Create(i, chromInfo);
                }
            }
        }

        protected override int GetOptimizationStep(TransitionChromInfo chromInfo)
        {
            return chromInfo.OptimizationStep;
        }
    }

    public class MoleculeResultRef : ResultRef<PeptideDocNode, PeptideChromInfo>
    {
        public static readonly MoleculeResultRef PROTOTYPE = new MoleculeResultRef();
        private MoleculeResultRef()
            : base(MoleculeRef.PROTOTYPE)
        {
        }

        public override string DocKeyType
        {
            get { return "MoleculeResult"; }
        }

        protected override IEnumerable<Tuple<int, PeptideChromInfo>> EnumerateChromInfos(PeptideDocNode parent)
        {
            if (parent.Results == null)
            {
                yield break;
            }
            for (int i = 0; i < parent.Results.Count; i++)
            {
                foreach (var peptideChromInfo in parent.Results[i])
                {
                    yield return Tuple.Create(i, peptideChromInfo);
                }
            }
        }

        protected override int GetOptimizationStep(PeptideChromInfo chromInfo)
        {
            return 0;
        }
    }
}