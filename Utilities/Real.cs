using System;
using System.Collections;
using System.Collections.Generic;

namespace Neon.Utilities {
    /// <summary>
    /// A Real that can be serialized.
    /// </summary>
    [Serializable]
    public class SerializedReal {
        public static implicit operator SerializedReal(float value) {
            SerializedReal serialized = new SerializedReal();
            serialized.Real = Real.Create(value);
            return serialized;
        }

        /// <summary>
        /// The high 32 bits in the long value
        /// </summary>
        public int LongHigh;

        /// <summary>
        /// The lower 32 bits in the long value
        /// </summary>
        public int LongLow;

        /// <summary>
        /// Converts a set of low and a set of high bits to a long.
        /// </summary>
        public static long ToLong(int longHigh, int longLow) {
            return (long)(longHigh * uint.MaxValue) + longLow;
        }

        /// <summary>
        /// Returns the high 32 bits of a long.
        /// </summary>
        public static int LongToHigh(long value) {
            return (int)(value / uint.MaxValue);
        }

        /// <summary>
        /// Returns the low 32 bits of a long.
        /// </summary>
        public static int LongToLow(long value) {
            return (int)value;
        }

        /// <summary>
        /// Returns the Real instance from the SerializedLong.
        /// </summary>
        public Real Real {
            get {
                return Real.Create(ToLong(LongHigh, LongLow), false);
            }
            set {
                LongHigh = LongToHigh(value.RawValue);
                LongLow = LongToLow(value.RawValue);
            }
        }
    }

    [Serializable]
    public struct Real {
        public long RawValue;
        public const int SHIFT_AMOUNT = 12; //12 is 4096

        public const long One = 1 << SHIFT_AMOUNT;
        public const int OneI = 1 << SHIFT_AMOUNT;
        public static Real OneF = Real.Create(1, true);

        #region Constructors
        public static implicit operator Real(float value) {
            return Real.Create(value);
        }

        public static implicit operator Real(double value) {
            return Real.Create((float)value);
        }

        public static Real CreateFromRaw(long StartingRawValue) {
            Real real;
            real.RawValue = StartingRawValue;
            return real;
        }

        /// <summary>
        /// Assuming this real has a base 10 representation, this shifts the decimal value to the
        /// left by count digits.
        /// </summary>
        private void ShiftDecimal(int count) {
            int digitBase = 10;

            int pow = 1;
            for (int i = 0; i < count; ++i) {
                pow *= digitBase;
            }

            this /= (OneF * pow);
        }

        /// <summary>
        /// Returns the number of digits in the given value. The negative sign is not considered
        /// a digit.
        /// </summary>
        private static int GetDigitCount(int number) {
            // remark: we compare against 10, and not 0, in this function because 0 has one digit

            int digits = 1;
            while (number >= 10) {
                number /= 10;
                digits += 1;
            }
            return digits;
        }

        /// <summary>
        /// Creates a real value with that is 0.number. For example, CreateDecimal(123) will create
        /// a real value that is equal to "0.123".
        /// </summary>
        /// <param name="number">The decimal number to create</param>
        /// <returns></returns>
        public static Real CreateDecimal(long beforeDecimal, int afterDecimal) {
            Contract.Requires(afterDecimal >= 0, "Cannot have a negative decimal portion");

            int sign = beforeDecimal >= 0 ? 1 : -1;

            Real real;
            real.RawValue = (One * afterDecimal) * sign;
            real.ShiftDecimal(GetDigitCount(afterDecimal));
            real.RawValue += One * beforeDecimal;
            return real;
        }

        public static Real CreateDecimal(long beforeDecimal) {
            Real real;
            real.RawValue = One * beforeDecimal;
            return real;
        }

