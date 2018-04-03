#pragma once

#include "Types.h"

class BigIntegerBuilder
{
private:

	static const int32 kcbitUint = 32;
	const static int32 Int32MaxValue = 0x7FFFFFFF;
	const static int32 UInt32MaxValue = 0xFFFFFFFF;

	bool _fWritable;

	// For a single uint, _iuLast is 0.
	int32 _iuLast;

	// Used if _iuLast == 0.
	uint32 _uSmall;

	// Used if _iuLast > 0.
	uint32 *_rgu;

	int32 _rguLength;

	static int32 CbitHighZero(uint32 u);
	static int32 CbitHighZero(uint64 uu);
	static uint32 AddCarry(uint32 &u1, uint32 u2, uint32 uCarry);
	static uint32 MulCarry(uint32 &u1, uint32 u2, uint32 uCarry);
	static uint32 Mod(BigIntegerBuilder *regNum, uint32 uDen);
	static void ModDivCore(BigIntegerBuilder *regNum, BigIntegerBuilder &regDen, bool fQuo, BigIntegerBuilder &regQuo);
	static uint32 AddMulCarry(uint32 &uAdd, uint32 uMul1, uint32 uMul2, uint32 uCarry);
	static int32 GetDiffLength(uint32 *rgu1, uint32 * rgu2, int32 cu);
	static uint32 SubBorrow(uint32 &u1, uint32 u2, uint32 uBorrow);
	static uint32 SubRevBorrow(uint32 &u1, uint32 u2, uint32 uBorrow);
	
	void ApplyBorrow(int32 iuMin);
	void Add(uint32 u);
	void Mul(uint32 u);
	void Sub(int32 &sign, uint32 u);
	void SubRev(BigIntegerBuilder * reg);
	void Set(uint32 uu);
	void Set(uint64 uu);
	uint32 DivMod(uint32 uDen);

	void Trim();
	void SetSizeLazy(int32 cu);
	void SetSizeKeep(int32 cu, int32 cuExtra);
	void EnsureWritable(int32 cuExtra);
	void EnsureWritable(int32 cu, int32 cuExtra);
	void ApplyCarry(int32 iu);
	void Load(BigIntegerBuilder &reg, int cuExtra);

	BigIntegerBuilder();

public:

	// Methods

	void Mul(BigIntegerBuilder &reg);
	void Div(BigIntegerBuilder &regDen);
	void Mod(BigIntegerBuilder &regDen);
	void Add(BigIntegerBuilder &reg);
	void Sub(int32 &sign, BigIntegerBuilder &reg);
	void GetInteger(int32 &sign, uint32 * &bits, int32 &bitSize);

	// Constructor

	BigIntegerBuilder(int32 sign, uint32 * bits, int32 bitSize, int32 &outSign);

	// Destructor

	~BigIntegerBuilder();
};