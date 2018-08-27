#pragma once

#include "IStackItemCounter.h"
#include "EStackItemType.h"
#include "BigInteger.h"
#include "IClaimable.h"

class IStackItem : public IClaimable
{
protected:

	IStackItemCounter* _counter;

public:

	const EStackItemType Type;

	// Converters

	virtual bool GetBoolean() = 0;
	virtual BigInteger* GetBigInteger() = 0;
	virtual bool GetInt32(int32 &ret) = 0;
	virtual bool Equals(IStackItem* it) = 0;
	virtual int32 ReadByteArraySize() = 0;
	virtual int32 ReadByteArray(byte* output, int32 sourceIndex, int32 count) = 0;

	// Serialize

	virtual int32 Serialize(byte* data, int32 length) = 0;
	virtual int32 GetSerializedSize() = 0;

	// Constructor

	inline IStackItem(IStackItemCounter* counter, EStackItemType type) :
		IClaimable(),
		_counter(counter),
		Type(type)
	{
		this->_counter->Claim();
	}

	// Destructor

	virtual ~IStackItem()
	{
		if (this->_counter->UnClaim())
		{
			// Fail when dispose counter before this, for this reason the pointer is a reference pointer

			delete(this->_counter);
		}
		else
		{
			this->_counter->ItemCounterDec();
		}
	};
};