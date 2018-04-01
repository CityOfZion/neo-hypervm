#pragma once
#include <vector>

// Ported from 
// - https://referencesource.microsoft.com/#System.Numerics/System/Numerics/BigInteger.cs

class BigInteger
{
public:

	const static BigInteger Min;
	const static BigInteger One;
	const static BigInteger Zero;
	const static BigInteger MinusOne;

	BigInteger(int value);
	BigInteger(BigInteger *value);
	BigInteger(unsigned __int32 value);
	BigInteger(unsigned __int32 * value, int size);
	BigInteger(unsigned char * value, int byteCount);

	BigInteger *Clone();
	void CopyInternal(__int32 sign, unsigned __int32 *bits, __int32 bitSize);

	bool ToInt32(int &ret);
	int ToByteArraySize();
	int ToByteArray(unsigned char * output, int length);

	BigInteger* Add(BigInteger* &other);
	BigInteger* Sub(BigInteger* &other);
	BigInteger* And(BigInteger* &other);
	BigInteger* Or(BigInteger* &other);
	BigInteger* Xor(BigInteger* &other);
	BigInteger* Negate();
	BigInteger* Abs();
	int GetSign();

	int CompareTo(BigInteger bi);
	int CompareTo(BigInteger *bi);

	~BigInteger();

private:

	const static __int32 Int32MaxValue = 0x7FFFFFFF;
	const static __int32 Int32MinValue = 0x80000000;
	const static __int32 UInt32MaxValue = 0xFFFFFFFF;
	const static __int32 UInt32MinValue = 0x00000000;

	const static __int32 knMaskHighBit = Int32MinValue;
	const static unsigned __int32 kuMaskHighBit = Int32MinValue;
	const static __int32 kcbitUint = 32;
	const static __int32 kcbitUlong = 64;
	const static __int32 DecimalScaleFactorMask = 0x00FF0000;
	const static __int32 DecimalSignMask = 0x80000000;

	__int32 _sign;
	unsigned __int32 * _bits;
	__int32 _bitsSize;

	void DangerousMakeTwosComplement(unsigned __int32 *d, int dSize);
	int ToUInt32Array(unsigned __int32 * &output);
	int Length(unsigned __int32 *rgu, int size);
	int GetDiffLength(unsigned __int32 *rgu1, unsigned __int32 * rgu2, int cu);
	BigInteger(int sign, unsigned __int32 *rgu, int rguSize);
};