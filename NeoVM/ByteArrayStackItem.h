#pragma once
#include "IStackItem.h"

class ByteArrayStackItem : public IStackItem
{
private:

	int PayloadLength;
	unsigned char* Payload;

public:

	// Converters

	bool GetBoolean();
	BigInteger * GetBigInteger();
	bool GetInt32(int &ret);
	int ReadByteArray(unsigned char * output, int sourceIndex, int count);
	IStackItem* Clone();
	int ReadByteArraySize();

	// Constructor

	ByteArrayStackItem(unsigned char* data, int length, bool copyPointer);

	// Destructor

	~ByteArrayStackItem();

	// Serialize

	int Serialize(unsigned char * data, int length);
	int GetSerializedSize();
};