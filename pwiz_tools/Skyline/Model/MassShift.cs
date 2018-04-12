using pwiz.Skyline.Model.Hibernate;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Model
{
    public struct MassShift
    {
        public static readonly MassShift ZERO = new MassShift();
        public MassShift(double mass) : this(mass, mass)
        {
                
        }

        public MassShift(double mono, double average) : this()
        {
            Mono = mono;
            Average = average;
        }
        public double Mono { get; private set; }
        public double Average { get; private set; }

        public MassShift Add(MassShift massShift)
        {
            return new MassShift(Mono + massShift.Mono, Average + massShift.Average);
        }

        public override string ToString()
        {
            if (Mono == Average)
            {
                return Mono.ToString(Formats.RoundTrip);
            }
            return "[" + Mono.ToString(Formats.RoundTrip) + "," + Average.ToString(Formats.RoundTrip) + "]";
        }
    }
}