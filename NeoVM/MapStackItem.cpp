#include "MapStackItem.h"

MapStackItem::MapStackItem() :IStackItem(EStackItemType::Map)
{

}

MapStackItem::~MapStackItem()
{

}

int MapStackItem::Count()
{
	return -1;
}

bool MapStackItem::GetBoolean()
{
	return true;
}

BigInteger * MapStackItem::GetBigInteger()
{
	return NULL;
}

bool MapStackItem::GetInt32(int &ret)
{
	return false;
}

int MapStackItem::ReadByteArray(unsigned char * output, int sourceIndex, int count)
{
	return -1;
}

int MapStackItem::ReadByteArraySize()
{
	return -1;
}

IStackItem* MapStackItem::Clone()
{
	// TODO: Fix
	return new MapStackItem();
}

void MapStackItem::Set(IStackItem* key, IStackItem* value)
{

}

// Serialize

int MapStackItem::Serialize(unsigned char * data, int length)
{
	return 0;
}

int MapStackItem::GetSerializedSize()
{
	return 0;
}