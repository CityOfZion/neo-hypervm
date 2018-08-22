#pragma once
#include "IStackItem.h"
#include "BigInteger.h"

class IntegerStackItem : public IStackItem
{
private:

	BigInteger _value;

public:

	// Converters

	inline bool GetBoolean()
	{
		return this->_value.CompareTo(BigInteger::Zero) != 0;
	}

	inline BigInteger* GetBigInteger()
	{
		return this->_value.Clone();
	}

	inline bool GetInt32(int32 &ret)
	{
		return this->_value.ToInt32(ret);
	}

	inline int32 ReadByteArray(byte* output, int32 sourceIndex, int32 count)
	{
		if (sourceIndex != 0)
		{
			return -1;
		}

		return this->_value.ToByteArray(output, count);
	}

	inline int32 ReadByteArraySize()
	{
		return this->_value.ToByteArraySize();
	}

	bool Equals(IStackItem* it);

	// Constructor

	inline IntegerStackItem(IStackItemCounter* counter, byte* data, int32 size) :
		IStackItem(counter, EStackItemType::Integer),
		_value(data, size) { }

	inline IntegerStackItem(IStackItemCounter* counter, int32 value) :
		IStackItem(counter, EStackItemType::Integer),
		_value(value) { }

	inline IntegerStackItem(IStackItemCounter* counter, BigInteger* &value) :
		IStackItem(counter, EStackItemType::Integer),
		_value(value)
	{
		// TODO: I don't like this, should be used the same pointer

		delete(value);
		value = NULL;
	}

	// Destructor

	inline ~IntegerStackItem()
	{
		/*if (this->Value == NULL) return;

		delete(this->Value);
		this->Value = NULL;*/
	}

	// Serialize

	inline int32 Serialize(byte* data, int32 length)
	{
		return this->_value.ToByteArray(data, length);
	}

	inline int32 GetSerializedSize()
	{
		return this->_value.ToByteArraySize();
	}
};