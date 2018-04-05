#include "StackItems.h"
#include "ELogStackOperation.h"

int32 StackItems::Count()
{
	return this->Size;
}

void StackItems::Push(IStackItem * it)
{
	if (it != NULL)
		it->Claims++;

	this->Size++;
	this->Stack.push_front(it);

	if (this->Log != NULL)
	{
		this->Log(it, this->Size - 1, ELogStackOperation::Push);
	}
}

void StackItems::Insert(int32 index, IStackItem *it)
{
	if (it != NULL)
		it->Claims++;

	if (index == this->Size)
	{
		this->Size++;
		this->Stack.push_back(it);

		if (this->Log != NULL)
		{
			this->Log(it, index, ELogStackOperation::Insert);
		}

		return;
	}

	std::list<IStackItem*>::iterator iter = this->Stack.begin();
	if (index > 0)
	{
		std::advance(iter, index);
	}

	this->Size++;
	this->Stack.insert(iter, it);

	if (this->Log != NULL)
	{
		this->Log(it, index, ELogStackOperation::Insert);
	}
}

IStackItem* StackItems::Peek(int32 index)
{
	if (index == 0)
	{
		IStackItem * ret = this->Stack.front();

		if (this->Log != NULL)
		{
			this->Log(ret, index, ELogStackOperation::Peek);
		}

		return ret;
	}
	if (index < 0)
	{
		if (this->Log != NULL)
		{
			this->Log(NULL, index, ELogStackOperation::Peek);
		}

		return NULL;
	}

	std::list<IStackItem*>::iterator it = this->Stack.begin();
	std::advance(it, index);

	if (this->Log != NULL)
	{
		this->Log((IStackItem*)*it, index, ELogStackOperation::Peek);
	}

	return (IStackItem*)*it;
}

IStackItem* StackItems::Remove(int32 index)
{
	if (index < 0) 
	{
		if (this->Log != NULL)
		{
			this->Log(NULL, index, ELogStackOperation::Remove);
		}

		return NULL;
	}

	std::list<IStackItem*>::iterator it = this->Stack.begin();
	if (index > 0) std::advance(it, index);

	IStackItem *itr = (IStackItem*)*it;

	this->Size--;
	this->Stack.erase(it);

	if (this->Log != NULL)
	{
		this->Log(itr, index, ELogStackOperation::Remove);
	}

	return itr;
}

void StackItems::Drop()
{
	IStackItem *it = this->Stack.front();

	this->Size--;
	this->Stack.pop_front();

	if (this->Log != NULL)
	{
		this->Log(it, this->Size, ELogStackOperation::Drop);
	}

	IStackItem::Free(it);
}

IStackItem* StackItems::Pop()
{
	IStackItem *it = this->Stack.front();
	this->Size--;
	this->Stack.pop_front();

	if (this->Log != NULL)
	{
		this->Log(it, this->Size, ELogStackOperation::Pop);
	}

	return it;
}

StackItems::~StackItems()
{
	if (this->Log != NULL)
	{
		// TODO: Make this log in right order (end to start)

		int32 index = 0;
		for (std::list<IStackItem*>::iterator it = this->Stack.begin(); it != this->Stack.end(); ++it)
		{
			IStackItem* ptr = (IStackItem*)*it;

			Log(ptr, index, ELogStackOperation::Drop);
			index++;

			IStackItem::Free(ptr);
		}

		this->Log = NULL;
	}
	else 
	{
		for (std::list<IStackItem*>::iterator it = this->Stack.begin(); it != this->Stack.end(); ++it)
		{
			IStackItem* ptr = (IStackItem*)*it;
			IStackItem::Free(ptr);
		}
	}
	
	this->Size = 0;
	this->Stack.clear();
}