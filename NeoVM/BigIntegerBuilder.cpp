#include "BigIntegerBuilder.h"

BigIntegerBuilder::BigIntegerBuilder(int32 sign, uint32 * bits, int32 bitSize, int32 &outSign)
{
	this->_fWritable = false;
	this->_rgu = bits;
	this->_rguLength = bitSize;

	int32 n = sign;
	int32 mask = n >> (kcbitUint - 1);
	outSign = (outSign ^ mask) - mask;

	if (this->_rgu == NULL)
	{
		this->_iuLast = 0;
		this->_uSmall = (uint32)((n ^ mask) - mask);
	}
	else
	{
		this->_iuLast = this->_rguLength - 1;
		this->_uSmall = this->_rgu[0];
		while (this->_iuLast > 0 && this->_rgu[this->_iuLast] == 0)
			--this->_iuLast;
	}

	// AssertValid(true);
}

void BigIntegerBuilder::GetInteger(int32 &sign, uint32 * &bits, int32 &bitSize)
{
	// Contract.Requires(sign == +1 || sign == -1);
	// AssertValid(true);
	// uint[] bits;
	// GetIntegerParts(sign, out sign, out bits);
	// return new BigInteger(sign, bits);
	// Contract.Requires(signSrc == +1 || signSrc == -1);
	// AssertValid(true);

	if (this->_iuLast == 0)
	{
		if (this->_uSmall <= Int32MaxValue)
		{
			sign = sign * (int)this->_uSmall;
			bits = NULL;
			bitSize = 0;
			return;
		}

		if (this->_rgu == NULL)
		{
			this->_rgu = new uint32[1]{ this->_uSmall };
			this->_rguLength = 1;
		}
		else if (this->_fWritable)
		{
			this->_rgu[0] = this->_uSmall;
		}
		else if (this->_rgu[0] != this->_uSmall)
		{
			this->_rgu = new uint32[1]{ this->_uSmall };
		}
	}

	// The sign is +/- 1.
	// sign = signSrc;

	int32 cuExtra = this->_rguLength - this->_iuLast - 1;
	// Contract.Assert(cuExtra >= 0);

	if (cuExtra <= 1)
	{
		if (cuExtra == 0 || this->_rgu[this->_iuLast + 1] == 0)
		{
			this->_fWritable = false;
			bits = this->_rgu;
			bitSize = this->_rguLength;
			return;
		}
		if (this->_fWritable)
		{
			this->_rgu[this->_iuLast + 1] = 0;
			this->_fWritable = false;
			bits = this->_rgu;
			bitSize = this->_rguLength;
			return;
		}
		// The buffer isn't writable, but has an extra uint that is non-zero,
		// so we have to allocate a new buffer.
	}

	// Keep the bigger buffer (if it is writable), but create a smaller one for the BigInteger.

	bits = this->_rgu;
	bitSize = this->_rguLength;
	this->_fWritable = false;
	// Array.Resize(ref bits, _iuLast + 1);

	if (!this->_fWritable)
		this->_rgu = bits;
}

void BigIntegerBuilder::Div(BigIntegerBuilder &reg)
{
	// AssertValid(true);
	// regDen.AssertValid(true);

	/*
	  if (regDen._iuLast == 0) {
		DivMod(regDen._uSmall);
		return;
	  }
	  if (_iuLast == 0) {
		_uSmall = 0;
		return;
	  }

	  BigIntegerBuilder regTmp = new BigIntegerBuilder();
	  ModDivCore(ref this, ref regDen, true, ref regTmp);
	  NumericsHelpers.Swap(ref this, ref regTmp);
	*/
}

void BigIntegerBuilder::Mod(BigIntegerBuilder &reg)
{
	// AssertValid(true);
	// regDen.AssertValid(true);

	/*
	if (regDen._iuLast == 0)
	{
		Set(Mod(ref this, regDen._uSmall));
		return;
	}
	if (_iuLast == 0)
		return;

	BigIntegerBuilder regTmp = new BigIntegerBuilder();
	ModDivCore(ref this, ref regDen, false, ref regTmp);
	*/
}