        public static Real Create(long StartingRawValue, bool UseMultiple) {
            Real fInt;
            fInt.RawValue = StartingRawValue;
            if (UseMultiple)
                fInt.RawValue = fInt.RawValue << SHIFT_AMOUNT;
            return fInt;
        }
        public static Real Create(double DoubleValue) {
            Real fInt;
            DoubleValue *= (double)One;
            fInt.RawValue = (int)Math.Round(DoubleValue);
            return fInt;
        }
        #endregion

        public int AsInt {
            get {
                return (int)(this.RawValue >> SHIFT_AMOUNT);
            }
        }

        public float AsFloat {
            get {
                return (float)this.RawValue / (float)One;
            }
        }

        public Real Inverse {
            get { return Real.Create(-this.RawValue, false); }
        }

        #region FromParts
        /// <summary>
        /// Create a fixed-int number from parts.  For example, to create 1.5 pass in 1 and 500.
        /// </summary>
        /// <param name="PreDecimal">The number above the decimal.  For 1.5, this would be 1.</param>
        /// <param name="PostDecimal">The number below the decimal, to three digits.  
        /// For 1.5, this would be 500. For 1.005, this would be 5.</param>
        /// <returns>A fixed-int representation of the number parts</returns>
        public static Real FromParts(int PreDecimal, int PostDecimal) {
            Real f = Real.Create(PreDecimal, true);
            if (PostDecimal != 0)
                f.RawValue += (Real.Create(PostDecimal) / 1000).RawValue;

            return f;
        }
        #endregion

        #region *
        public static Real operator *(Real one, Real other) {
            Real fInt;
            fInt.RawValue = (one.RawValue * other.RawValue) >> SHIFT_AMOUNT;
            return fInt;
        }

        public static Real operator *(Real one, int multi) {
            return one * (Real)multi;
        }

        public static Real operator *(int multi, Real one) {
            return one * (Real)multi;
        }
        #endregion

        #region /
        public static Real operator /(Real one, Real other) {
            Real fInt;
            fInt.RawValue = (one.RawValue << SHIFT_AMOUNT) / (other.RawValue);
            return fInt;
        }

        public static Real operator /(Real one, int divisor) {
            return one / (Real)divisor;
        }

        public static Real operator /(int divisor, Real one) {
            return (Real)divisor / one;
        }
        #endregion

        #region %
        public static Real operator %(Real one, Real other) {
            Real fInt;
            fInt.RawValue = (one.RawValue) % (other.RawValue);
            return fInt;
        }

        public static Real operator %(Real one, int divisor) {
            return one % (Real)divisor;
        }

        public static Real operator %(int divisor, Real one) {
            return (Real)divisor % one;
        }
        #endregion

        #region +
        public static Real operator +(Real one, Real other) {
            Real fInt;
            fInt.RawValue = one.RawValue + other.RawValue;
            return fInt;
        }

        public static Real operator +(Real one, int other) {
            return one + (Real)other;
        }

        public static Real operator +(int other, Real one) {
            return one + (Real)other;
        }
        #endregion

        #region -
        public static Real operator -(Real a) {
            return a.Inverse;
        }

        public static Real operator -(Real one, Real other) {
            Real fInt;
            fInt.RawValue = one.RawValue - other.RawValue;
            return fInt;
        }

        public static Real operator -(Real one, int other) {
            return one - (Real)other;
        }

        public static Real operator -(int other, Real one) {
            return (Real)other - one;
        }
        #endregion

        #region ==
        public static bool operator ==(Real one, Real other) {
            return one.RawValue == other.RawValue;
        }

        public static bool operator ==(Real one, int other) {
            return one == (Real)other;
        }

        public static bool operator ==(int other, Real one) {
            return (Real)other == one;
        }
        #endregion

        #region !=
        public static bool operator !=(Real one, Real other) {
            return one.RawValue != other.RawValue;
        }

        public static bool operator !=(Real one, int other) {
            return one != (Real)other;
        }

        public static bool operator !=(int other, Real one) {
            return (Real)other != one;
        }
        #endregion

