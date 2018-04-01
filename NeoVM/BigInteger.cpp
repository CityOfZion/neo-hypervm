#include "BigInteger.h"

// Constants

const BigInteger BigInteger::Min = BigInteger(-1, new unsigned __int32[1]{ BigInteger::kuMaskHighBit }, 1);
const BigInteger BigInteger::One = BigInteger(1);
const BigInteger BigInteger::Zero = BigInteger(0);
const BigInteger BigInteger::MinusOne = BigInteger(-1);

BigInteger::BigInteger(int value)
{
	this->_sign = value;
	this->_bits = NULL;
	this->_bitsSize = 0;

	// AssertValid();
}

BigInteger::BigInteger(BigInteger *value)
{
	this->_sign = value->_sign;

	if (value->_bits == NULL || value->_bitsSize <= 0)
	{
		this->_bits = NULL;
		this->_bitsSize = 0;
	}
	else
	{
		this->_bitsSize = value->_bitsSize;
		this->_bits = new unsigned __int32[value->_bitsSize];

		for (int x = 0; x < value->_bitsSize; x++)
			this->_bits[x] = value->_bits[x];
	}

	// AssertValid();
}

void BigInteger::CopyInternal(__int32 sign, unsigned __int32 *bits, __int32 bitSize)
{
	this->_sign = sign;
	this->_bitsSize = bitSize;

	if (bitSize > 0)
	{
		this->_bits = new unsigned __int32[bitSize];

		for (int x = 0; x < bitSize; x++)
			this->_bits[x] = bits[x];
	}
	else
	{
		if (this->_bits != NULL)
		{
			delete[]this->_bits;
			this->_bits = NULL;
		}
	}
}

// Do an in-place twos complement of d and also return the result.
// "Dangerous" because it causes a mutation and needs to be used
// with care for immutable types
void BigInteger::DangerousMakeTwosComplement(unsigned __int32 *d, int dSize)
{
	// first do complement and +1 as long as carry is needed
	int i = 0;
	unsigned __int32 v = 0;
	for (; i < dSize; i++)
	{
		v = ~d[i] + 1;
		d[i] = v;
		if (v != 0) { i++; break; }
	}
	if (v != 0)
	{
		// now ones complement is sufficient
		for (; i < dSize; i++) {
			d[i] = ~d[i];
		}
	}
	else
	{
		//??? this is weird
		//d = resize(d, d.Length + 1);
		//d[d.Length - 1] = 1;
	}
	// return d;
}

BigInteger::BigInteger(unsigned __int32 * value, int size)
{
	if (value == NULL)
	{
		this->_sign = 0;
		this->_bitsSize = 0;
		this->_bits = NULL;
		return;
	}

	int dwordCount = size;
	bool isNegative = dwordCount > 0 && ((value[dwordCount - 1] & 0x80000000) == 0x80000000);

	// Try to conserve space as much as possible by checking for wasted leading uint[] entries 
	while (dwordCount > 0 && value[dwordCount - 1] == 0) dwordCount--;

	if (dwordCount == 0)
	{
		// BigInteger.Zero

		this->_sign = 0;
		this->_bitsSize = 0;
		this->_bits = NULL;

		// AssertValid();
		return;
	}
	if (dwordCount == 1)
	{
		if ((int)value[0] < 0 && !isNegative)
		{
			this->_bits = new unsigned __int32[1]{ value[0] };
			this->_bitsSize = 1;
			this->_sign = +1;
		}
		// handle the special cases where the BigInteger likely fits into _sign
		else if (Int32MinValue == (int)value[0])
		{
			CopyInternal(Min._sign, Min._bits, Min._bitsSize);
		}
		else
		{
			this->_sign = (int)value[0];
			this->_bitsSize = 0;
			this->_bits = NULL;
		}

		//AssertValid();
		return;
	}

	if (!isNegative)
	{
		// handle the simple postive value cases where the input is already in sign magnitude
		if (dwordCount != size)
		{
			this->_sign = +1;
			this->_bitsSize = dwordCount;
			this->_bits = new unsigned __int32[dwordCount];

			for (int x = 0; x < dwordCount; x++)
				this->_bits[x] = value[x];
		}
		// no trimming is possible.  Assign value directly to _bits.  
		else
		{
			this->_sign = +1;
			this->_bitsSize = size;
			this->_bits = new unsigned __int32[size];
			for (int x = 0; x < size; x++)
				this->_bits[x] = value[x];
		}

		// AssertValid();
		return;
	}

	// finally handle the more complex cases where we must transform the input into sign magnitude
	this->DangerousMakeTwosComplement(value, size); // mutates val
	// pack _bits to remove any wasted space after the twos complement

	int len = size;
	while (len > 0 && value[len - 1] == 0) len--;

	// the number is represented by a single dword
	if (len == 1 && ((int)(value[0])) > 0)
	{
		if (value[0] == 1 /* abs(-1) */)
		{
			CopyInternal(MinusOne._sign, MinusOne._bits, MinusOne._bitsSize);
		}
		else if (value[0] == kuMaskHighBit /* abs(Int32.MinValue) */)
		{
			CopyInternal(Min._sign, Min._bits, Min._bitsSize);
		}
		else
		{
			this->_sign = (-1) * ((int)value[0]);
			this->_bitsSize = 0;
			this->_bits = NULL;
		}
	}
	// the number is represented by multiple dwords
	// trim off any wasted uint values when possible
	else if (len != size)
	{
		this->_sign = -1;
		this->_bitsSize = len;
		this->_bits = new unsigned __int32[len];

		for (int x = 0; x < len; x++)
			this->_bits[x] = value[x];
	}
	// no trimming is possible.  Assign value directly to _bits.  
	else
	{
		this->_sign = -1;
		this->_bitsSize = size;
		this->_bits = new unsigned __int32[size];
		for (int x = 0; x < size; x++)
			this->_bits[x] = value[x];
	}

	//AssertValid();
}

