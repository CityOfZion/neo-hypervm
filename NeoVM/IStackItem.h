#pragma once
#include "EStackItemType.h"
#include "BigInteger.h"

class IStackItem
{
public:

	int Claims;
	EStackItemType Type;

	// Converters

	virtual bool GetBoolean() = 0;
	virtual BigInteger * GetBigInteger() = 0;
	virtual bool GetInt32(int &ret) = 0;
	virtual int ReadByteArray(unsigned char * output, int sourceIndex, int count) = 0;
	virtual int ReadByteArraySize() = 0;
	virtual IStackItem* Clone() = 0;

	// Constructor

	IStackItem(EStackItemType type);

	// Destructor

	virtual ~IStackItem() { };
	static void Free(IStackItem* &item);

	// Serialize

	virtual int Serialize(unsigned char * data, int length) = 0;
	virtual int GetSerializedSize() = 0;
};