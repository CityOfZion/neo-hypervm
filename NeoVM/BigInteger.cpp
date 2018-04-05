#include "BigInteger.h"
#include "BigIntegerBuilder.h"

// Constants

const BigInteger BigInteger::Min = BigInteger(-1, new uint32[1]{ BigInteger::kuMaskHighBit }, 1);
const BigInteger BigInteger::One = BigInteger(1);
const BigInteger BigInteger::Zero = BigInteger(0);
const BigInteger BigInteger::MinusOne = BigInteger(-1);

BigInteger::BigInteger(int32 value)
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
		this->_bits = new uint32[value->_bitsSize];

		for (int32 x = 0; x < value->_bitsSize; x++)
			this->_bits[x] = value->_bits[x];
	}

	// AssertValid();
}

BigInteger::BigInteger(const BigInteger &value)
{
	this->_sign = value._sign;

	if (value._bits == NULL || value._bitsSize <= 0)
	{
		this->_bits = NULL;
		this->_bitsSize = 0;
	}
	else
	{
		this->_bitsSize = value._bitsSize;
		this->_bits = new uint32[value._bitsSize];

		for (int32 x = 0; x < value._bitsSize; x++)
			this->_bits[x] = value._bits[x];
	}

	// AssertValid();
}

BigInteger::BigInteger(uint32* value, int32 valueSize, bool negative)
{
	if (value == NULL)
	{
		this->_sign = 0;
		this->_bitsSize = 0;
		this->_bits = NULL;
		return;
	}

	// Contract.EndContractBlock();

	int32 len;

	// Try to conserve space as much as possible by checking for wasted leading uint[] entries 
	// sometimes the uint[] has leading zeros from bit manipulation operations & and ^
	for (len = valueSize; len > 0 && value[len - 1] == 0; len--);

	if (len == 0)
	{
		this->_sign = 0;
		this->_bitsSize = 0;
		this->_bits = NULL;
	}
	// values like (Int32.MaxValue+1) are stored as "0x80000000" and as such cannot be packed into _sign
	else if (len == 1 && value[0] < kuMaskHighBit)
	{
		this->_sign = (negative ? -(int)value[0] : (int)value[0]);
		this->_bitsSize = 0;
		this->_bits = NULL;
		// Although Int32.MinValue fits in _sign, we represent this case differently for negate
		if (this->_sign == Int32MinValue)
		{
			CopyInternal(Min._sign, Min._bits, Min._bitsSize);
		}
	}
	else
	{
		this->_bitsSize = len;
		this->_sign = negative ? -1 : +1;
		this->_bits = new uint32[len];
		for (int32 x = 0; x < len; x++)
			this->_bits[x] = value[x];
	}

	// AssertValid();
}

BigInteger::BigInteger(uint32* value, int32 size)
{
	if (value == NULL)
	{
		this->_sign = 0;
		this->_bitsSize = 0;
		this->_bits = NULL;
		return;
	}

	int32 dwordCount = size;
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
			this->_bits = new uint32[1]{ value[0] };
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
			this->_bits = new uint32[dwordCount];

			for (int32 x = 0; x < dwordCount; x++)
				this->_bits[x] = value[x];
		}
		// no trimming is possible.  Assign value directly to _bits.  
		else
		{
			this->_sign = +1;
			this->_bitsSize = size;
			this->_bits = new uint32[size];
			for (int32 x = 0; x < size; x++)
				this->_bits[x] = value[x];
		}

		// AssertValid();
		return;
	}

	// finally handle the more complex cases where we must transform the input into sign magnitude
	this->DangerousMakeTwosComplement(value, size); // mutates val
	// pack _bits to remove any wasted space after the twos complement

	int32 len = size;
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
		this->_bits = new uint32[len];

		for (int32 x = 0; x < len; x++)
			this->_bits[x] = value[x];
	}
	// no trimming is possible.  Assign value directly to _bits.  
	else
	{
		this->_sign = -1;
		this->_bitsSize = size;
		this->_bits = new uint32[size];
		for (int32 x = 0; x < size; x++)
			this->_bits[x] = value[x];
	}

	//AssertValid();
}

