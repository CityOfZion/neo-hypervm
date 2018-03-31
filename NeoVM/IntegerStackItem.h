#pragma once
#include "IStackItem.h"
#include "BigInteger.h"

class IntegerStackItem : public IStackItem
{
private:

	BigInteger * Value;

public:

	// Converters

	bool GetBoolean();
	BigInteger * GetBigInteger();
	bool GetInt32(int &ret);
	int ReadByteArray(unsigned char * output, int sourceIndex, int count);
	IStackItem* Clone();
	int ReadByteArraySize();

	// Constructor

	IntegerStackItem(int value);
	IntegerStackItem(BigInteger *value, bool copyPointer);
	IntegerStackItem(unsigned char* data, int length);

	// Destructor

	~IntegerStackItem();

	// Serialize

	int Serialize(unsigned char * data, int length);
	int GetSerializedSize();
};