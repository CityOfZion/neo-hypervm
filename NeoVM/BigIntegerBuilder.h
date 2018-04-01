#pragma once
class BigIntegerBuilder
{
private:

	static const int kcbitUint = 32;

	// For a single uint, _iuLast is 0.
	int _iuLast;

	// Used if _iuLast == 0.
	unsigned __int32 _uSmall;

	// Used if _iuLast > 0.
	unsigned __int32 *_rgu;

	int _rguLength;

public:

	// Methods

	void Mul(BigIntegerBuilder &reg);
	void Div(BigIntegerBuilder &reg);
	void Mod(BigIntegerBuilder &reg);
	void Add(BigIntegerBuilder &reg);
	void Sub(int &sign, BigIntegerBuilder &reg);
	void GetInteger(int &sign, unsigned __int32 * &bits, int &bitSize);

	// Constructor

	//BigIntegerBuilder(__int32 sign, unsigned __int32 * bits, int bitSize);
	BigIntegerBuilder(__int32 sign, unsigned __int32 * bits, int bitSize, __int32 &outSign);

	// Destructor

	~BigIntegerBuilder();
};