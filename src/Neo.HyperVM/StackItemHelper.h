#pragma once

#include "IStackItem.h"

class StackItemHelper
{
public:

	static void Free(IStackItem* &item);
	static void Free(IStackItem* &itemA, IStackItem* &itemB);
	static void Free(IStackItem* &itemA, IStackItem* &itemB, IStackItem* &itemC);

	static void UnclaimAndFree(IStackItem* &item);
};