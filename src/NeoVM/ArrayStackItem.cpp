#include "ArrayStackItem.h"
#include "BoolStackItem.h"
#include "StackItemHelper.h"

ArrayStackItem::ArrayStackItem(IStackItemCounter* counter) :
	IStackItem(counter, EStackItemType::Array),
	_list(std::list<IStackItem*>())
{ }

ArrayStackItem::ArrayStackItem(IStackItemCounter* counter, bool isStruct) :
	IStackItem(counter, (isStruct ? EStackItemType::Struct : EStackItemType::Array)),
	_list(std::list<IStackItem*>())
{ }

IStackItem* ArrayStackItem::Clone()
{
	auto ret = new ArrayStackItem(_counter, this->Type == EStackItemType::Struct);

	for (int32 x = 0, m = this->Count(); x < m; ++x)
	{
		auto i = this->Get(x);

		if (i->Type == EStackItemType::Struct)
			ret->Add(((ArrayStackItem*)i)->Clone());
		else
			ret->Add(i);
	}

	return ret;
}

bool ArrayStackItem::Equals(IStackItem* it)
{
	if (it == this) return true;

	// Different type (Array must be equal pointer)

	if (it->Type != EStackItemType::Struct)
	{
		return false;
	}

	// Different size

	int32 c = this->Count();
	auto arr = (ArrayStackItem*)it;

	if (arr->Count() != c)
	{
		return false;
	}

	// Check sequence

	for (int32 x = 0; x < c; ++x)
	{
		if (!this->Get(x)->Equals(arr->Get(x)))
			return false;
	}

	return true;
}

// Read

IStackItem* ArrayStackItem::Get(int32 index)
{
	if (index == 0)
		return this->_list.front();

	auto it = this->_list.begin();
	std::advance(it, index);

	return (IStackItem*)*it;
}

int32 ArrayStackItem::IndexOf(IStackItem* item)
{
	int32 index = 0;
	for (auto it = this->_list.begin(); it != this->_list.end(); ++it)
	{
		if ((IStackItem*)*it == item)
			return index;

		++index;
	}
	return -1;
}

// Write

void ArrayStackItem::Clear()
{
	for (std::list<IStackItem*>::iterator it = this->_list.begin(); it != this->_list.end(); ++it)
	{
		IStackItem* ptr = (IStackItem*)*it;
		StackItemHelper::UnclaimAndFree(ptr);
	}

	this->_list.clear();
}

void ArrayStackItem::Insert(int32 index, IStackItem* item)
{
	if (item != NULL)
		item->Claim();

	if (index == 0)
	{
		this->_list.push_front(item);
	}
	else
	{
		std::list<IStackItem*>::iterator it = this->_list.begin();
		std::advance(it, index);

		this->_list.insert(it, item);
	}
}

void ArrayStackItem::Add(IStackItem* item)
{
	if (item != NULL)
		item->Claim();

	this->_list.push_back(item);
}

void ArrayStackItem::RemoveAt(int32 index)
{
	if (index == 0)
	{
		IStackItem* it = this->_list.front();
		this->_list.pop_front();
		StackItemHelper::UnclaimAndFree(it);
	}
	else
	{
		std::list<IStackItem*>::iterator it = this->_list.begin();
		std::advance(it, index);

		IStackItem* s = (IStackItem*)*it;
		this->_list.erase(it);
		StackItemHelper::UnclaimAndFree(s);
	}
}

void ArrayStackItem::Set(int32 index, IStackItem* item)
{
	if (item != NULL)
		item->Claim();

	if (index == 0)
	{
		IStackItem* it = this->_list.front();
		this->_list.pop_front();
		StackItemHelper::UnclaimAndFree(it);
		this->_list.push_front(item);
	}
	else
	{
		std::list<IStackItem*>::iterator it = this->_list.begin();
		std::advance(it, index);

		IStackItem* s = (IStackItem*)*it;
		StackItemHelper::UnclaimAndFree(s);

		*it = item;
	}
}