        #region >=
        public static bool operator >=(Real one, Real other) {
            return one.RawValue >= other.RawValue;
        }

        public static bool operator >=(Real one, int other) {
            return one >= (Real)other;
        }

        public static bool operator >=(int other, Real one) {
            return (Real)other >= one;
        }
        #endregion

        #region <=
        public static bool operator <=(Real one, Real other) {
            return one.RawValue <= other.RawValue;
        }

        public static bool operator <=(Real one, int other) {
            return one <= (Real)other;
        }

        public static bool operator <=(int other, Real one) {
            return (Real)other <= one;
        }
        #endregion

        #region >
        public static bool operator >(Real one, Real other) {
            return one.RawValue > other.RawValue;
        }

        public static bool operator >(Real one, int other) {
            return one > (Real)other;
        }

        public static bool operator >(int other, Real one) {
            return (Real)other > one;
        }
        #endregion

        #region <
        public static bool operator <(Real one, Real other) {
            return one.RawValue < other.RawValue;
        }

        public static bool operator <(Real one, int other) {
            return one < (Real)other;
        }

        public static bool operator <(int other, Real one) {
            return (Real)other < one;
        }
        #endregion

        public static explicit operator int(Real src) {
            return (int)(src.RawValue >> SHIFT_AMOUNT);
        }

        public static explicit operator Real(int src) {
            return Real.Create(src, true);
        }

        public static explicit operator Real(long src) {
            return Real.Create(src, true);
        }

        public static explicit operator Real(ulong src) {
            return Real.Create((long)src, true);
        }

        public static Real operator <<(Real one, int Amount) {
            return Real.Create(one.RawValue << Amount, false);
        }

        public static Real operator >>(Real one, int Amount) {
            return Real.Create(one.RawValue >> Amount, false);
        }

        public override bool Equals(object obj) {
            if (obj is Real)
                return ((Real)obj).RawValue == this.RawValue;
            else
                return false;
        }

        public override int GetHashCode() {
            return RawValue.GetHashCode();
        }

        public override string ToString() {
            return string.Format("{0}", AsFloat);
        }

        #region PI, DoublePI
        public static Real PI = Real.Create(12868, false); //PI x 2^12
        public static Real TwoPIF = PI * 2; //radian equivalent of 360 degrees
        public static Real PIOver180F = PI / (Real)180; //PI / 180
        #endregion

        #region Sqrt
        public static Real Sqrt(Real f, int NumberOfIterations) {
            if (f.RawValue < 0) //NaN in Math.Sqrt
                throw new ArithmeticException("Input Error");
            if (f.RawValue == 0)
                return (Real)0;
            Real k = f + Real.OneF >> 1;
            for (int i = 0; i < NumberOfIterations; i++)
                k = (k + (f / k)) >> 1;

            if (k.RawValue < 0)
                throw new ArithmeticException("Overflow");
            else
                return k;
        }

        public static Real Sqrt(Real f) {
            byte numberOfIterations = 8;
            if (f.RawValue > 0x64000)
                numberOfIterations = 12;
            if (f.RawValue > 0x3e8000)
                numberOfIterations = 16;
            return Sqrt(f, numberOfIterations);
        }
        #endregion

        #region Sin
        public static Real Sin(Real i) {
            Real j = (Real)0;
            for (; i < 0; i += Real.Create(25736, false))
                ;
            if (i > Real.Create(25736, false))
                i %= Real.Create(25736, false);
            Real k = (i * Real.Create(10, false)) / Real.Create(714, false);
            if (i != 0 && i != Real.Create(6434, false) && i != Real.Create(12868, false) &&
                i != Real.Create(19302, false) && i != Real.Create(25736, false))
                j = (i * Real.Create(100, false)) / Real.Create(714, false) - k * Real.Create(10, false);
            if (k <= Real.Create(90, false))
                return sin_lookup(k, j);
            if (k <= Real.Create(180, false))
                return sin_lookup(Real.Create(180, false) - k, j);
            if (k <= Real.Create(270, false))
                return sin_lookup(k - Real.Create(180, false), j).Inverse;
            else
                return sin_lookup(Real.Create(360, false) - k, j).Inverse;
        }

