#include "ExecutionContextStack.h"
#include "ELogStackOperation.h"

int32 ExecutionContextStack::Count()
{
	return this->Size;
}

void ExecutionContextStack::Push(ExecutionContext * i)
{
	this->Size++;
	this->Stack.push_front(i);
	i->Claims++;

	if (this->Log != NULL)
	{
		this->Log(i, this->Size - 1, ELogStackOperation::Push);
	}
}

ExecutionContext* ExecutionContextStack::Peek(int32 index)
{
	if (index == 0)
	{
		ExecutionContext *ret = this->Stack.front();

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

	std::list<ExecutionContext*>::iterator it = this->Stack.begin();
	std::advance(it, index);

	if (this->Log != NULL)
	{
		this->Log((ExecutionContext*)*it, index, ELogStackOperation::Peek);
	}

	return (ExecutionContext*)*it;
}

ExecutionContext* ExecutionContextStack::TryPeek(int32 index)
{
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
		ExecutionContext *ret = this->Stack.front();

		if (this->Log != NULL)
		{
			this->Log(ret, index, ELogStackOperation::TryPeek);
		}

		return ret;
	}

	std::list<ExecutionContext*>::iterator it = this->Stack.begin();
	std::advance(it, index);

	if (this->Log != NULL)
	{
		this->Log((ExecutionContext*)*it, index, ELogStackOperation::TryPeek);
	}

	return (ExecutionContext*)*it;
}

void ExecutionContextStack::Drop()
{
	ExecutionContext *it = this->Stack.front();

	this->Size--;
	this->Stack.pop_front();

	if (this->Log != NULL)
	{
		this->Log(it, this->Size, ELogStackOperation::Drop);
	}

	it->Claims--;
	ExecutionContext::Free(it);
}

ExecutionContext* ExecutionContextStack::Pop()
{
	ExecutionContext *it = this->Stack.front();

	this->Size--;
	this->Stack.pop_front();

	if (this->Log != NULL)
	{
		this->Log(it, this->Size, ELogStackOperation::Pop);
	}

	it->Claims--;
	return it;
}

ExecutionContextStack::~ExecutionContextStack()
{
	if (this->Log != NULL)
	{
		// TODO: Make this log in right order (end to start)

		int32 index = 0;
		for (std::list<ExecutionContext*>::iterator it = this->Stack.begin(); it != this->Stack.end(); ++it)
		{
			ExecutionContext* ptr = (ExecutionContext*)*it;
			this->Log(ptr, index, ELogStackOperation::Drop);
			index++;

			if (ptr == NULL) continue;

			ptr->Claims--;
			ExecutionContext::Free(ptr);
		}
	
		this->Log = NULL;
	}
	else 
	{
		for (std::list<ExecutionContext*>::iterator it = this->Stack.begin(); it != this->Stack.end(); ++it)
		{
			ExecutionContext* ptr = (ExecutionContext*)*it;
			if (ptr == NULL) continue;

			ptr->Claims--;
			ExecutionContext::Free(ptr);
		}
	}

	this->Size = 0;
	this->Stack.clear();
}