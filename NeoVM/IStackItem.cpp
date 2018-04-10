#include "IStackItem.h"

// Constructor

IStackItem::IStackItem(EStackItemType type) :Claims(0), Type(type) { }

// Methods

void IStackItem::Free(IStackItem* &item)
{
	if (item != NULL && item->Claims == 0)
	{
		// Is zero because if the item is cloned you can call this method twice (PUSH1,DUP,EQUAL)

		delete(item);
		item = NULL;
	}
}

void IStackItem::UnClaim() { this->Claims--; }

void IStackItem::Claim() { this->Claims++; }

void IStackItem::UnclaimAndFree(IStackItem* &item)
{
	if (item == NULL) return;

	if (item->Claims == 1)
	{
		item->Claims = 0;

		// Is zero because if the item is cloned you can call this method twice (PUSH1,DUP,EQUAL)

		delete(item);
		item = NULL;
	}
	else
	{
		item->Claims--;
	}
}