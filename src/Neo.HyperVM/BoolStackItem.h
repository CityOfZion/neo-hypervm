#pragma once
#include "IStackItem.h"

class BoolStackItem : public IStackItem
{
private:

	bool _value;

public:

	// Converters

	inline bool GetBoolean()
	{
		return this->_value;
	}

	inline BigInteger* GetBigInteger()
	{
		return new BigInteger(this->_value ? BigInteger::One : BigInteger::Zero);
	}

	inline bool GetInt32(int32 &ret)
	{
		ret = this->_value ? 1 : 0;
		return true;
	}

	int32 ReadByteArray(byte* output, int32 sourceIndex, int32 count);

	inline int32 ReadByteArraySize()
	{
		return this->_value ? 1 : 0;
	}

	bool Equals(IStackItem* it);

	// Constructor & Destructor

	inline BoolStackItem(IStackItemCounter* counter, bool value) :
		IStackItem(counter, EStackItemType::Bool), 
		_value(value) 
	{

	}

	inline ~BoolStackItem() { }

	// Serialize

	inline int32 Serialize(byte* data, int32 length)
	{
		if (length <= 0) return 0;

		data[0] = _value ? 0x01 : 0x00;
		return 1;
	}

	inline int32 GetSerializedSize()
	{
		return _value ? 1 : 0;
	}
};