BigInteger::BigInteger(unsigned char * value, int byteCount)
{
	if (byteCount <= 0 || value == NULL)
	{
		// BigInteger.Zero
		this->_sign = 0;
		this->_bitsSize = 0;
		this->_bits = NULL;
		return;
	}

	bool isNegative = byteCount > 0 && ((value[byteCount - 1] & 0x80) == 0x80);

	// Try to conserve space as much as possible by checking for wasted leading byte[] entries
	while (byteCount > 0 && value[byteCount - 1] == 0) byteCount--;

	if (byteCount == 0)
	{
		// BigInteger.Zero
		this->_sign = 0;
		this->_bitsSize = 0;
		this->_bits = NULL;
		// AssertValid();
		return;
	}

	if (byteCount <= 4)
	{
		if (isNegative)
			this->_sign = (int)0xffffffff;
		else
			this->_sign = 0;

		for (int i = byteCount - 1; i >= 0; i--)
		{
			this->_sign <<= 8;
			this->_sign |= value[i];
		}

		if (_sign < 0 && !isNegative)
		{
			// int32 overflow
			// example: Int64 value 2362232011 (0xCB, 0xCC, 0xCC, 0x8C, 0x0)
			// can be naively packed into 4 bytes (due to the leading 0x0)
			// it overflows into the int32 sign bit

			this->_bits = new unsigned __int32[1]{ (unsigned __int32)this->_sign };
			this->_sign = +1;
			this->_bitsSize = 1;

			// AssertValid();
			return;
		}
		if (_sign == Int32MinValue)
		{
			this->_sign = -1;
			this->_bits = new unsigned __int32[1]{ BigInteger::kuMaskHighBit };
			this->_bitsSize = 1;

			// AssertValid();
			return;
		}

		this->_bits = NULL;
	}
	else
	{
		int unalignedBytes = byteCount % 4;
		int dwordCount = byteCount / 4 + (unalignedBytes == 0 ? 0 : 1);
		bool isZero = true;
		unsigned __int32 *val = new unsigned __int32[dwordCount]();

		// Copy all dwords, except but don't do the last one if it's not a full four bytes

		int max = dwordCount - (unalignedBytes == 0 ? 0 : 1);
		int curDword = 0, curByte = 3, byteInDword;

		for (; curDword < max; curDword++)
		{
			byteInDword = 0;
			while (byteInDword < 4)
			{
				if (value[curByte] != 0x00) isZero = false;
				val[curDword] <<= 8;
				val[curDword] |= value[curByte];
				curByte--;
				byteInDword++;
			}
			curByte += 8;
		}

		// Copy the last dword specially if it's not aligned
		if (unalignedBytes != 0)
		{
			if (isNegative) val[dwordCount - 1] = 0xffffffff;
			for (curByte = byteCount - 1; curByte >= byteCount - unalignedBytes; curByte--)
			{
				if (value[curByte] != 0x00) isZero = false;
				val[curDword] <<= 8;
				val[curDword] |= value[curByte];
			}
		}

		if (isZero)
		{
			this->_sign = 0;
			this->_bitsSize = 0;
			this->_bits = NULL;
		}
		else if (isNegative)
		{
			this->DangerousMakeTwosComplement(val, dwordCount); // mutates val
			// pack _bits to remove any wasted space after the twos complement

			int len = dwordCount;
			while (len > 0 && val[len - 1] == 0)
				len--;

			if (len == 1 && ((int)(val[0])) > 0)
			{
				if (val[0] == 1) // abs(-1)
				{
					this->_sign = -1;
					this->_bits = NULL;
					this->_bitsSize = 0;
				}
				else if (val[0] == kuMaskHighBit) // abs(Int32.MinValue)
				{
					this->_sign = -1;
					this->_bits = new unsigned __int32[1]{ BigInteger::kuMaskHighBit };
					this->_bitsSize = 1;
				}
				else
				{
					this->_sign = (-1) * ((int)val[0]);
					this->_bits = NULL;
					this->_bitsSize = 0;
				}
			}
			else if (len != dwordCount)
			{
				this->_sign = -1;
				this->_bits = new unsigned __int32[len];
				this->_bitsSize = len;

				for (int x = 0; x < len; x++)
					this->_bits[x] = val[x];
			}
			else
			{
				this->_sign = -1;
				this->_bits = val;
				this->_bitsSize = dwordCount;
			}
		}
		else
		{
			this->_sign = +1;
			this->_bits = val;
			this->_bitsSize = dwordCount;
		}

		// Free resources

		if (this->_bits != val)
			delete(val);
	}

	// AssertValid();
}