void BigIntegerBuilder::Mul(BigIntegerBuilder &regMul)
{
	// AssertValid(true);
	// regMul.AssertValid(true);

	/*
	  if (regMul._iuLast == 0)
		Mul(regMul._uSmall);
	  else if (_iuLast == 0) {
		uint32 u = _uSmall;
		if (u == 1)
		  this = new BigIntegerBuilder(ref regMul);
		else if (u != 0) {
		  Load(ref regMul, 1);
		  Mul(u);
		}
	  }
	  else {
		int32 cuBase = _iuLast + 1;
		SetSizeKeep(cuBase + regMul._iuLast, 1);

		for (int32 iu = cuBase; --iu >= 0; ) {
		  uint32 uMul = _rgu[iu];
		  _rgu[iu] = 0;
		  uint32 uCarry = 0;
		  for (int32 iuSrc = 0; iuSrc <= regMul._iuLast; iuSrc++)
			uCarry = AddMulCarry(ref _rgu[iu + iuSrc], regMul._rgu[iuSrc], uMul, uCarry);
		  if (uCarry != 0) {
			for (int32 iuDst = iu + regMul._iuLast + 1; uCarry != 0 && iuDst <= _iuLast; iuDst++)
			  uCarry = AddCarry(ref _rgu[iuDst], 0, uCarry);
			if (uCarry != 0) {
			  SetSizeKeep(_iuLast + 2, 0);
			  _rgu[_iuLast] = uCarry;
			}
		  }
		}
		// AssertValid(true);
	  }
	*/
}

// Apply a single borrow starting at iu. This does NOT trim the result.
void BigIntegerBuilder::ApplyBorrow(int32 iuMin)
{
	// Contract.Requires(0 < iuMin);
	// Contract.Assert(_fWritable && _iuLast > 0);
	// Contract.Assert(iuMin <= _iuLast);

	for (int32 iu = iuMin; iu <= this->_iuLast; iu++)
	{
		uint32 u = this->_rgu[iu]--;
		if (u > 0)
			return;
	}
	// Borrowed off the end!
	// Contract.Assert(false, "Invalid call to ApplyBorrow");
}
// Makes sure the buffer is writable and can support cu uints.
// Preserves the contents of the buffer up to min(_iuLast + 1, cu).
// Changes the size if cu <= _iuLast and the buffer needs to be allocated.
void BigIntegerBuilder::EnsureWritable(int32 cu, int32 cuExtra)
{
	// Contract.Requires(cu > 1 && cuExtra >= 0);
	// AssertValid(false);

	if (this->_fWritable && this->_rguLength >= cu)
		return;

	uint32 *rgu = new uint32[cu + cuExtra];

	if (this->_iuLast > 0)
	{
		if (this->_iuLast >= cu)
			this->_iuLast = cu - 1;

		// Array.Copy(_rgu, rgu, _iuLast + 1);
		for (int x = this->_iuLast; x >= 0; x--)
			rgu[x] = this->_rgu[x];
	}

	this->_rguLength = cu + cuExtra;
	this->_rgu = rgu;
	this->_fWritable = true;

	// AssertValid(false);
}

// Makes sure the buffer is writable and can support _iuLast + 1 uints.
// Preserves the contents of the buffer.
void BigIntegerBuilder::EnsureWritable(int32 cuExtra)
{
	// Contract.Requires(cuExtra >= 0);
	// AssertValid(false);
	// Contract.Assert(_iuLast > 0);

	if (this->_fWritable)
	{
		// TODO: Check size??
		return;
	}

	uint32 *rgu = new uint32[_iuLast + 1 + cuExtra];

	for (int x = _iuLast + cuExtra; x >= 0; x--)
		rgu[x] = this->_rgu[x];

	this->_rgu = rgu;
	this->_rguLength = _iuLast + 1 + cuExtra;
	this->_fWritable = true;

	// AssertValid(false);
}

// Sets the size to cu and makes sure the buffer is writable (if cu > 1),
// but makes no guarantees about the contents of the buffer.
void BigIntegerBuilder::SetSizeLazy(int32 cu)
{
	// Contract.Requires(cu > 0);
	// AssertValid(false);

	if (cu <= 1)
	{
		this->_iuLast = 0;
		return;
	}
	if (!this->_fWritable || this->_rguLength < cu)
	{
		if (this->_fWritable && this->_rgu != NULL)
			delete[](this->_rgu);

		this->_rguLength = cu;
		this->_rgu = new uint32[cu];
		this->_fWritable = true;
	}

	this->_iuLast = cu - 1;
	//AssertValid(false);
}

uint32 BigIntegerBuilder::AddCarry(uint32 &u1, uint32 u2, uint32 uCarry)
{
	uint64 uu = (uint64)u1 + u2 + uCarry;
	u1 = (uint32)uu;
	return (uint32)(uu >> kcbitUint);
}

uint32 BigIntegerBuilder::SubBorrow(uint32 &u1, uint32 u2, uint32 uBorrow)
{
	uint64 uu = (uint64)u1 - u2 - uBorrow;
	u1 = (uint32)uu;
	return (uint32)-(int)(uu >> kcbitUint);
}

