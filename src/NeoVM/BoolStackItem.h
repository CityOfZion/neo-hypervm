#pragma once
#include "IStackItem.h"

class BoolStackItem : public IStackItem
{
private:

	bool Value;

public:

	// Converters

	bool GetBoolean();
	BigInteger * GetBigInteger();
	bool GetInt32(int32 &ret);
	int32 ReadByteArray(byte * output, int32 sourceIndex, int32 count);
	int32 ReadByteArraySize();
	bool Equals(IStackItem * it);

	// Constructor

	BoolStackItem(bool value);

	// Destructor

	~BoolStackItem();

	// Serialize

	int32 Serialize(byte * data, int32 length);
	int32 GetSerializedSize();
};