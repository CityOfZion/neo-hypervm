#pragma once

#include "EStackItemType.h"
#include "BigInteger.h"
#include "IClaimable.h"

class IStackItem : public IClaimable
{
public:

	const EStackItemType Type;

	// Converters

	virtual bool GetBoolean() = 0;
	virtual BigInteger* GetBigInteger() = 0;
	virtual bool GetInt32(int32 &ret) = 0;
	virtual bool Equals(IStackItem* it) = 0;
	virtual int32 ReadByteArraySize() = 0;
	virtual int32 ReadByteArray(byte* output, int32 sourceIndex, int32 count) = 0;

	// Constructor

	inline IStackItem(EStackItemType type) : IClaimable(), Type(type) { }

	// Destructor

	virtual ~IStackItem() { };

	// Claims

	static void Free(IStackItem* &item);
	static void Free(IStackItem* &itemA, IStackItem* &itemB);
	static void Free(IStackItem* &itemA, IStackItem* &itemB, IStackItem* &itemC);
	static void UnclaimAndFree(IStackItem* &item);
	
	// Serialize

	virtual int32 Serialize(byte* data, int32 length) = 0;
	virtual int32 GetSerializedSize() = 0;
};