using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using pwiz.Common.Collections;
using pwiz.Common.DataBinding.Attributes;
using pwiz.Skyline.Model.Hibernate;
using pwiz.Skyline.Util;
using pwiz.Skyline.Util.Extensions;

namespace pwiz.Skyline.Model.Databinding.Entities
{
    public class MzDistribution : IFormattable
    {
        public MzDistribution(IEnumerable<KeyValuePair<double, double>> mzs, double? monoisotopicMz)
        {
            var mzList = mzs.OrderBy(mz => mz.Key).ToArray();
            Mzs = new FormattableList<double>(ImmutableList.ValueOf(mzList.Select(mz => mz.Key)));
            Abundances = new FormattableList<double>(ImmutableList.ValueOf(mzList.Select(mz=>mz.Value)));
            var totalAbundance = mzList.Sum(mz => mz.Value);
            if (totalAbundance != 0)
            {
                var totalMzIntensity = mzList.Sum(mz => mz.Key * mz.Value);
                AverageMz = totalMzIntensity / totalAbundance;
            }
            MonoisotopicMz = monoisotopicMz;
        }
        [Format(Formats.Mz)]
        public FormattableList<double> Mzs { get; private set; }
        [Format(Formats.STANDARD_RATIO)]
        public FormattableList<double> Abundances { get; private set; }
        [Format(Formats.Mz)]
        public double AverageMz { get; private set; }
        [Format(Formats.Mz)]
        public double? MonoisotopicMz { get; private set; }

        public override string ToString()
        {
            return ToString(null, CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (MonoisotopicMz.HasValue)
            {
                return TextUtil.SpaceSeparate("Mono:", MonoisotopicMz.Value.ToString(format, formatProvider),
                    "Average:", AverageMz.ToString(format, formatProvider));
            }
            return TextUtil.SpaceSeparate("Average:", AverageMz.ToString(format, formatProvider));
        }
    }
}
