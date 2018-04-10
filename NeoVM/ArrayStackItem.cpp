#include "ArrayStackItem.h"
#include "BoolStackItem.h"

ArrayStackItem::ArrayStackItem(bool isStruct) :
	IStackItem(isStruct ? EStackItemType::Struct : EStackItemType::Array), List(std::list<IStackItem*>())
{ }

ArrayStackItem::ArrayStackItem(bool isStruct, int32 count) :
	IStackItem(isStruct ? EStackItemType::Struct : EStackItemType::Array), List(std::list<IStackItem*>())
{
	// Init size
	for (int32 i = 0; i < count; i++)
		this->Add(new BoolStackItem(false));
}

ArrayStackItem::~ArrayStackItem()
{
	this->Clear();
}

void ArrayStackItem::Reverse()
{
	this->List.reverse();
}

bool ArrayStackItem::GetBoolean() { return true; }
BigInteger * ArrayStackItem::GetBigInteger() { return NULL; }
bool ArrayStackItem::GetInt32(int32 &ret) { return false; }
int32 ArrayStackItem::ReadByteArray(byte * output, int32 sourceIndex, int32 count) { return -1; }
int32 ArrayStackItem::ReadByteArraySize() { return -1; }

IStackItem* ArrayStackItem::Clone()
{
	ArrayStackItem* ret = new ArrayStackItem(this->Type == EStackItemType::Struct);

	for (int32 x = 0, m = this->Count(); x < m; x++)
	{
		IStackItem* i = this->Get(x);

		if (i->Type == EStackItemType::Struct)
			ret->Add(((ArrayStackItem*)i)->Clone());
		else
			ret->Add(i);
	}

	return ret;
}

bool ArrayStackItem::Equals(IStackItem * it)
{
	if (it == this) return true;

	// Array must be equal

	if (this->Type == EStackItemType::Array)
	{
		return false;
	}

	// Different type

	if (it->Type != EStackItemType::Struct)
	{
		return false;
	}

	// Different size

	int c = this->Count();
	ArrayStackItem* arr = (ArrayStackItem*)it;

	if (arr->Count() != c)
	{
		return false;
	}

	// Check sequence

	for (int32 x = 0; x < c; x++)
	{
		if (!this->Get(x)->Equals(arr->Get(x)))
			return false;
	}

	return true;
}

int32 ArrayStackItem::Count()
{
	return static_cast<int>(this->List.size());
}

void ArrayStackItem::Clear()
{
	for (std::list<IStackItem*>::iterator it = this->List.begin(); it != this->List.end(); ++it)
	{
		IStackItem* ptr = (IStackItem*)*it;
		IStackItem::UnclaimAndFree(ptr);
	}

	this->List.clear();
}

IStackItem* ArrayStackItem::Get(int32 index)
{
	if (index == 0)
		return this->List.front();

	std::list<IStackItem*>::iterator it = this->List.begin();
	std::advance(it, index);

	return (IStackItem*)*it;
}

void ArrayStackItem::Add(IStackItem* item)
{
	if (item != NULL)
		item->Claim();

	this->List.push_back(item);
}

void ArrayStackItem::Set(int32 index, IStackItem* item)
{
	if (item != NULL)
		item->Claim();

	std::list<IStackItem*>::iterator it = this->List.begin();
	if (index > 0) std::advance(it, index);

	IStackItem* & s(*it);
	IStackItem::UnclaimAndFree(s);
	s = item;
}

int32 ArrayStackItem::IndexOf(IStackItem* item)
{
	int index = 0;
	for (std::list<IStackItem*>::iterator it = this->List.begin(); it != this->List.end(); ++it)
	{
		if ((IStackItem*)*it == item) return index;
		index++;
	}
	return -1;
}

void ArrayStackItem::Insert(int32 index, IStackItem* item)
{
	if (item != NULL)
		item->Claim();

	std::list<IStackItem*>::iterator it = this->List.begin();
	if (index > 0) std::advance(it, index);

	this->List.insert(it, item);
}

void ArrayStackItem::RemoveAt(int32 index)
{
	std::list<IStackItem*>::iterator it = this->List.begin();
	if (index > 0) std::advance(it, index);

	IStackItem* & s(*it);
	IStackItem::UnclaimAndFree(s);

	this->List.erase(it);
}

// Serialize

int32 ArrayStackItem::Serialize(byte * data, int32 length) { return 0; }
int32 ArrayStackItem::GetSerializedSize() { return 0; }