#pragma once
#include "IStackItem.h"

class InteropStackItem : public IStackItem
{
private:

	int PayloadLength;
	unsigned char* Payload;

public:

	// Converters

	bool GetBoolean();
	BigInteger * GetBigInteger();
	bool GetInt32(int &ret);
	IStackItem* Clone();
	int ReadByteArray(unsigned char * output, int sourceIndex, int count);
	int ReadByteArraySize();

	// Constructor

	InteropStackItem(unsigned char* data, int length);

	// Destructor

	~InteropStackItem();

	// Serialize

	int Serialize(unsigned char * data, int length);
	int GetSerializedSize();
};