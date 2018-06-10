#pragma once
#include "IStackItem.h"
#include "BigInteger.h"

class IntegerStackItem : public IStackItem
{
private:

	BigInteger Value;

public:

	// Converters

	bool GetBoolean();
	BigInteger* GetBigInteger();
	bool GetInt32(int32 &ret);
	int32 ReadByteArray(byte* output, int32 sourceIndex, int32 count);
	int32 ReadByteArraySize();
	bool Equals(IStackItem* it);

	// Constructor

	IntegerStackItem(int32 value);
	IntegerStackItem(BigInteger* &value);
	IntegerStackItem(byte* data, int32 length);

	// Destructor

	~IntegerStackItem();

	// Serialize

	int32 Serialize(byte* data, int32 length);
	int32 GetSerializedSize();
};