#pragma once

#include "IStackItemCounter.h"
#include "IStackItem.h"
#include <list>

class ArrayStackItem : public IStackItem
{
private:

	std::list<IStackItem*> _list;

public:

	// Converters

	inline bool GetBoolean() { return true; }
	inline BigInteger* GetBigInteger() { return NULL; }
	inline bool GetInt32(int32 &ret) { return false; }
	inline int32 ReadByteArray(byte* output, int32 sourceIndex, int32 count) { return -1; }
	inline int32 ReadByteArraySize() { return -1; }

	inline void Reverse()
	{
		this->_list.reverse();
	}

	IStackItem* Clone();
	bool Equals(IStackItem* it);

	inline int32 Count()
	{
		return static_cast<int>(this->_list.size());
	}

	void Clear();
	IStackItem* Get(int32 index);
	void Add(IStackItem* item);
	void Set(int32 index, IStackItem* item);
	void Insert(int32 index, IStackItem* item);
	void RemoveAt(int32 index);
	int32 IndexOf(IStackItem* item);

	// Constructor

	ArrayStackItem(volatile IStackItemCounter* &counter);
	ArrayStackItem(volatile IStackItemCounter* &counter, bool isStruct);

	// Destructor

	inline ~ArrayStackItem()
	{
		this->Clear();
	}

	// Serialize

	inline int32 Serialize(byte* data, int32 length) { return 0; }
	inline int32 GetSerializedSize() { return 0; }
};