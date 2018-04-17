#pragma once

#include "IStackItem.h"
#include "ArrayStackItem.h"
#include <map>

class MapStackItem : public IStackItem
{
private:

	std::map<IStackItem*, IStackItem*> Dictionary;

public:

	// Converters

	bool GetBoolean();
	BigInteger * GetBigInteger();
	bool GetInt32(int32 &ret);
	int32 ReadByteArray(byte * output, int32 sourceIndex, int32 count);
	int32 ReadByteArraySize();
	void Clear();
	bool Equals(IStackItem * it);

	// Map Methods

	int32 Count();
	bool Set(IStackItem* key, IStackItem* value);
	IStackItem* Get(IStackItem* key);
	bool Remove(IStackItem* key, bool dispose);
	void FillKeys(ArrayStackItem* arr);
	void FillValues(ArrayStackItem* arr);

	// Constructor

	MapStackItem();

	// Destructor

	~MapStackItem();

	// Serialize

	int32 Serialize(byte * data, int32 length);
	int32 GetSerializedSize();
};