BigInteger* BigInteger::Clone()
{
	return new BigInteger(this->_sign, this->_bits, this->_bitsSize);
}

int BigInteger::Length(unsigned __int32 *rgu, int size)
{
	if (rgu[size - 1] != 0)
		return size;

	//Contract.Assert(cu >= 2 && rgu[cu - 2] != 0);
	return size - 1;
}

int BigInteger::GetDiffLength(unsigned __int32 *rgu1, unsigned __int32 * rgu2, int cu)
{
	for (int iv = cu; --iv >= 0; )
	{
		if (rgu1[iv] != rgu2[iv])
			return iv + 1;
	}
	return 0;
}

int BigInteger::ToUInt32Array(unsigned __int32 * &output)
{
	if (this->_bits == NULL && this->_sign == 0)
	{
		output = new unsigned __int32[1]{ 0 };
		return 1;
	}

	int dwords_size;
	unsigned __int32 *dwords;
	unsigned __int32 highDWord;

	if (this->_bits == NULL)
	{
		dwords = new unsigned __int32[1]{ (unsigned __int32)this->_sign };
		dwords_size = 1;
		highDWord = (this->_sign < 0) ? UInt32MaxValue : 0;
	}
	else if (_sign == -1)
	{
		dwords = this->_bits;
		dwords_size = this->_bitsSize;
		this->DangerousMakeTwosComplement(dwords, dwords_size);  // mutates dwords
		highDWord = UInt32MaxValue;
	}
	else
	{
		dwords = this->_bits;
		dwords_size = this->_bitsSize;
		highDWord = 0;
	}

	// find highest significant byte
	int msb;
	for (msb = dwords_size - 1; msb > 0; msb--)
	{
		if (dwords[msb] != highDWord) break;
	}
	// ensure high bit is 0 if positive, 1 if negative
	bool needExtraByte = (dwords[msb] & 0x80000000) != (highDWord & 0x80000000);

	int trimmed_size = msb + 1 + (needExtraByte ? 1 : 0);
	output = new unsigned __int32[trimmed_size];

	for (int x = 0, m = msb + 1; x < m; x++)
		output[x] = dwords[x];

	if (needExtraByte) output[trimmed_size - 1] = highDWord;

	// Clean

	if (dwords != this->_bits)
		delete[]dwords;

	return trimmed_size;
}

