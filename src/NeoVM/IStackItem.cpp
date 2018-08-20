#include "IStackItem.h"

// Methods

void IStackItem::Free(IStackItem* &item)
{
	if (item != NULL && item->IsUnClaimed())
	{
		// Is zero because if the item is cloned you can call this method twice (PUSH1,DUP,EQUAL)
		// But in Linux doesn't work (next call is not NULL) careful!

		delete(item);
		item = NULL;
	}
}

void IStackItem::Free(IStackItem* &itemA, IStackItem* &itemB, IStackItem* &itemC)
{
	if (itemA != NULL && itemA == itemB && itemB == itemC)
	{
		// All equals

		Free(itemA);

		itemB = NULL;
		itemC = NULL;
		return;
	}

	if (itemA != NULL && itemA == itemB)
	{
		// A=B

		Free(itemA);
		Free(itemC);

		itemB = NULL;
		return;
	}

	if (itemB != NULL && itemB == itemC)
	{
		// B=C

		Free(itemB);
		Free(itemA);

		itemC = NULL;
		return;
	}

	if (itemA != NULL && itemA == itemC)
	{
		// A=C

		Free(itemA);
		Free(itemB);

		itemC = NULL;
		return;
	}

	// Differents

	if (itemA != NULL) Free(itemA);
	if (itemB != NULL) Free(itemB);
	if (itemC != NULL) Free(itemC);
}

void IStackItem::Free(IStackItem* &itemA, IStackItem* &itemB)
{
	if (itemA != NULL && itemA->IsUnClaimed())
	{
		// Check equals

		if (itemB == itemA)
		{
			itemB = NULL;
		}

		delete(itemA);
		itemA = NULL;
	}

	if (itemB != NULL && itemB->IsUnClaimed())
	{
		delete(itemB);
		itemB = NULL;
	}
}

void IStackItem::UnclaimAndFree(IStackItem* &item)
{
	if (item != NULL && item->UnClaim())
	{
		// Is zero because if the item is cloned you can call this method twice (PUSH1,DUP,EQUAL)

		delete(item);
		item = NULL;
	}
}