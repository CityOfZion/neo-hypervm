#include "IStackItem.h"

// Constructor

IStackItem::IStackItem(EStackItemType type) :Type(type), Claims(0) { }

// Methods

void IStackItem::Free(IStackItem* &item)
{
	if (item == NULL) return;

	if (item->Claims == 0)
	{
		// Is zero because if the item is cloned you can call this method twice (PUSH1,DUP,EQUAL)

		delete(item);
		item = NULL;
	}
}