BigInteger::BigInteger(byte* value, int32 byteCount)
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

		for (int32 i = byteCount - 1; i >= 0; i--)
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

			this->_bits = new uint32[1]{ (uint32)this->_sign };
			this->_sign = +1;
			this->_bitsSize = 1;

			// AssertValid();
			return;
		}
		if (_sign == Int32MinValue)
		{
			this->_sign = -1;
			this->_bits = new uint32[1]{ BigInteger::kuMaskHighBit };
			this->_bitsSize = 1;

			// AssertValid();
			return;
		}

		this->_bits = NULL;
	}
	else
	{
		int32 unalignedBytes = byteCount % 4;
		int32 dwordCount = byteCount / 4 + (unalignedBytes == 0 ? 0 : 1);
		bool isZero = true;
		uint32 *val = new uint32[dwordCount]();

		// Copy all dwords, except but don't do the last one if it's not a full four bytes

		int32 max = dwordCount - (unalignedBytes == 0 ? 0 : 1);
		int32 curDword = 0, curByte = 3, byteInDword;

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

			int32 len = dwordCount;
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
					this->_bits = new uint32[1]{ BigInteger::kuMaskHighBit };
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
				this->_bits = new uint32[len];
				this->_bitsSize = len;

				for (int32 x = 0; x < len; x++)
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
			delete[](val);
	}

	// AssertValid();
}

