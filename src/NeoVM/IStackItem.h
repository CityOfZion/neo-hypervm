#pragma once

#include "EStackItemType.h"
#include "BigInteger.h"

class IStackItem
{
private:

	int32 Claims;

public:

	EStackItemType Type;

	// Converters

	virtual bool GetBoolean() = 0;
	virtual BigInteger * GetBigInteger() = 0;
	virtual bool GetInt32(int32 &ret) = 0;
	virtual bool Equals(IStackItem * it) = 0;
	virtual int32 ReadByteArraySize() = 0;
	virtual int32 ReadByteArray(byte * output, int32 sourceIndex, int32 count) = 0;

	// Constructor

	IStackItem(EStackItemType type);

	// Destructor

	virtual ~IStackItem() { };

	// Claims

	void Claim();
	void UnClaim();

	static void Free(IStackItem* &item);
	static void UnclaimAndFree(IStackItem* &item);
	
	// Serialize

	virtual int32 Serialize(byte * data, int32 length) = 0;
	virtual int32 GetSerializedSize() = 0;
};