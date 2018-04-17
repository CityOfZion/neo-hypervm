#include "MapStackItem.h"

MapStackItem::MapStackItem() :IStackItem(EStackItemType::Map), Dictionary(std::map<IStackItem*, IStackItem*>()) { }

MapStackItem::~MapStackItem() { this->Clear(); }

int32 MapStackItem::Count()
{
	return static_cast<int>(this->Dictionary.size());
}

bool MapStackItem::GetBoolean() { return true; }
BigInteger * MapStackItem::GetBigInteger() { return NULL; }
bool MapStackItem::GetInt32(int32 &ret) { return false; }
int32 MapStackItem::ReadByteArray(byte * output, int32 sourceIndex, int32 count) { return -1; }
int32 MapStackItem::ReadByteArraySize() { return -1; }

bool MapStackItem::Remove(IStackItem* key, bool dispose)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		if (!it->first->Equals(key)) continue;

		IStackItem* value = it->second;

		if (dispose)
		{
			if (it->first == key)
				IStackItem::UnclaimAndFree(key);

			IStackItem::UnclaimAndFree(value);
		}
		else 
		{
			value->UnClaim();
		}

		this->Dictionary.erase(key);
		return true;
	}

	return false;
}

void MapStackItem::Clear()
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		IStackItem* key = it->first;
		IStackItem* value = it->second;

		IStackItem::UnclaimAndFree(key);
		IStackItem::UnclaimAndFree(value);
	}

	this->Dictionary.clear();
}

bool MapStackItem::Equals(IStackItem * it)
{
	return (it == this);
}

IStackItem* MapStackItem::Get(IStackItem* key)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		if (!it->first->Equals(key)) continue;
		return it->second;
	}

	return NULL;
}

void MapStackItem::FillKeys(ArrayStackItem* arr)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		if (it->first->Type == EStackItemType::Struct)
		{
			arr->Add(((ArrayStackItem*)it->first)->Clone());
		}
		else
		{
			arr->Add(it->first);
		}
	}
}

void MapStackItem::FillValues(ArrayStackItem* arr)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		if (it->second->Type == EStackItemType::Struct)
		{
			arr->Add(((ArrayStackItem*)it->second)->Clone());
		}
		else
		{
			arr->Add(it->second);
		}
	}
}

bool MapStackItem::Set(IStackItem* key, IStackItem* value)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->Dictionary.begin(); it != this->Dictionary.end(); ++it)
	{
		if (!it->first->Equals(key)) continue;

		IStackItem* v = it->second;
		if (v == value) return false;

		IStackItem::UnclaimAndFree(v);

		value->Claim();
		it->second = value;
		return false;
	}

	key->Claim();
	value->Claim();

	this->Dictionary.emplace(key, value);
	return true;
}

// Serialize

int32 MapStackItem::Serialize(byte * data, int32 length) { return 0; }

int32 MapStackItem::GetSerializedSize() { return 0; }