// Apply a single carry starting at iu, extending the register
// if needed.
void BigIntegerBuilder::ApplyCarry(int32 iu)
{
	// Contract.Requires(0 <= iu);
	// Contract.Assert(_fWritable && _iuLast > 0);
	// Contract.Assert(iu <= _iuLast + 1);

	for (; ; iu++)
	{
		if (iu > this->_iuLast)
		{
			if (this->_iuLast + 1 == this->_rguLength)
			{
				//Array.Resize(ref _rgu, _iuLast + 2);
				uint32 * nrgu = new uint32[_iuLast + 2];
				for (int x = _iuLast + 1; x >= 0; x--)
					nrgu[x] = this->_rgu[x];

				// TODO: Check this!
				this->_rgu = nrgu;
			}
			this->_rgu[++this->_iuLast] = 1;
			break;
		}
		if (++this->_rgu[iu] > 0)
			break;
	}
}

// Loads the value of reg into this register. If we need to allocate memory
// to perform the load, allocate cuExtra elements.
void BigIntegerBuilder::Load(BigIntegerBuilder &reg, int cuExtra)
{
	// Contract.Requires(cuExtra >= 0);
	// AssertValid(false);
	// reg.AssertValid(true);

	if (reg._iuLast == 0)
	{
		this->_uSmall = reg._uSmall;
		this->_iuLast = 0;
	}
	else
	{
		if (!this->_fWritable || this->_rguLength <= reg._iuLast)
		{
			this->_rgu = new uint32[reg._iuLast + 1 + cuExtra];
			this->_rguLength = reg._iuLast + 1 + cuExtra;
			this->_fWritable = true;
		}

		this->_iuLast = reg._iuLast;
		//Array.Copy(reg._rgu, _rgu, _iuLast + 1);
		for (int x = _iuLast; x >= 0; x--)
			this->_rgu[x] = reg._rgu[x];
	}

	// AssertValid(true);
}

void BigIntegerBuilder::Add(uint32 u)
{
	// AssertValid(true);

	if (this->_iuLast == 0)
	{
		if ((this->_uSmall += u) >= u)
			return;

		SetSizeLazy(2);
		this->_rgu[0] = this->_uSmall;
		this->_rgu[1] = 1;
		return;
	}

	if (u == 0)
		return;

	uint32 uNew = this->_rgu[0] + u;
	if (uNew < u)
	{
		// Have carry.
		EnsureWritable(1);
		ApplyCarry(1);
	}
	else if (!this->_fWritable)
	{
		EnsureWritable(0);
	}

	this->_rgu[0] = uNew;

	// AssertValid(true);
}

void BigIntegerBuilder::Add(BigIntegerBuilder &reg)
{
	// AssertValid(true);
	// reg.AssertValid(true);

	if (reg._iuLast == 0)
	{
		Add(reg._uSmall);
		return;
	}
	if (this->_iuLast == 0)
	{
		uint32 u = this->_uSmall;
		if (u == 0)
		{
			//this = new BigIntegerBuilder(ref reg);
			Load(reg, 0);
		}
		else
		{
			Load(reg, 1);
			Add(u);
		}
		return;
	}

	EnsureWritable((this->_iuLast > reg._iuLast ? this->_iuLast : reg._iuLast) + 1, 1);

	int32 cuAdd = reg._iuLast + 1;
	if (this->_iuLast < reg._iuLast)
	{
		cuAdd = this->_iuLast + 1;

		//Array.Copy(reg._rgu, _iuLast + 1, _rgu, _iuLast + 1, reg._iuLast - _iuLast);
		for (int x = 0; x < reg._iuLast - this->_iuLast; x++)
			this->_rgu[x + this->_iuLast + 1] = reg._rgu[x + this->_iuLast + 1];

		// Contract.Assert(_iuLast > 0);
		this->_iuLast = reg._iuLast;
	}

	// Add, tracking carry.
	uint32 uCarry = 0;
	for (int32 iu = 0; iu < cuAdd; iu++)
	{
		uCarry = AddCarry(this->_rgu[iu], reg._rgu[iu], uCarry);
		// Contract.Assert(uCarry <= 1);
	}

	// Deal with extra carry.
	if (uCarry != 0)
		ApplyCarry(cuAdd);

	// AssertValid(true);
}

void BigIntegerBuilder::Trim()
{
	// AssertValid(false);
	if (this->_iuLast > 0 && this->_rgu[this->_iuLast] == 0)
	{
		this->_uSmall = this->_rgu[0];
		while (--this->_iuLast > 0 && this->_rgu[this->_iuLast] == 0);
	}
	// AssertValid(true);
}

int32 BigIntegerBuilder::GetDiffLength(uint32 *rgu1, uint32 * rgu2, int32 cu)
{
	for (int32 iv = cu; --iv >= 0; )
	{
		if (rgu1[iv] != rgu2[iv])
			return iv + 1;
	}
	return 0;
}

uint32 BigIntegerBuilder::SubRevBorrow(uint32 &u1, uint32 u2, uint32 uBorrow)
{
	uint64 uu = (uint64)u2 - u1 - uBorrow;
	u1 = (uint32)uu;
	return (uint32)-(int)(uu >> kcbitUint);
}

