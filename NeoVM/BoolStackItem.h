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
	bool GetInt32(int &ret);
	IStackItem* Clone();
	int ReadByteArray(unsigned char * output, int sourceIndex, int count);
	int ReadByteArraySize();

	// Constructor

	BoolStackItem(bool value);

	// Destructor

	~BoolStackItem();

	// Serialize

	int Serialize(unsigned char * data, int length);
	int GetSerializedSize();
};