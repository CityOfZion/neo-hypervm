#include "StackItems.h"
#include "ELogStackOperation.h"

int32 StackItems::Count()
{
	return this->Size;
}

void StackItems::Push(IStackItem* it)
{
	this->Size++;
	this->Stack.push_front(it);
	it->Claim();

	if (this->Log != NULL)
	{
		this->Log(it, this->Size - 1, ELogStackOperation::Push);
	}
}

void StackItems::Insert(int32 index, IStackItem* it)
{
	it->Claim();

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

IStackItem* StackItems::TryPeek(int32 index)
{
	if (index < 0)
	{
		index += this->Size;
	}
	if (this->Size <= index || index < 0)
	{
		if (this->Log != NULL)
		{
			this->Log(NULL, index, ELogStackOperation::TryPeek);
		}

		return NULL;
	}
	if (index == 0)
	{
		IStackItem* ret = this->Stack.front();

		if (this->Log != NULL)
		{
			this->Log(ret, index, ELogStackOperation::TryPeek);
		}

		return ret;
	}

	std::list<IStackItem*>::iterator it = this->Stack.begin();
	std::advance(it, index);

	if (this->Log != NULL)
	{
		this->Log((IStackItem*)*it, index, ELogStackOperation::TryPeek);
	}

	return (IStackItem*)*it;
}

IStackItem* StackItems::Peek(int32 index)
{
	if (index < 0)
	{
		index += this->Size;
	}
	if (index == 0)
	{
		IStackItem* ret = this->Stack.front();

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
		index += this->Size;
	}

	std::list<IStackItem*>::iterator it = this->Stack.begin();
	if (index > 0) std::advance(it, index);

	IStackItem* itr = (IStackItem*)*it;

	this->Size--;
	this->Stack.erase(it);

	if (this->Log != NULL)
	{
		this->Log(itr, index, ELogStackOperation::Remove);
	}

	itr->UnClaim();
	return itr;
}

void StackItems::Drop()
{
	IStackItem* it = this->Stack.front();

	this->Size--;
	this->Stack.pop_front();

	if (this->Log != NULL)
	{
		this->Log(it, this->Size, ELogStackOperation::Drop);
	}

	IStackItem::UnclaimAndFree(it);
}

IStackItem* StackItems::Pop()
{
	IStackItem* it = this->Stack.front();
	this->Size--;
	this->Stack.pop_front();

	if (this->Log != NULL)
	{
		this->Log(it, this->Size, ELogStackOperation::Pop);
	}

	it->UnClaim();
	return it;
}

void StackItems::Clear()
{
	if (this->Log != NULL)
	{
		// TODO: Make this log in right order (end to start)

		int32 index = 0;
		for (std::list<IStackItem*>::iterator it = this->Stack.begin(); it != this->Stack.end(); ++it)
		{
			IStackItem* ptr = (IStackItem*)*it;

			Log(ptr, index, ELogStackOperation::Drop);
			++index;

			IStackItem::UnclaimAndFree(ptr);
		}
	}
	else
	{
		for (std::list<IStackItem*>::iterator it = this->Stack.begin(); it != this->Stack.end(); ++it)
		{
			IStackItem* ptr = (IStackItem*)*it;
			IStackItem::UnclaimAndFree(ptr);
		}
	}

	this->Size = 0;
	this->Stack.clear();
}

void StackItems::CopyTo(StackItems* stack, int32 count)
{
	if (stack == NULL || count == 0) return;

	if (count == -1)
	{
		count = this->Size;
	}

	std::list<IStackItem*>::iterator it = this->Stack.begin();
	std::advance(it, this->Size - count);

	for (; it != this->Stack.end(); ++it)
	{
		IStackItem* ptr = (IStackItem*)*it;
		stack->Insert(stack->Size, ptr);
	}
}

StackItems::~StackItems()
{
	this->Clear();
	this->Log = NULL;
}