#include "BigIntegerBuilder.h"

/*
BigIntegerBuilder::BigIntegerBuilder(__int32 sign, unsigned __int32 *bits, int bitSize)
{
	this->_rguLength = bitSize;

	if (bits == 0)
	{
		this->_rgu = 0;
		this->_iuLast = 0;

		// _uSmall = NumericsHelpers.Abs(bn._Sign);
		unsigned __int32 mask = (unsigned __int32)(sign >> 31);
		this->_uSmall = ((unsigned __int32)sign ^ mask) - mask;
	}
	else
	{
		this->_rgu = new unsigned[bitSize];
		for (int i = 0; i < bitSize; i++) this->_rgu[i] = bits[i];

		this->_iuLast = this->_rguLength - 1;
		this->_uSmall = this->_rgu[0];
		while (this->_iuLast > 0 && this->_rgu[this->_iuLast] == 0)
			--this->_iuLast;
	}

	// AssertValid(true);
}
*/

BigIntegerBuilder::BigIntegerBuilder(__int32 sign, unsigned __int32 * bits, int bitSize, __int32 &outSign)
{
	this->_rguLength = bitSize;

	int n = sign;
	int mask = n >> (kcbitUint - 1);
	outSign = (outSign ^ mask) - mask;

	if (bits == 0)
	{
		this->_rgu = 0;
		this->_iuLast = 0;
		this->_uSmall = (unsigned __int32)((n ^ mask) - mask);
	}
	else
	{
		this->_rgu = new unsigned[bitSize];
		for (int i = 0; i < bitSize; i++) this->_rgu[i] = bits[i];

		this->_iuLast = this->_rguLength - 1;
		this->_uSmall = this->_rgu[0];
		while (this->_iuLast > 0 && this->_rgu[this->_iuLast] == 0)
			--this->_iuLast;
	}

	// AssertValid(true);
}

void BigIntegerBuilder::GetInteger(int &sign, unsigned __int32 * &bits, int &bitSize)
{
	// Contract.Requires(sign == +1 || sign == -1);
	// AssertValid(true);

	// uint[] bits;
	// GetIntegerParts(sign, out sign, out bits);
	// return new BigInteger(sign, bits);

	// Contract.Requires(signSrc == +1 || signSrc == -1);
	// AssertValid(true);

	/*
	if (_iuLast == 0) 
	{
		if (_uSmall <= int.MaxValue) 
		{
			sign = signSrc * (int)_uSmall;
			bits = null;
			return;
		}
		if (_rgu == null)
			_rgu = new uint[1]{ _uSmall };
		else if (_fWritable)
			_rgu[0] = _uSmall;
		else if (_rgu[0] != _uSmall)
			_rgu = new uint[1]{ _uSmall };
	}

	// The sign is +/- 1.
	sign = signSrc;

	int cuExtra = _rgu.Length - _iuLast - 1;
	// Contract.Assert(cuExtra >= 0);
	if (cuExtra <= 1) 
	{
		if (cuExtra == 0 || _rgu[_iuLast + 1] == 0) 
		{
			_fWritable = false;
			bits = _rgu;
			return;
		}
		if (_fWritable) 
		{
			_rgu[_iuLast + 1] = 0;
			_fWritable = false;
			bits = _rgu;
			return;
		}
		// The buffer isn't writable, but has an extra uint that is non-zero,
		// so we have to allocate a new buffer.
	}

	// Keep the bigger buffer (if it is writable), but create a smaller one for the BigInteger.
	bits = _rgu;
	Array.Resize(ref bits, _iuLast + 1);
	if (!_fWritable)
		_rgu = bits;
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
		uint u = _uSmall;
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

	  int cuAdd = reg._iuLast + 1;
	  if (_iuLast < reg._iuLast)
	  {
		cuAdd = _iuLast + 1;
		Array.Copy(reg._rgu, _iuLast + 1, _rgu, _iuLast + 1, reg._iuLast - _iuLast);
		Contract.Assert(_iuLast > 0);
		_iuLast = reg._iuLast;
	  }

	  // Add, tracking carry.
	  uint uCarry = 0;
	  for (int iu = 0; iu < cuAdd; iu++)
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

void BigIntegerBuilder::Sub(int &sign, BigIntegerBuilder &reg)
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
		uint u = _uSmall;
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

	  int cuSub = reg._iuLast + 1;
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

		uint u1 = _rgu[_iuLast];
		uint u2 = reg._rgu[_iuLast];
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
	  uint uBorrow = 0;
	  for (int iu = 0; iu < cuSub; iu++)
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
	if (this->_rgu == 0) return;

	delete[](_rgu);
	_rgu = 0;
}