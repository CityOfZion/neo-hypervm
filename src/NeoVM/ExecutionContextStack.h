#pragma once

#include <list>
#include "ExecutionContext.h"

class ExecutionContextStack
{
	int32 Size = 0;
	std::list<ExecutionContext*> Stack;

public:

	OnStackChangeCallback Log;
	int32 Count();

	void Drop();
	void Clear();
	void Remove(int32 index);
	ExecutionContext* Pop();
	void Push(ExecutionContext* i);
	ExecutionContext* Peek(int32 index);
	ExecutionContext* TryPeek(int32 index);

	~ExecutionContextStack();
};