        private static Real sin_lookup(Real i, Real j) {
            if (j > 0 && j < Real.Create(10, false) && i < Real.Create(90, false))
                return Real.Create(SIN_TABLE[i.RawValue], false) +
                    ((Real.Create(SIN_TABLE[i.RawValue + 1], false) - Real.Create(SIN_TABLE[i.RawValue], false)) /
                    Real.Create(10, false)) * j;
            else
                return Real.Create(SIN_TABLE[i.RawValue], false);
        }

        private static int[] SIN_TABLE = {
        0, 71, 142, 214, 285, 357, 428, 499, 570, 641, 
        711, 781, 851, 921, 990, 1060, 1128, 1197, 1265, 1333, 
        1400, 1468, 1534, 1600, 1665, 1730, 1795, 1859, 1922, 1985, 
        2048, 2109, 2170, 2230, 2290, 2349, 2407, 2464, 2521, 2577, 
        2632, 2686, 2740, 2793, 2845, 2896, 2946, 2995, 3043, 3091, 
        3137, 3183, 3227, 3271, 3313, 3355, 3395, 3434, 3473, 3510, 
        3547, 3582, 3616, 3649, 3681, 3712, 3741, 3770, 3797, 3823, 
        3849, 3872, 3895, 3917, 3937, 3956, 3974, 3991, 4006, 4020, 
        4033, 4045, 4056, 4065, 4073, 4080, 4086, 4090, 4093, 4095, 
        4096
    };
        #endregion

        private static Real mul(Real F1, Real F2) {
            return F1 * F2;
        }

        #region Cos, Tan, Asin
        public static Real Cos(Real i) {
            return Sin(i + Real.Create(6435, false));
        }

        public static Real Tan(Real i) {
            return Sin(i) / Cos(i);
        }

        public static Real Asin(Real F) {
            bool isNegative = F < 0;
            F = Abs(F);

            if (F > Real.OneF)
                throw new ArithmeticException("Bad Asin Input:" + F.AsFloat);

            Real f1 = mul(mul(mul(mul(Real.Create(145103 >> Real.SHIFT_AMOUNT, false), F) -
                Real.Create(599880 >> Real.SHIFT_AMOUNT, false), F) +
                Real.Create(1420468 >> Real.SHIFT_AMOUNT, false), F) -
                Real.Create(3592413 >> Real.SHIFT_AMOUNT, false), F) +
                Real.Create(26353447 >> Real.SHIFT_AMOUNT, false);
            Real f2 = PI / Real.Create(2, true) - (Sqrt(Real.OneF - F) * f1);

            return isNegative ? f2.Inverse : f2;
        }
        #endregion

        #region ATan, ATan2
        public static Real Atan(Real F) {
            return Asin(F / Sqrt(Real.OneF + (F * F)));
        }

        public static Real Atan2(Real F1, Real F2) {
            if (F2.RawValue == 0 && F1.RawValue == 0)
                return (Real)0;

            Real result = (Real)0;
            if (F2 > 0)
                result = Atan(F1 / F2);
            else if (F2 < 0) {
                if (F1 >= 0)
                    result = (PI - Atan(Abs(F1 / F2)));
                else
                    result = (PI - Atan(Abs(F1 / F2))).Inverse;
            }
            else
                result = (F1 >= 0 ? PI : PI.Inverse) / Real.Create(2, true);

            return result;
        }
        #endregion

        #region Abs
        public static Real Abs(Real F) {
            if (F < 0)
                return F.Inverse;
            else
                return F;
        }
        #endregion
    }
}