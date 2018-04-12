using System.Collections.Generic;
using System.Linq;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.Results;

namespace pwiz.Skyline.Model.ElementLocators
{
    public class ReplicateRef : ElementRef
    {
        public static readonly ReplicateRef PROTOTYPE = new ReplicateRef();
        private ReplicateRef() : base(DocumentRef.PROTOTYPE)
        {
        }

        public override string ElementType
        {
            get { return "Replicate"; }
        }

        public ChromatogramSet FindChromatogramSet(SrmDocument document)
        {
            var measuredResults = document.Settings.MeasuredResults;
            if (measuredResults == null)
            {
                return null;
            }
            return measuredResults.Chromatograms.FirstOrDefault(Matches);
        }

        public bool Matches(ChromatogramSet chromatogramSet)
        {
            return chromatogramSet.Name == Name;
        }

        protected override IEnumerable<ElementRef> EnumerateSiblings(SrmDocument document)
        {
            var measuredResults = document.Settings.MeasuredResults;
            if (measuredResults == null)
            {
                yield break;
            }
            foreach (var chromatogramSet in measuredResults.Chromatograms)
            {
                yield return ChangeName(chromatogramSet.Name);
            }
        }

        public static ReplicateRef FromChromatogramSet(ChromatogramSet chromatogramSet)
        {
            return (ReplicateRef) PROTOTYPE.ChangeName(chromatogramSet.Name);
        }

        public override AnnotationDef.AnnotationTargetSet AnnotationTargets
        {
            get { return AnnotationDef.AnnotationTargetSet.Singleton(AnnotationDef.AnnotationTarget.replicate); }
        }
    }

    public class ResultFileRef : ElementRef
    {
        public static readonly ResultFileRef PROTOTYPE = new ResultFileRef();
        private ResultFileRef()
            : base(ReplicateRef.PROTOTYPE)
        {

        }

        public override string ElementType
        {
            get { return "ResultFile"; }
        }

        public MsDataFileUri FilePath { get; private set; }

        public ResultFileRef ChangeFilePath(MsDataFileUri filePath)
        {
            return ChangeProp(ImClone(this), im => im.FilePath = filePath);
        }

        protected override IEnumerable<ElementRef> EnumerateSiblings(SrmDocument document)
        {
            var chromatogramSet = ((ReplicateRef)Parent).FindChromatogramSet(document);
            if (chromatogramSet == null)
            {
                yield break;
            }
            bool useFullPath = UseFullPath(chromatogramSet);
            foreach (var filePath in chromatogramSet.MSDataFilePaths)
            {
                var resultFileRef = (ResultFileRef)ChangeName(GetName(filePath));
                if (useFullPath)
                {
                    yield return resultFileRef.ChangeFilePath(filePath);
                }
                else
                {
                    yield return resultFileRef;
                }
            }
        }

        public static bool UseFullPath(ChromatogramSet chromatogramSet)
        {
            if (chromatogramSet.FileCount <= 1)
            {
                return false;
            }
            return chromatogramSet.MSDataFilePaths.Select(GetName).Distinct().Count() !=
                   chromatogramSet.FileCount;
        }

        public static string GetName(MsDataFileUri msDataFilePath)
        {
            return msDataFilePath.GetFileName();
        }

        protected bool Equals(ResultFileRef other)
        {
            return base.Equals(other) && Equals(FilePath, other.FilePath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ResultFileRef)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (FilePath != null ? FilePath.GetHashCode() : 0);
            }
        }
    }
}