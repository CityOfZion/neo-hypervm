#pragma once
#include "IStackItem.h"
#include <list>

class ArrayStackItem : public IStackItem
{
private:

	std::list<IStackItem*> List;

public:

	// Converters

	bool GetBoolean();
	BigInteger * GetBigInteger();
	bool GetInt32(int &ret);
	IStackItem* Clone();
	int ReadByteArray(unsigned char * output, int sourceIndex, int count);
	int ReadByteArraySize();

	// Is Struct?

	const bool IsStruct;

	int Count();
	void Clear();
	IStackItem* Get(int index);
	void Add(IStackItem* item);
	void Set(int index, IStackItem* item, bool disposePrev);
	void Insert(int index, IStackItem* item);
	void RemoveAt(int index, bool dispose);
	int IndexOf(IStackItem* item);

	// Constructor

	ArrayStackItem(bool isStruct);
	ArrayStackItem(bool isStruct, int count);

	// Destructor

	~ArrayStackItem();

	// Serialize

	int Serialize(unsigned char * data, int length);
	int GetSerializedSize();
};