#include "ArrayStackItem.h"
#include "BoolStackItem.h"
#include "StackItemHelper.h"
#include "Stack.h"

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

	Stack<IStackItem> queue;

	queue.Push(this);
	queue.Push(ret);

	while (queue.Count() > 0)
	{
		auto a = (ArrayStackItem*)queue.Pop();
		auto b = (ArrayStackItem*)queue.Pop();

		for (int32 x = 0, m = b->Count(); x < m; ++x)
		{
			auto sb = b->Get(x);

			if (sb->Type == EStackItemType::Struct)
			{
				auto sa = new ArrayStackItem(_counter, true);
				a->Add(sa);

				queue.Insert(queue.Count(), sa);
				queue.Insert(queue.Count(), sb);
			}
			else
			{
				a->Add(sb);
			}
		}
	}

	return ret;
}

bool ArrayStackItem::Equals(IStackItem* it)
{
	if (it == NULL) return false;
	if (it == this) return true;

	// Different type (Array must be equal pointer)

	if (it->Type != EStackItemType::Struct)
	{
		return false;
	}

	// Init stacks

	Stack<IStackItem> stack1;
	Stack<IStackItem> stack2;

	stack1.Push(this);
	stack2.Push(it);

	// Check sequence

	while (stack1.Count() > 0)
	{
		auto a = stack1.Pop();
		auto b = stack2.Pop();

		if (a->Type == EStackItemType::Struct)
		{
			if (a == b) continue;

			auto sa = (ArrayStackItem*)a;

			if (b->Type != EStackItemType::Struct) return false;

			auto sb = (ArrayStackItem*)b;

			int32 sac = sa->Count();
			int32 sbc = sb->Count();
			
			if (sac != sbc) return false;

			for (int32 x = 0; x < sac; x++) 
			{
				stack1.Push(sa->Get(x));
			}

			for (int32 x = 0; x < sbc; x++)
			{
				stack2.Push(sb->Get(x));
			}
		}
		else
		{
			if (!a->Equals(b)) return false;
		}
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