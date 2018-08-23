#pragma once

#include "IStackItem.h"
#include "ArrayStackItem.h"
#include <map>

class MapStackItem : public IStackItem
{
private:

	std::map<IStackItem*, IStackItem*> _dictionary;

public:

	// Converters

	inline bool GetBoolean() { return true; }
	inline BigInteger* GetBigInteger() { return NULL; }
	inline bool GetInt32(int32 &ret) { return false; }
	inline int32 ReadByteArray(byte* output, int32 sourceIndex, int32 count) { return -1; }
	inline int32 ReadByteArraySize() { return -1; }

	void Clear();
	
	inline bool Equals(IStackItem* it)
	{
		return (it == this);
	}

	// Map Methods

	inline int32 Count()
	{
		return static_cast<int>(this->_dictionary.size());
	}
	
	bool Set(IStackItem* key, IStackItem* value);
	IStackItem* Get(IStackItem* key);
	bool Remove(IStackItem* &key);
	void FillKeys(ArrayStackItem* arr);
	void FillValues(ArrayStackItem* arr);
	IStackItem* GetKey(int index);
	IStackItem* GetValue(int index);

	// Constructor & Destructor

	inline MapStackItem() :IStackItem(EStackItemType::Map), _dictionary(std::map<IStackItem*, IStackItem*>()) { }

	inline ~MapStackItem() { this->Clear(); }

	// Serialize

	inline int32 Serialize(byte* data, int32 length) { return 0; }
	inline int32 GetSerializedSize() { return 0; }
};