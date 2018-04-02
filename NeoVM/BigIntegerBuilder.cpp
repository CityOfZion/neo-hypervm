#include "BigIntegerBuilder.h"

/*
BigIntegerBuilder::BigIntegerBuilder(int32 sign, uint32 *bits, int32 bitSize)
{
	this->_rguLength = bitSize;
	this->disposable = true;

	if (bits == 0)
	{
		this->_rgu = 0;
		this->_iuLast = 0;

		// _uSmall = NumericsHelpers.Abs(bn._Sign);
		uint32 mask = (uint32)(sign >> 31);
		this->_uSmall = ((uint32)sign ^ mask) - mask;
	}
	else
	{
		this->_rgu = new uint32[bitSize];
		for (int32 i = 0; i < bitSize; i++) this->_rgu[i] = bits[i];

		this->_iuLast = this->_rguLength - 1;
		this->_uSmall = this->_rgu[0];
		while (this->_iuLast > 0 && this->_rgu[this->_iuLast] == 0)
			--this->_iuLast;
	}

	// AssertValid(true);
}
*/

BigIntegerBuilder::BigIntegerBuilder(int32 sign, uint32 * bits, int32 bitSize, int32 &outSign)
{
	this->_fWritable = false;
	this->_rgu = bits;
	this->_rguLength = bitSize;

	int32 n = sign;
	int32 mask = n >> (kcbitUint - 1);
	sign = (sign ^ mask) - mask;
	
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

void BigIntegerBuilder::Add(BigIntegerBuilder &reg)
{
	// AssertValid(true);
	// reg.AssertValid(true);

	/*
	  if (reg._iuLast == 0)
	  {
		Add(reg._uSmall);
		return;
	  }
	  if (_iuLast == 0)
	  {
		uint32 u = _uSmall;
		if (u == 0)
		  this = new BigIntegerBuilder(ref reg);
		else
		{
		  Load(ref reg, 1);
		  Add(u);
		}
		return;
	  }

	  EnsureWritable(Math.Max(_iuLast, reg._iuLast) + 1, 1);

	  int32 cuAdd = reg._iuLast + 1;
	  if (_iuLast < reg._iuLast)
	  {
		cuAdd = _iuLast + 1;
		Array.Copy(reg._rgu, _iuLast + 1, _rgu, _iuLast + 1, reg._iuLast - _iuLast);
		Contract.Assert(_iuLast > 0);
		_iuLast = reg._iuLast;
	  }

	  // Add, tracking carry.
	  uint32 uCarry = 0;
	  for (int32 iu = 0; iu < cuAdd; iu++)
	  {
		uCarry = AddCarry(ref _rgu[iu], reg._rgu[iu], uCarry);
		Contract.Assert(uCarry <= 1);
	  }

	  // Deal with extra carry.
	  if (uCarry != 0)
		ApplyCarry(cuAdd);
	  */

	  // AssertValid(true);
}

void BigIntegerBuilder::Sub(int32 &sign, BigIntegerBuilder &reg)
{
	// Contract.Requires(sign == +1 || sign == -1);
	// AssertValid(true);
	// reg.AssertValid(true);

	/*
	  if (reg._iuLast == 0)
	  {
		Sub(ref sign, reg._uSmall);
		return;
	  }
	  if (_iuLast == 0)
	  {
		uint32 u = _uSmall;
		if (u == 0)
		  this = new BigIntegerBuilder(ref reg);
		else
		{
		  Load(ref reg);
		  Sub(ref sign, u);
		}
		sign = -sign;
		return;
	  }

	  if (_iuLast < reg._iuLast)
	  {
		SubRev(ref reg);
		sign = -sign;
		return;
	  }

	  int32 cuSub = reg._iuLast + 1;
	  if (_iuLast == reg._iuLast)
	  {
		// Determine which is larger.
		_iuLast = BigInteger.GetDiffLength(_rgu, reg._rgu, _iuLast + 1) - 1;
		if (_iuLast < 0)
		{
		  _iuLast = 0;
		  _uSmall = 0;
		  return;
		}

		uint32 u1 = _rgu[_iuLast];
		uint32 u2 = reg._rgu[_iuLast];
		if (_iuLast == 0)
		{
		  if (u1 < u2) {
			_uSmall = u2 - u1;
			sign = -sign;
		  }
		  else
			_uSmall = u1 - u2;
		  AssertValid(true);
		  return;
		}

		if (u1 < u2)
		{
		  Contract.Assert(_iuLast > 0);
		  reg._iuLast = _iuLast;
		  SubRev(ref reg);
		  reg._iuLast = cuSub - 1;
		  Contract.Assert(reg._iuLast > 0);
		  sign = -sign;
		  return;
		}
		cuSub = _iuLast + 1;
	  }

	  EnsureWritable();

	  // Subtract, tracking borrow.
	  uint32 uBorrow = 0;
	  for (int32 iu = 0; iu < cuSub; iu++)
	  {
		uBorrow = SubBorrow(ref _rgu[iu], reg._rgu[iu], uBorrow);
		Contract.Assert(uBorrow <= 1);
	  }
	  if (uBorrow != 0)
	  {
		Contract.Assert(uBorrow == 1 && cuSub <= _iuLast);
		ApplyBorrow(cuSub);
	  }
	  Trim();
	*/
}

BigIntegerBuilder::~BigIntegerBuilder()
{
	if (this->_rgu == NULL || !this->_fWritable) return;

	delete[](this->_rgu);
	this->_rgu = NULL;
}