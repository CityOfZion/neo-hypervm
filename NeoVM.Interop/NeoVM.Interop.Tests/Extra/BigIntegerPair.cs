using System.Numerics;

namespace NeoVM.Interop.Tests.Extra
{
    /// <summary>
    /// Contains two BigInteger values
    /// </summary>
    public class BigIntegerPair
    {
        public readonly BigInteger A, B;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        public BigIntegerPair(BigInteger a, BigInteger b)
        {
            A = a;
            B = b;
        }
    }
}