BigInteger* BigInteger::Or(BigInteger* & bi)
{
	if (bi == NULL || bi->_sign == 0) // IsZero
	{
		return new BigInteger(this);
	}

	if (this->_sign == 0) // IsZero
	{
		return new BigInteger(bi);
	}

	unsigned __int32 *x, *y, *z;

	int sizex = this->ToUInt32Array(x);
	int sizey = bi->ToUInt32Array(y);
	int sizez = sizex > sizey ? sizex : sizey;

	z = new unsigned __int32[sizez];

	unsigned __int32 xExtend = (this->_sign < 0) ? UInt32MaxValue : 0;
	unsigned __int32 yExtend = (bi->_sign < 0) ? UInt32MaxValue : 0;

	unsigned __int32 xu, yu;

	for (int i = 0; i < sizez; i++)
	{
		xu = (i < sizex) ? x[i] : xExtend;
		yu = (i < sizey) ? y[i] : yExtend;
		z[i] = xu | yu;
	}

	BigInteger *ret = new BigInteger(z, sizez);

	delete[]x;
	delete[]y;
	delete[]z;

	return ret;
}

BigInteger* BigInteger::Xor(BigInteger* & bi)
{
	if (bi == NULL || bi->_sign == 0) // IsZero
	{
		return new BigInteger(this);
	}

	if (this->_sign == 0) // IsZero
	{
		return new BigInteger(bi);
	}

	unsigned __int32 *x, *y, *z;

	int sizex = this->ToUInt32Array(x);
	int sizey = bi->ToUInt32Array(y);
	int sizez = sizex > sizey ? sizex : sizey;

	z = new unsigned __int32[sizez];

	unsigned __int32 xExtend = (this->_sign < 0) ? UInt32MaxValue : 0;
	unsigned __int32 yExtend = (bi->_sign < 0) ? UInt32MaxValue : 0;

	unsigned __int32 xu, yu;

	for (int i = 0; i < sizez; i++)
	{
		xu = (i < sizex) ? x[i] : xExtend;
		yu = (i < sizey) ? y[i] : yExtend;
		z[i] = xu ^ yu;
	}

	BigInteger *ret = new BigInteger(z, sizez);

	delete[]x;
	delete[]y;
	delete[]z;

	return ret;
}

BigInteger* BigInteger::And(BigInteger* & bi)
{
	if (bi == NULL || bi->_sign == 0 || this->_sign == 0) // IsZero
	{
		return new BigInteger(BigInteger::Zero);
	}

	unsigned __int32 *x, *y, *z;

	int sizex = this->ToUInt32Array(x);
	int sizey = bi->ToUInt32Array(y);
	int sizez = sizex > sizey ? sizex : sizey;

	z = new unsigned __int32[sizez];

	unsigned __int32 xExtend = (this->_sign < 0) ? UInt32MaxValue : 0;
	unsigned __int32 yExtend = (bi->_sign < 0) ? UInt32MaxValue : 0;

	unsigned __int32 xu, yu;

	for (int i = 0; i < sizez; i++)
	{
		xu = (i < sizex) ? x[i] : xExtend;
		yu = (i < sizey) ? y[i] : yExtend;
		z[i] = xu & yu;
	}

	BigInteger *ret = new BigInteger(z, sizez);

	delete[]x;
	delete[]y;
	delete[]z;

	return ret;
}

BigInteger* BigInteger::Add(BigInteger* & bi)
{
	// left.AssertValid();
	// right.AssertValid();

	if (bi->_sign == 0) // IsZero
	{
		return NULL;
	}

	/*
	if (this->_sign == 0) // IsZero
	{
		this->_sign = bi->_sign;
		this->_bitsSize = bi->_bitsSize;

		if (this->_bits != NULL)
		{
			delete[]this->_bits;
			this->_bits = NULL;
		}

		if (bi->_bits != NULL)
		{
			this->_bits = new unsigned __int32[this->_bitsSize];

			for (int x = 0; x < this->_bitsSize; x++)
				this->_bits[x] = bi->_bits[x];
		}
		return NULL;
	}

	int sign1 = +1;
	int sign2 = +1;
	BigIntegerBuilder reg1 = new BigIntegerBuilder(left, ref sign1);
	BigIntegerBuilder reg2 = new BigIntegerBuilder(right, ref sign2);

	if (sign1 == sign2)
	reg1.Add(ref reg2);
	else
	reg1.Sub(ref sign1, ref reg2);

	return reg1.GetInteger(sign1);
	*/

	return NULL;
}

