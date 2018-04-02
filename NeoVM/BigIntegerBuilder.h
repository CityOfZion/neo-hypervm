#pragma once

#include "Types.h"

class BigIntegerBuilder
{
private:

	static const int32 kcbitUint = 32;
	const static int32 Int32MaxValue = 0x7FFFFFFF;

	bool disposable;

	// For a single uint, _iuLast is 0.
	int32 _iuLast;

	// Used if _iuLast == 0.
	uint32 _uSmall;

	// Used if _iuLast > 0.
	uint32 *_rgu;

	int32 _rguLength;

public:

	// Methods

	void Mul(BigIntegerBuilder &reg);
	void Div(BigIntegerBuilder &reg);
	void Mod(BigIntegerBuilder &reg);
	void Add(BigIntegerBuilder &reg);
	void Sub(int32 &sign, BigIntegerBuilder &reg);
	void GetInteger(int32 &sign, uint32 * &bits, int32 &bitSize);

	// Constructor

	//BigIntegerBuilder(int32 sign, uint32 * bits, int32 bitSize);
	BigIntegerBuilder(int32 sign, uint32 * bits, int32 bitSize, int32 &outSign);

	// Destructor

	~BigIntegerBuilder();
};