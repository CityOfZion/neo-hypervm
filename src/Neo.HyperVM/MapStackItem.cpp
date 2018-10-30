#include "MapStackItem.h"
#include "StackItemHelper.h"

bool MapStackItem::Remove(IStackItem* &key)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->_dictionary.begin(); it != this->_dictionary.end(); ++it)
	{
		if (!it->first->Equals(key)) continue;

		IStackItem* value = it->second;

		// Unclaim

		IStackItem* ckey = it->first; // Could be different

		ckey->UnClaim();
		value->UnClaim();

		// Remove

		this->_dictionary.erase(ckey);

		// Free

		bool equal = ckey == key;
		StackItemHelper::Free(ckey, value);
		if (equal) key = ckey;

		return true;
	}

	return false;
}

void MapStackItem::Clear()
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->_dictionary.begin(); it != this->_dictionary.end(); ++it)
	{
		IStackItem* key = it->first;
		IStackItem* value = it->second;

		StackItemHelper::UnclaimAndFree(key);
		StackItemHelper::UnclaimAndFree(value);
	}

	this->_dictionary.clear();
}

IStackItem* MapStackItem::Get(IStackItem* key)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->_dictionary.begin(); it != this->_dictionary.end(); ++it)
	{
		if (!it->first->Equals(key)) continue;
		return it->second;
	}

	return nullptr;
}

IStackItem* MapStackItem::GetKey(int index)
{
	int32 ix = 0;
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->_dictionary.begin(); it != this->_dictionary.end(); ++it, ++ix)
	{
		if (ix == index)
		{
			return it->first;
		}
	}

	return nullptr;
}

IStackItem* MapStackItem::GetValue(int index)
{
	int32 ix = 0;
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->_dictionary.begin(); it != this->_dictionary.end(); ++it, ++ix)
	{
		if (ix == index)
		{
			return it->second;
		}
	}

	return nullptr;
}

void MapStackItem::FillKeys(ArrayStackItem* arr)
{
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->_dictionary.begin(); it != this->_dictionary.end(); ++it)
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
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->_dictionary.begin(); it != this->_dictionary.end(); ++it)
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
	for (std::map<IStackItem*, IStackItem*>::iterator it = this->_dictionary.begin(); it != this->_dictionary.end(); ++it)
	{
		if (!it->first->Equals(key)) continue;

		IStackItem* v = it->second;
		if (v == value) return false;

		StackItemHelper::UnclaimAndFree(v);

		value->Claim();
		it->second = value;
		return false;
	}

	key->Claim();
	value->Claim();

	this->_dictionary.emplace(key, value);
	return true;
}