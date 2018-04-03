#pragma once
#include "IStackItem.h"

class InteropStackItem : public IStackItem
{
private:

	int32 PayloadLength;
	byte* Payload;

public:

	// Converters

	bool GetBoolean();
	BigInteger * GetBigInteger();
	bool GetInt32(int32 &ret);
	IStackItem* Clone();
	int32 ReadByteArray(byte * output, int32 sourceIndex, int32 count);
	int32 ReadByteArraySize();
	bool Equals(IStackItem * it);

	// Constructor

	InteropStackItem(byte* data, int32 length);

	// Destructor

	~InteropStackItem();

	// Serialize

	int32 Serialize(byte * data, int32 length);
	int32 GetSerializedSize();
};