BigInteger* BigInteger::Sub(BigInteger* &bi)
{
	// left.AssertValid();
	// right.AssertValid();

	if (bi->_sign == 0) // IsZero
	{
		return NULL;
	}

	/*
	if (this->_sign == 0) // IsZero
	{
		this->_sign = -bi->_sign;
		this->_bitsSize = bi->_bitsSize;

		if (this->_bits != NULL)
		{
			delete[]this->_bits;
			this->_bits = NULL;
		}

		if (bi->_bits != NULL)
		{
			this->_bits = new unsigned __int32[this->_bitsSize];

			for (int x = 0; x < this->_bitsSize; x++)
				this->_bits[x] = bi->_bits[x];
		}
		return NULL;
	}

	int sign1 = +1;
	int sign2 = -1;
	BigIntegerBuilder reg1 = new BigIntegerBuilder(left, ref sign1);
	BigIntegerBuilder reg2 = new BigIntegerBuilder(right, ref sign2);

	if (sign1 == sign2)
	reg1.Add(ref reg2);
	else
	reg1.Sub(ref sign1, ref reg2);

	return reg1.GetInteger(sign1);
	*/
	return NULL;
}

BigInteger* BigInteger::Negate()
{
	return new BigInteger(-this->_sign, this->_bits, this->_bitsSize);
}

BigInteger* BigInteger::Abs()
{
	if (this->CompareTo(BigInteger::Zero) >= 0)
		return new BigInteger(this);

	return new BigInteger(-this->_sign, this->_bits, this->_bitsSize);
}

int BigInteger::CompareTo(BigInteger bi)
{
	// AssertValid();
	// other.AssertValid();

	if ((this->_sign ^ bi._sign) < 0)
	{
		// Different signs, so the comparison is easy.
		return this->_sign < 0 ? -1 : +1;
	}

	// Same signs
	if (this->_bits == NULL)
	{
		if (bi._bits == NULL)
			return this->_sign < bi._sign ? -1 : this->_sign > bi._sign ? +1 : 0;
		return -bi._sign;
	}
	int cuThis, cuOther;
	if (bi._bits == NULL || (cuThis = Length(this->_bits, this->_bitsSize)) > (cuOther = Length(bi._bits, this->_bitsSize)))
		return this->_sign;

	if (cuThis < cuOther)
		return -this->_sign;

	int cuDiff = GetDiffLength(this->_bits, bi._bits, cuThis);
	if (cuDiff == 0)
		return 0;

	return this->_bits[cuDiff - 1] < bi._bits[cuDiff - 1] ? -this->_sign : this->_sign;
}

int BigInteger::CompareTo(BigInteger *bi)
{
	// AssertValid();
	// other.AssertValid();

	if ((this->_sign ^ bi->_sign) < 0)
	{
		// Different signs, so the comparison is easy.
		return this->_sign < 0 ? -1 : +1;
	}

	// Same signs
	if (this->_bits == NULL)
	{
		if (bi->_bits == NULL)
			return this->_sign < bi->_sign ? -1 : this->_sign > bi->_sign ? +1 : 0;
		return -bi->_sign;
	}
	int cuThis, cuOther;
	if (bi->_bits == NULL || (cuThis = Length(this->_bits, this->_bitsSize)) > (cuOther = Length(bi->_bits, this->_bitsSize)))
		return this->_sign;

	if (cuThis < cuOther)
		return -this->_sign;

	int cuDiff = GetDiffLength(this->_bits, bi->_bits, cuThis);
	if (cuDiff == 0)
		return 0;

	return this->_bits[cuDiff - 1] < bi->_bits[cuDiff - 1] ? -this->_sign : this->_sign;
}

bool BigInteger::ToInt32(int &ret)
{
	// value.AssertValid();
	if (this->_bits == NULL)
	{
		ret = this->_sign;  // value packed into int32 sign
		return true;
	}
	else if (Length(this->_bits, this->_bitsSize) > 1)
	{
		// more than 32 bits
		//throw new OverflowException(SR.GetString(SR.Overflow_Int32));
		return false;
	}
	else if (this->_sign > 0)
	{
		ret = (__int32)this->_bits[0];
		return true;
	}
	else
	{
		if (this->_bits[0] > kuMaskHighBit)
		{
			// value > Int32.MinValue
			//throw new OverflowException(SR.GetString(SR.Overflow_Int32));
			return false;
		}
		ret = -(__int32)this->_bits[0];
		return true;
	}
}

