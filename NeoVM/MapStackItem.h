#pragma once
#include "IStackItem.h"

class MapStackItem : public IStackItem
{
public:

	// Converters

	bool GetBoolean();
	BigInteger * GetBigInteger();
	bool GetInt32(int32 &ret);
	IStackItem* Clone();
	int32 ReadByteArray(byte * output, int32 sourceIndex, int32 count);
	int32 ReadByteArraySize();

	// Map Methods

	int32 Count();
	void Set(IStackItem* key, IStackItem* value);

	// Constructor

	MapStackItem();

	// Destructor

	~MapStackItem();

	// Serialize

	int32 Serialize(byte * data, int32 length);
	int32 GetSerializedSize();
};