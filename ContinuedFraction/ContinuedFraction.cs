using System;
using System.Linq;
using System.Numerics;
using System.Globalization;
using System.Collections.Generic;

namespace ExtendedNumerics
{

	public class ContinuedFraction
	{
		public BigInteger[] Terms { get; private set; }

		public ContinuedFraction(BigInteger[] terms)
		{
			Terms = terms;
		}

		public ContinuedFraction(BigDecimal value, int precision = -1)
			: this(FromBigDecimal_Internal(value, precision))
		{
		}

		public ContinuedFraction(string decimalDigitString, int precision = -1)
			: this(BigDecimal.Parse(decimalDigitString), precision)
		{
		}

		public ContinuedFraction(double value, int precision = -1)
			: this(new BigDecimal(value), precision)
		{
		}

		public ContinuedFraction(float value, int precision = -1)
			: this(new BigDecimal(value), precision)
		{
		}

		public static ContinuedFraction FromBigRational(BigRational value, int precision = -1)
		{
			return FromFraction(value.GetImproperFraction(), precision);
		}

		public static ContinuedFraction FromFraction(Fraction value, int precision = -1)
		{
			return FromBigDecimal(new BigDecimal(value.Numerator, value.Denominator), precision);
		}

		public static ContinuedFraction FromString(string decimalDigitString, int precision = -1)
		{
			return FromBigDecimal(BigDecimal.Parse(decimalDigitString), precision);
		}

		public static ContinuedFraction FromBigDecimal(BigDecimal value, int precision = -1)
		{
			return new ContinuedFraction(FromBigDecimal_Internal(value, precision));
		}

		protected static BigInteger[] FromBigDecimal_Internal(BigDecimal value, int precision = -1)
		{
			BigDecimal currentValue = value;
			List<BigInteger> result = new List<BigInteger>();

			if (currentValue.ToString().Count() < 3)
			{
				return result.ToArray();
			}
			if (BigInteger.Abs(currentValue.WholeValue) >= 0)
			{
				result.Add(currentValue.WholeValue);
				currentValue = currentValue.GetFractionalPart();
			}

			int prec = precision;
			string len = currentValue.ToString().Substring(2);
			if (prec <= 0)
			{
				prec = Math.Min(BigDecimal.Precision, len.Count() + 1);
				if (prec % 2 == 1)
				{
					prec -= 1;
				}
			}

			BigDecimal next = 0;
			BigDecimal last = 0;

			int currentValue_TensColumn = 1;
			int counter = prec;
			do
			{
				currentValue = BigDecimal.One / currentValue;
				next = new BigDecimal(currentValue.WholeValue);
				currentValue = BigDecimal.Subtract(currentValue, next);

				if (next > 0 && currentValue > 0)
				{
					BigInteger leftOfDecimalPoint = (BigInteger)next;
					result.Add(leftOfDecimalPoint);
					counter -= next.ToString().Count();
				}
				else
				{
					break;
				}

				currentValue_TensColumn = int.Parse(currentValue.ToString().Substring(2, 1));
				last = new BigDecimal(next.Mantissa, next.Exponent);
			}
			while (next > 0 && currentValue > 0 && counter > 0);

			if (result.Count > 1 && last == 1 && currentValue_TensColumn != 0)
			{
				BigInteger final = result.Last();
				result.RemoveAt(result.Count - 1);
				BigInteger temp = (BigInteger)last;
				result.Add(final + temp);
			}

			return result.ToArray();
		}

		public static BigRational ToBigRational(ContinuedFraction continuedFraction, int termsToInclude = -1)
		{
			return new BigRational(ToFraction(continuedFraction, termsToInclude));
		}

		public static Fraction ToFraction(ContinuedFraction continuedFraction, int termsToInclude = -1)
		{
			List<BigInteger> fraction = continuedFraction.Terms.Reverse().ToList();

			if (termsToInclude <= 0)
			{
				fraction = fraction.Take(termsToInclude).ToList();
			}

			BigInteger numerator = 0;
			BigInteger denominator = 1;
			foreach (BigInteger digit in fraction)
			{
				if (numerator == 0)
				{
					numerator = digit;
				}
				else
				{
					Swap(ref numerator, ref denominator);
					numerator += digit * denominator;
				}
			}
			return new Fraction(numerator, denominator);
		}

		private static void Swap(ref BigInteger left, ref BigInteger right)
		{
			BigInteger swap = left;
			left = right;
			right = swap;
		}

		public override string ToString()
		{
			if (!Terms.Any()) return "(empty)";
			return $"[{Terms.First()};{string.Join(",", Terms.Skip(1).Select(n => n.ToString()))}]";
		}
	}
}
