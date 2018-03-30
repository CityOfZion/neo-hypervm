#pragma once
#include "IStackItem.h"

class MapStackItem : public IStackItem
{
public:

	// Converters

	bool GetBoolean();
	BigInteger * GetBigInteger();
	bool GetInt32(int &ret);
	IStackItem* Clone();
	int ReadByteArray(unsigned char * output, int sourceIndex, int count);
	int ReadByteArraySize();

	// Map Methods

	int Count();
	void Set(IStackItem* key, IStackItem* value);

	// Constructor

	MapStackItem();

	// Destructor

	~MapStackItem();

	// Serialize

	int Serialize(unsigned char * data, int length);
	int GetSerializedSize();
};