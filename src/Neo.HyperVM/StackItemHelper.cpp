#include "StackItemHelper.h"

void StackItemHelper::Free(IStackItem* &item)
{
	if (item != nullptr && item->IsUnClaimed())
	{
		// Is zero because if the item is cloned you can call this method twice (PUSH1,DUP,EQUAL)
		// But in Linux doesn't work (next call is not NULL) careful!

		delete(item);
		item = nullptr;
	}
}

void StackItemHelper::Free(IStackItem* &itemA, IStackItem* &itemB, IStackItem* &itemC)
{
	if (itemA != nullptr && itemA == itemB && itemB == itemC)
	{
		// All equals

		Free(itemA);

		itemB = itemA;
		itemC = itemA;
		return;
	}

	if (itemA != nullptr && itemA == itemB)
	{
		// A=B

		Free(itemA);
		Free(itemC);

		itemB = itemA;
		return;
	}

	if (itemB != nullptr && itemB == itemC)
	{
		// B=C

		Free(itemB);
		Free(itemA);

		itemC = itemB;
		return;
	}

	if (itemA != nullptr && itemA == itemC)
	{
		// A=C

		Free(itemA);
		Free(itemB);

		itemC = itemA;
		return;
	}

	// Differents

	if (itemA != nullptr) Free(itemA);
	if (itemB != nullptr) Free(itemB);
	if (itemC != nullptr) Free(itemC);
}

void StackItemHelper::Free(IStackItem* &itemA, IStackItem* &itemB)
{
	if (itemA != nullptr && itemA->IsUnClaimed())
	{
		// Check equals

		if (itemB == itemA)
		{
			itemB = nullptr;
			delete(itemA);
			itemA = nullptr;
			return;
		}

		delete(itemA);
		itemA = nullptr;
	}

	if (itemB != nullptr && itemB->IsUnClaimed())
	{
		delete(itemB);
		itemB = nullptr;
	}
}

void StackItemHelper::UnclaimAndFree(IStackItem* &item)
{
	if (item != nullptr && item->UnClaim())
	{
		// Is zero because if the item is cloned you can call this method twice (PUSH1,DUP,EQUAL)

		delete(item);
		item = nullptr;
	}
}