int BigInteger::GetSign()
{
	return (this->_sign >> (BigInteger::kcbitUint - 1)) - (-this->_sign >> (BigInteger::kcbitUint - 1));
}

BigInteger::BigInteger(unsigned __int32 value)
{
	if (value <= Int32MaxValue)
	{
		this->_sign = (int)value;
		this->_bits = NULL;
		this->_bitsSize = 0;
	}
	else
	{
		this->_sign = +1;
		this->_bits = new unsigned __int32[1]{ value };
		this->_bitsSize = 1;
	}

	// AssertValid();
}

BigInteger::BigInteger(int sign, unsigned __int32 *rgu, int rguSize)
{
	this->_sign = sign;

	if (rgu == NULL || rguSize <= 0)
	{
		this->_bits = NULL;
		this->_bitsSize = 0;

		// AssertValid();
		return;
	}

	this->_bitsSize = rguSize;
	this->_bits = new unsigned __int32[rguSize];

	for (int x = 0; x < rguSize; x++)
		this->_bits[x] = rgu[x];

	// AssertValid();
}

int BigInteger::ToByteArraySize()
{
	// TODO: Is not the exactly size of ToByteArray

	if (this->_bitsSize == 0 && _sign == 0)
	{
		return 1;
	}

	if (this->_bitsSize == 0)
	{
		return 5;
	}
	else
	{
		return (this->_bitsSize * 4) + 1;
	}
}
int BigInteger::ToByteArray(unsigned char * output, int length)
{
	if (this->_bitsSize == 0 && _sign == 0)
	{
		if (length >= 1)
		{
			output[0] = 0;
			return 1;
		}

		return 0;
	}

	// We could probably make this more efficient by eliminating one of the passes.
	// The current code does one pass for uint array -> byte array conversion,
	// and then another pass to remove unneeded bytes at the top of the array.

	int dwordsSize;
	unsigned __int8 highByte;
	unsigned __int32 *dwords;

	if (this->_bitsSize == 0)
	{
		dwords = new unsigned __int32[1]{ (unsigned __int32)_sign };
		dwordsSize = 1;
		highByte = (__int8)((_sign < 0) ? 0xff : 0x00);
	}
	else if (_sign == -1)
	{
		dwordsSize = this->_bitsSize;

		if (this->_bits != NULL)
		{
			// Clone
			dwords = new unsigned __int32[dwordsSize];
			for (int x = 0; x < dwordsSize; x++)
				dwords[x] = this->_bits[x];

			this->DangerousMakeTwosComplement(dwords, dwordsSize);  // mutates dwords
		}
		else
		{
			dwords = NULL;
		}

		highByte = 0xff;
	}
	else
	{
		dwords = this->_bits;
		dwordsSize = this->_bitsSize;
		highByte = 0x00;
	}

	unsigned __int8 *bytes = new unsigned __int8[4 * dwordsSize]/*()*/;
	int curByte = 0;

	unsigned __int32 dword;
	for (int i = 0; i < dwordsSize; i++)
	{
		dword = dwords[i];
		for (int j = 0; j < 4; j++)
		{
			bytes[curByte++] = (unsigned __int8)(dword & 0xff);
			dword >>= 8;
		}
	}

	// find highest significant byte
	int msb;
	for (msb = (4 * dwordsSize) - 1; msb > 0; msb--)
	{
		if (bytes[msb] != highByte) break;
	}

	// ensure high bit is 0 if positive, 1 if negative
	bool needExtraByte = (bytes[msb] & 0x80) != (highByte & 0x80);

	int l = msb + 1 + (needExtraByte ? 1 : 0);

	if (l <= length)
	{
		memcpy(output, bytes, msb + 1);
		if (needExtraByte) output[l - 1] = highByte;
	}
	else
	{
		l = 0;
	}
	// Free memory

	delete[] bytes;
	if (dwords != this->_bits)
		delete[]dwords;

	return l;
}

BigInteger::~BigInteger()
{
	if (this->_bits == NULL) return;

	delete[]this->_bits;
	this->_bits = NULL;
	this->_bitsSize = 0;
}