void BigInteger::CopyInternal(int32 sign, uint32 *bits, int32 bitSize)
{
	this->_sign = sign;
	this->_bitsSize = bitSize;

	if (bitSize > 0)
	{
		this->_bits = new uint32[bitSize];

		for (int32 x = 0; x < bitSize; x++)
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
void BigInteger::DangerousMakeTwosComplement(uint32 *d, int32 dSize)
{
	// first do complement and +1 as long as carry is needed
	int32 i = 0;
	uint32 v = 0;
	for (; i < dSize; i++)
	{
		v = ~d[i] + 1;
		d[i] = v;
		if (v != 0) { i++; break; }
	}
	if (v != 0)
	{
		// now ones complement is sufficient
		for (; i < dSize; i++)
			d[i] = ~d[i];
	}
	else
	{
		//??? this is weird
		//d = resize(d, d.Length + 1);
		//d[d.Length - 1] = 1;
	}
	// return d;
}

BigInteger* BigInteger::Clone()
{
	return new BigInteger(this->_sign, this->_bits, this->_bitsSize);
}

int32 BigInteger::Length(uint32 *rgu, int32 size)
{
	if (rgu[size - 1] != 0)
		return size;

	//Contract.Assert(cu >= 2 && rgu[cu - 2] != 0);
	return size - 1;
}

int32 BigInteger::GetDiffLength(uint32 *rgu1, uint32 * rgu2, int32 cu)
{
	for (int32 iv = cu; --iv >= 0; )
	{
		if (rgu1[iv] != rgu2[iv])
			return iv + 1;
	}
	return 0;
}

int32 BigInteger::ToUInt32Array(uint32 * &output)
{
	if (this->_bits == NULL && this->_sign == 0)
	{
		output = new uint32[1]{ 0 };
		return 1;
	}

	int32 dwords_size;
	uint32 *dwords;
	uint32 highDWord;

	if (this->_bits == NULL)
	{
		dwords = new uint32[1]{ (uint32)this->_sign };
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
	int32 msb;
	for (msb = dwords_size - 1; msb > 0; msb--)
	{
		if (dwords[msb] != highDWord) break;
	}
	// ensure high bit is 0 if positive, 1 if negative
	bool needExtraByte = (dwords[msb] & 0x80000000) != (highDWord & 0x80000000);

	int32 trimmed_size = msb + 1 + (needExtraByte ? 1 : 0);
	output = new uint32[trimmed_size];

	for (int32 x = 0, m = msb + 1; x < m; x++)
		output[x] = dwords[x];

	if (needExtraByte) output[trimmed_size - 1] = highDWord;

	// Clean

	if (dwords != this->_bits)
		delete[]dwords;

	return trimmed_size;
}

BigInteger* BigInteger::Or(BigInteger* bi)
{
	if (bi == NULL || bi->_sign == 0) // IsZero
	{
		return new BigInteger(this);
	}

	if (this->_sign == 0) // IsZero
	{
		return new BigInteger(bi);
	}

	uint32 *x, *y, *z;

	int32 sizex = this->ToUInt32Array(x);
	int32 sizey = bi->ToUInt32Array(y);
	int32 sizez = sizex > sizey ? sizex : sizey;

	z = new uint32[sizez];

	uint32 xExtend = (this->_sign < 0) ? UInt32MaxValue : 0;
	uint32 yExtend = (bi->_sign < 0) ? UInt32MaxValue : 0;

	uint32 xu, yu;

	for (int32 i = 0; i < sizez; i++)
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

BigInteger* BigInteger::Xor(BigInteger* bi)
{
	if (bi == NULL || bi->_sign == 0) // IsZero
	{
		return new BigInteger(this);
	}

	if (this->_sign == 0) // IsZero
	{
		return new BigInteger(bi);
	}

	uint32 *x, *y, *z;

	int32 sizex = this->ToUInt32Array(x);
	int32 sizey = bi->ToUInt32Array(y);
	int32 sizez = sizex > sizey ? sizex : sizey;

	z = new uint32[sizez];

	uint32 xExtend = (this->_sign < 0) ? UInt32MaxValue : 0;
	uint32 yExtend = (bi->_sign < 0) ? UInt32MaxValue : 0;

	uint32 xu, yu;

	for (int32 i = 0; i < sizez; i++)
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

bool BigInteger::GetPartsForBitManipulation(BigInteger *x, uint32 * &xd, int32 &xl)
{
	if (x->_bits == NULL)
	{
		if (x->_sign < 0)
		{
			xd = new uint32[1]{ (uint32)-x->_sign };
		}
		else
		{
			xd = new uint32[1]{ (uint32)x->_sign };
		}

		xl = 1;
	}
	else
	{
		xl = x->_bitsSize;

		if (xl > 0)
		{
			xd = new uint32[xl];
			for (int32 y = 0; y < xl; y++)
				xd[y] = x->_bits[y];
		}
		else
		{
			xd = NULL;
		}
	}

	return x->_sign < 0;
}

BigInteger* BigInteger::Shl(int32 shift)
{
	if (shift == 0) return new BigInteger(this);
	else if (shift == Int32MinValue) return this->Shr(Int32MaxValue)->Shr(1);
	else if (shift < 0) return this->Shr(-shift);

	int32 digitShift = shift / kcbitUint;
	int32 smallShift = shift - (digitShift * kcbitUint);

	int32 xl;
	uint32 *xd;
	bool negx = this->GetPartsForBitManipulation(this, xd, xl);

	int32 zl = xl + digitShift + 1;
	uint32 *zd = new uint32[zl]();

	if (smallShift == 0)
	{
		for (int32 i = 0; i < xl; i++)
			zd[i + digitShift] = xd[i];
	}
	else
	{
		int32 i;
		int32 carryShift = kcbitUint - smallShift;
		uint32 carry = 0;

		for (i = 0; i < xl; i++)
		{
			uint32 rot = xd[i];
			zd[i + digitShift] = rot << smallShift | carry;
			carry = rot >> carryShift;
		}
		zd[i + digitShift] = carry;
	}

	delete[](xd);
	BigInteger * ret = new BigInteger(zd, zl, negx);
	delete[](zd);

	return ret;
}

BigInteger* BigInteger::Shr(int32 shift)
{
	if (shift == 0) return new BigInteger(this);
	else if (shift == Int32MinValue) return this->Shl(Int32MaxValue)->Shl(1);
	else if (shift < 0) return this->Shl(-shift);

	int32 digitShift = shift / kcbitUint;
	int32 smallShift = shift - (digitShift * kcbitUint);

	uint32 *xd;
	int32 xl;
	bool negx = this->GetPartsForBitManipulation(this, xd, xl);

	if (negx)
	{
		if (shift >= (kcbitUint * xl))
		{
			return new BigInteger(BigInteger::MinusOne);
		}

		// This version don't require this copy, because `GetPartsForBitManipulation` always return a copy

		/*
		uint32 *temp = new uint32[xl];
		for (int32 i = 0; i < xl; i++)temp[i] = xd[i];  // make a copy of immutable value._bits
		delete[](xd);
		xd = temp;
		*/

		this->DangerousMakeTwosComplement(xd, xl); // mutates xd
	}

	int32 zl = xl - digitShift;
	if (zl < 0) zl = 0;
	uint32 * zd = new uint32[zl];

	if (smallShift == 0)
	{
		for (int32 i = xl - 1; i >= digitShift; i--)
		{
			zd[i - digitShift] = xd[i];
		}
	}
	else
	{
		int32 carryShift = kcbitUint - smallShift;
		uint32 carry = 0;
		for (int32 i = xl - 1; i >= digitShift; i--)
		{
			uint32 rot = xd[i];
			if (negx && i == xl - 1)
				// sign-extend the first shift for negative ints then let the carry propagate
				zd[i - digitShift] = (rot >> smallShift) | (0xFFFFFFFF << carryShift);
			else
				zd[i - digitShift] = (rot >> smallShift) | carry;
			carry = rot << carryShift;
		}
	}
	if (negx)
	{
		this->DangerousMakeTwosComplement(zd, zl); // mutates zd
	}

	delete[](xd);
	BigInteger *ret = new BigInteger(zd, zl, negx);
	delete[](zd);

	return ret;
}

BigInteger* BigInteger::And(BigInteger* bi)
{
	if (bi == NULL || bi->_sign == 0 || this->_sign == 0) // IsZero
	{
		return new BigInteger(BigInteger::Zero);
	}

	uint32 *x, *y, *z;

	int32 sizex = this->ToUInt32Array(x);
	int32 sizey = bi->ToUInt32Array(y);
	int32 sizez = sizex > sizey ? sizex : sizey;

	z = new uint32[sizez];

	uint32 xExtend = (this->_sign < 0) ? UInt32MaxValue : 0;
	uint32 yExtend = (bi->_sign < 0) ? UInt32MaxValue : 0;

	uint32 xu, yu;

	for (int32 i = 0; i < sizez; i++)
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

BigInteger* BigInteger::Div(BigInteger* bi)
{
	// dividend.AssertValid();
	// divisor.AssertValid();

	if (bi->_sign == 0) // IsZero
	{
		return NULL;
	}

	int32 sign = +1;
	BigIntegerBuilder regNum(this->_sign, this->_bits, this->_bitsSize, sign);
	BigIntegerBuilder regDen(bi->_sign, bi->_bits, bi->_bitsSize, sign);

	regNum.Div(regDen);

	int32 bitSize;
	uint32 *bits;
	regNum.GetInteger(sign, bits, bitSize);

	return new BigInteger(sign, bits, bitSize);
}

BigInteger* BigInteger::Mul(BigInteger* bi)
{
	// left.AssertValid();
	// right.AssertValid();

	int32 sign = +1;
	BigIntegerBuilder reg1(this->_sign, this->_bits, this->_bitsSize, sign);
	BigIntegerBuilder reg2(bi->_sign, bi->_bits, bi->_bitsSize, sign);

	reg1.Mul(reg2);

	int32 bitSize;
	uint32 *bits;
	reg1.GetInteger(sign, bits, bitSize);

	return new BigInteger(sign, bits, bitSize);
}

BigInteger* BigInteger::Mod(BigInteger* bi)
{
	// dividend.AssertValid();
	// divisor.AssertValid();

	if (bi->_sign == 0) // IsZero
	{
		return NULL;
	}

	int32 signNum = +1;
	int32 signDen = +1;
	BigIntegerBuilder regNum(this->_sign, this->_bits, this->_bitsSize, signNum);
	BigIntegerBuilder regDen(bi->_sign, bi->_bits, bi->_bitsSize, signDen);

	regNum.Mod(regDen);

	int32 bitSize;
	uint32 *bits;
	regNum.GetInteger(signNum, bits, bitSize);

	return new BigInteger(signNum, bits, bitSize);
}

BigInteger* BigInteger::Add(BigInteger* bi)
{
	// left.AssertValid();
	// right.AssertValid();

	if (bi->_sign == 0) // IsZero
	{
		return new BigInteger(this);
	}

	if (this->_sign == 0) // IsZero
	{
		return new BigInteger(bi);
	}

	int32 sign1 = +1;
	int32 sign2 = +1;

	BigIntegerBuilder reg1(this->_sign, this->_bits, this->_bitsSize, sign1);
	BigIntegerBuilder reg2(bi->_sign, bi->_bits, bi->_bitsSize, sign2);

	if (sign1 == sign2)
		reg1.Add(reg2);
	else
		reg1.Sub(sign1, reg2);

	int32 bitSize;
	uint32 *bits;
	reg1.GetInteger(sign1, bits, bitSize);

	return new BigInteger(sign1, bits, bitSize);
}

BigInteger* BigInteger::Sub(BigInteger* bi)
{
	// left.AssertValid();
	// right.AssertValid();

	if (bi->_sign == 0) // IsZero
	{
		return new BigInteger(this);
	}

	if (this->_sign == 0) // IsZero
	{
		// negate
		return bi->Negate();
	}

	int32 sign1 = +1;
	int32 sign2 = -1;

	BigIntegerBuilder reg1(this->_sign, this->_bits, this->_bitsSize, sign1);
	BigIntegerBuilder reg2(bi->_sign, bi->_bits, bi->_bitsSize, sign2);

	if (sign1 == sign2)
		reg1.Add(reg2);
	else
		reg1.Sub(sign1, reg2);

	int32 bitSize;
	uint32 *bits;
	reg1.GetInteger(sign1, bits, bitSize);

	return new BigInteger(sign1, bits, bitSize);
}

BigInteger* BigInteger::Negate()
{
	return new BigInteger(-this->_sign, this->_bits, this->_bitsSize);
}

BigInteger* BigInteger::Invert()
{
	BigInteger *add = new BigInteger(BigInteger::One);
	BigInteger* ret = Add(add);
	delete(add);

	// Negate
	BigInteger* realRet = new BigInteger(-ret->_sign, ret->_bits, ret->_bitsSize);
	delete(ret);
	return realRet;
}

BigInteger* BigInteger::Abs()
{
	if (this->CompareTo(BigInteger::Zero) >= 0)
		return new BigInteger(this);

	return new BigInteger(-this->_sign, this->_bits, this->_bitsSize);
}

int32 BigInteger::CompareTo(const BigInteger &bi)
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

	int32 cuThis, cuOther;
	if (bi._bits == NULL || (cuThis = Length(this->_bits, this->_bitsSize)) > (cuOther = Length(bi._bits, bi._bitsSize)))
		return this->_sign;

	if (cuThis < cuOther)
		return -this->_sign;

	int32 cuDiff = GetDiffLength(this->_bits, bi._bits, cuThis);
	if (cuDiff == 0)
		return 0;

	return this->_bits[cuDiff - 1] < bi._bits[cuDiff - 1] ? -this->_sign : this->_sign;
}

int32 BigInteger::CompareTo(BigInteger *bi)
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

	int32 cuThis, cuOther;
	if (bi->_bits == NULL || (cuThis = Length(this->_bits, this->_bitsSize)) > (cuOther = Length(bi->_bits, bi->_bitsSize)))
		return this->_sign;

	if (cuThis < cuOther)
		return -this->_sign;

	int32 cuDiff = GetDiffLength(this->_bits, bi->_bits, cuThis);
	if (cuDiff == 0)
		return 0;

	return this->_bits[cuDiff - 1] < bi->_bits[cuDiff - 1] ? -this->_sign : this->_sign;
}

bool BigInteger::ToInt32(int32 &ret)
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
		ret = (int32)this->_bits[0];
		// checked
		return ret >= 0;
	}
	else
	{
		if (this->_bits[0] > kuMaskHighBit)
		{
			// value > Int32.MinValue
			//throw new OverflowException(SR.GetString(SR.Overflow_Int32));
			return false;
		}
		ret = -(int32)this->_bits[0];
		return true;
	}
}

int32 BigInteger::GetSign()
{
	return (this->_sign >> (BigInteger::kcbitUint - 1)) - (-this->_sign >> (BigInteger::kcbitUint - 1));
}

BigInteger::BigInteger(uint32 value)
{
	if (value <= Int32MaxValue)
	{
		this->_sign = (int32)value;
		this->_bits = NULL;
		this->_bitsSize = 0;
	}
	else
	{
		this->_sign = +1;
		this->_bits = new uint32[1]{ value };
		this->_bitsSize = 1;
	}

	// AssertValid();
}

BigInteger::BigInteger(int32 sign, uint32 *rgu, int32 rguSize)
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
	this->_bits = new uint32[rguSize];

	for (int32 x = 0; x < rguSize; x++)
		this->_bits[x] = rgu[x];

	// AssertValid();
}

int32 BigInteger::ToByteArraySize()
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

int32 BigInteger::ToByteArray(byte * output, int32 length)
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

	int32 dwordsSize;
	byte highByte;
	uint32 *dwords;

	if (this->_bitsSize == 0)
	{
		dwords = new uint32[1]{ (uint32)_sign };
		dwordsSize = 1;
		highByte = (byte)((_sign < 0) ? 0xff : 0x00);
	}
	else if (_sign == -1)
	{
		dwordsSize = this->_bitsSize;

		if (this->_bits != NULL)
		{
			// Clone
			dwords = new uint32[dwordsSize];
			for (int32 x = 0; x < dwordsSize; x++)
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

	byte *bytes = new byte[4 * dwordsSize]/*()*/;
	int32 curByte = 0;

	uint32 dword;
	for (int32 i = 0; i < dwordsSize; i++)
	{
		dword = dwords[i];
		for (int32 j = 0; j < 4; j++)
		{
			bytes[curByte++] = (byte)(dword & 0xff);
			dword >>= 8;
		}
	}

	// find highest significant byte
	int32 msb;
	for (msb = (4 * dwordsSize) - 1; msb > 0; msb--)
	{
		if (bytes[msb] != highByte) break;
	}

	// ensure high bit is 0 if positive, 1 if negative
	bool needExtraByte = (bytes[msb] & 0x80) != (highByte & 0x80);

	int32 l = msb + 1 + (needExtraByte ? 1 : 0);

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