// Subtract this register from the given one and put the result in this one.
// Asserts that reg is larger in the most significant uint.
void BigIntegerBuilder::SubRev(BigIntegerBuilder * reg)
{
	// Contract.Assert(0 < _iuLast && _iuLast <= reg._iuLast);
	// Contract.Assert(_iuLast < reg._iuLast || _rgu[_iuLast] < reg._rgu[_iuLast]);

	EnsureWritable(reg->_iuLast + 1, 0);

	int cuSub = this->_iuLast + 1;
	if (this->_iuLast < reg->_iuLast)
	{
		// Array.Copy(reg->_rgu, _iuLast + 1, _rgu, _iuLast + 1, reg->_iuLast - _iuLast);

		int index = _iuLast + 1;
		for (int x = reg->_iuLast - _iuLast - 1; x >= 0; x--)
			this->_rgu[x + index] = reg->_rgu[x + index];

		// Contract.Assert(_iuLast > 0);
		_iuLast = reg->_iuLast;
	}

	uint32 uBorrow = 0;
	for (int iu = 0; iu < cuSub; iu++)
	{
		uBorrow = SubRevBorrow(this->_rgu[iu], reg->_rgu[iu], uBorrow);
		// Contract.Assert(uBorrow <= 1);
	}
	if (uBorrow != 0)
	{
		// Contract.Assert(uBorrow == 1);
		ApplyBorrow(cuSub);
	}
	Trim();
}

void BigIntegerBuilder::Sub(int32 &sign, uint32 u)
{
	// Contract.Requires(sign == +1 || sign == -1);
	// AssertValid(true);

	if (this->_iuLast == 0)
	{
		if (u <= this->_uSmall)
		{
			this->_uSmall -= u;
		}
		else
		{
			this->_uSmall = u - this->_uSmall;
			sign = -sign;
		}

		// AssertValid(true);
		return;
	}

	if (u == 0)
		return;

	EnsureWritable(0);

	uint32 uTmp = this->_rgu[0];
	this->_rgu[0] = uTmp - u;

	if (uTmp < u)
	{
		ApplyBorrow(1);
		Trim();
	}

	// AssertValid(true);
}

void BigIntegerBuilder::Sub(int32 &sign, BigIntegerBuilder &reg)
{
	// Contract.Requires(sign == +1 || sign == -1);
	// AssertValid(true);
	// reg.AssertValid(true);

	if (reg._iuLast == 0)
	{
		Sub(sign, reg._uSmall);
		return;
	}
	if (this->_iuLast == 0)
	{
		uint32 u = this->_uSmall;
		if (u == 0)
		{
			// this = new BigIntegerBuilder(ref reg);
			Load(reg, 0);
		}
		else
		{
			Load(reg, 0);
			Sub(sign, u);
		}
		sign = -sign;
		return;
	}

	if (this->_iuLast < reg._iuLast)
	{
		SubRev(&reg);
		sign = -sign;
		return;
	}

	int32 cuSub = reg._iuLast + 1;
	if (this->_iuLast == reg._iuLast)
	{
		// Determine which is larger.
		this->_iuLast = GetDiffLength(this->_rgu, reg._rgu, this->_iuLast + 1) - 1;
		if (this->_iuLast < 0)
		{
			this->_iuLast = 0;
			this->_uSmall = 0;
			return;
		}

		uint32 u1 = this->_rgu[this->_iuLast];
		uint32 u2 = reg._rgu[this->_iuLast];
		if (this->_iuLast == 0)
		{
			if (u1 < u2)
			{
				this->_uSmall = u2 - u1;
				sign = -sign;
			}
			else
			{
				this->_uSmall = u1 - u2;
			}

			// AssertValid(true);
			return;
		}

		if (u1 < u2)
		{
			// Contract.Assert(_iuLast > 0);
			reg._iuLast = this->_iuLast;
			SubRev(&reg);
			reg._iuLast = cuSub - 1;
			// Contract.Assert(reg._iuLast > 0);
			sign = -sign;
			return;
		}
		cuSub = this->_iuLast + 1;
	}

	EnsureWritable(0);

	// Subtract, tracking borrow.
	uint32 uBorrow = 0;
	for (int32 iu = 0; iu < cuSub; iu++)
	{
		uBorrow = SubBorrow(this->_rgu[iu], reg._rgu[iu], uBorrow);
		// Contract.Assert(uBorrow <= 1);
	}
	if (uBorrow != 0)
	{
		// Contract.Assert(uBorrow == 1 && cuSub <= _iuLast);
		ApplyBorrow(cuSub);
	}

	Trim();
}

BigIntegerBuilder::~BigIntegerBuilder()
{
	if (this->_rgu == NULL || !this->_fWritable) return;

	delete[](this->_rgu);
	this->_rgu = NULL;
}