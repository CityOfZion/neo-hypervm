#include "ArrayStackItem.h"
#include "BoolStackItem.h"

ArrayStackItem::ArrayStackItem(bool isStruct) :
	IStackItem(isStruct ? EStackItemType::Struct : EStackItemType::Array), IsStruct(isStruct)
{
	List = std::list<IStackItem*>();
}

ArrayStackItem::ArrayStackItem(bool isStruct, int32 count) :
	IStackItem(isStruct ? EStackItemType::Struct : EStackItemType::Array), IsStruct(isStruct)
{
	List = std::list<IStackItem*>();

	// Init size
	for (int32 i = 0; i < count; i++)
		Add(new BoolStackItem(false));
}

ArrayStackItem::~ArrayStackItem()
{
	this->Clear();
}

// Serialize

int32 ArrayStackItem::Serialize(byte * data, int32 length)
{
	return 0;
}

int32 ArrayStackItem::GetSerializedSize()
{
	return 0;
}

bool ArrayStackItem::GetBoolean()
{
	return true;
}

void ArrayStackItem::Reverse()
{
	this->List.reverse();
}

BigInteger * ArrayStackItem::GetBigInteger()
{
	return NULL;
}

bool ArrayStackItem::GetInt32(int32 &ret)
{
	return false;
}

int32 ArrayStackItem::ReadByteArray(byte * output, int32 sourceIndex, int32 count)
{
	return -1;
}

int32 ArrayStackItem::ReadByteArraySize()
{
	return -1;
}

IStackItem* ArrayStackItem::Clone()
{
	ArrayStackItem* ret = new ArrayStackItem(this->IsStruct);

	for (int32 x = 0, m = this->Count(); x < m; x++)
	{
		IStackItem* i = this->Get(x);
		
		if (i->Type == EStackItemType::Struct)
			ret->Add(i->Clone());
		else
			ret->Add(i);
	}

	return ret;
}

bool ArrayStackItem::Equals(IStackItem * it)
{
	return (it == this);
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

		if (ptr != NULL)
		{
			ptr->Claims--;
			IStackItem::Free(ptr);
		}
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
		item->Claims++;

	this->List.push_back(item);
}

void ArrayStackItem::Set(int32 index, IStackItem* item, bool disposePrev)
{
	if (item != NULL)
		item->Claims++;

	std::list<IStackItem*>::iterator it = this->List.begin();
	if (index > 0) std::advance(it, index);

	IStackItem* & s(*it);

	if (s != NULL)
	{
		s->Claims--;

		if (disposePrev)
			IStackItem::Free(s);
	}

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
		item->Claims++;

	std::list<IStackItem*>::iterator it = this->List.begin();
	if (index > 0) std::advance(it, index);

	this->List.insert(it, item);
}

void ArrayStackItem::RemoveAt(int32 index, bool dispose)
{
	std::list<IStackItem*>::iterator it = this->List.begin();
	if (index > 0) std::advance(it, index);

	IStackItem* & s(*it);

	if (s != NULL && dispose)
	{
		s->Claims--;
		IStackItem::Free(s);
	}

	this->List.erase(it);
}