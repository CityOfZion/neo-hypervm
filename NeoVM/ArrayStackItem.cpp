#include "ArrayStackItem.h"
#include "BoolStackItem.h"

ArrayStackItem::ArrayStackItem(bool isStruct) :
	IStackItem(isStruct ? EStackItemType::Struct : EStackItemType::Array), IsStruct(isStruct)
{
	List = std::list<IStackItem*>();
}

ArrayStackItem::ArrayStackItem(bool isStruct, int count) :
	IStackItem(isStruct ? EStackItemType::Struct : EStackItemType::Array), IsStruct(isStruct)
{
	List = std::list<IStackItem*>();

	// Init size
	for (int i = 0; i < count; i++)
		this->List.push_back(new BoolStackItem(false));
}

ArrayStackItem::~ArrayStackItem()
{
	this->Clear();
}

// Serialize

int ArrayStackItem::Serialize(unsigned char * data, int length)
{
	return 0;
}

int ArrayStackItem::GetSerializedSize()
{
	return 0;
}

bool ArrayStackItem::GetBoolean()
{
	return true;
}

BigInteger * ArrayStackItem::GetBigInteger()
{
	return NULL;
}

bool ArrayStackItem::GetInt32(int &ret)
{
	return false;
}

int ArrayStackItem::ReadByteArray(unsigned char * output, int sourceIndex, int count)
{
	return -1;
}

int ArrayStackItem::ReadByteArraySize()
{
	return -1;
}

IStackItem* ArrayStackItem::Clone()
{
	ArrayStackItem* ret = new ArrayStackItem(this->IsStruct);

	for (int x = 0, m = this->Count(); x < m; x++)
		ret->Add(this->Get(x)->Clone());

	return ret;
}

int ArrayStackItem::Count()
{
	return static_cast<int>(this->List.size());
}

void ArrayStackItem::Clear()
{
	for (std::list<IStackItem*>::iterator it = this->List.begin(); it != this->List.end(); ++it)
	{
		IStackItem* ptr = (IStackItem*)*it;
		IStackItem::Free(ptr);
	}

	this->List.clear();
}

IStackItem* ArrayStackItem::Get(int index)
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

void ArrayStackItem::Set(int index, IStackItem* item, bool disposePrev)
{
	if (item != NULL)
		item->Claims++;

	std::list<IStackItem*>::iterator it = this->List.begin();
	if (index > 0) std::advance(it, index);

	IStackItem* & s(*it);

	if (s != NULL && disposePrev)
		IStackItem::Free(s);

	s = item;
}

int ArrayStackItem::IndexOf(IStackItem* item)
{
	auto it = std::find(this->List.begin(), this->List.end(), item);
	if (it == this->List.end())
		return -1;

	return std::distance(this->List.begin(), it);
}

void ArrayStackItem::Insert(int index, IStackItem* item)
{
	if (item != NULL)
		item->Claims++;

	std::list<IStackItem*>::iterator it = this->List.begin();
	if (index > 0) std::advance(it, index);

	this->List.insert(it, item);
}

void ArrayStackItem::RemoveAt(int index, bool dispose)
{
	std::list<IStackItem*>::iterator it = this->List.begin();
	if (index > 0) std::advance(it, index);

	IStackItem* & s(*it);

	if (s != NULL && dispose)
		IStackItem::Free(s);

	this->List.erase(it);
}