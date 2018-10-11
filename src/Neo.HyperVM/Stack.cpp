#include "Stack.h"
#include "IStackItem.h"
#include "ExecutionContext.h"

template <class T>
void Stack<T>::RealRemoveAt(int32 index)
{
	this->_size--;

	if (index < this->_size)
	{
		for (int32 x = index; x < this->_size; ++x)
		{
			this->_items[x] = this->_items[x + 1];
		}
	}

	this->_items[this->_size] = NULL;
}

template <class T>
void Stack<T>::Drop(int32 index)
{
	if (index < 0)
	{
		index += this->_size;
	}

	if (index >= this->_size)
	{
		return;
	}

	this->RealRemoveAt(this->_size - index - 1);
}

template <class T>
T* Stack<T>::Pop(int32 index)
{
	if (index < 0)
	{
		index += this->_size;
	}

	if (index >= this->_size)
	{
		return NULL;
	}

	int32 pos = this->_size - index - 1;
	auto ret = this->_items[pos];

	this->RealRemoveAt(pos);

	return ret;
}

template <class T>
void Stack<T>::Insert(int32 index, T* item)
{
	if (index > this->_size)
	{
		return;
	}

	if (this->_size == this->_itemsLength)
	{
		this->EnsureCapacity(this->_size + 1);
	}

	index = this->_size - index;

	if (index < this->_size)
	{
		for (int32 x = this->_size; x > index; x--)
		{
			this->_items[x] = this->_items[x - 1];
		}
	}

	this->_items[index] = item;
	++this->_size;
}

template <class T>
void Stack<T>::EnsureCapacity(int32 min)
{
	if (this->_itemsLength >= min)
	{
		return;
	}

	int32 num = (this->_itemsLength == 0) ? 4 : (this->_itemsLength * 2);

	if (num > 2146435071)
	{
		num = 2146435071;
	}
	else if (num < min)
	{
		num = min;
	}

	if (num != this->_itemsLength)
	{
		auto array = new T*[num];

		if (this->_size > 0)
		{
			// Copy array

			for (int32 x = 0; x < this->_size; ++x)
			{
				array[x] = this->_items[x];
			}
		}

		if (this->_items != NULL)
		{
			delete[](this->_items);
		}

		this->_items = array;
		this->_itemsLength = num;
	}
}

template <class T>
T* Stack<T>::Pop()
{
	if (this->_size == 0)
	{
		return NULL;
	}

	auto pos = this->_size - 1;
	auto ret = this->_items[pos];

	this->RealRemoveAt(pos);

	return ret;
}

template <class T>
void Stack<T>::SendTo(Stack<T>* stack, int32 count)
{
	if (count == -1)
	{
		count = this->_size;
	}
	else if (count > this->_size)
	{
		count = this->_size;
	}

	if (count <= 0) return;

	stack->EnsureCapacity(stack->_size + count);

	for (int32 x = this->_size - count, mx = x + count; x < mx; x++)
	{
		auto item = this->_items[x];

		if (item == NULL) break;

		stack->_items[stack->_size] = item;
		++stack->_size;

		// Send to

		this->_items[x] = NULL;
		--this->_size;
	}
}

template <class T>
void Stack<T>::Push(T* item)
{
	if (this->_size == this->_itemsLength)
	{
		this->EnsureCapacity(this->_size + 1);
	}

	this->_items[this->_size] = item;
	++this->_size;
}

template <class T>
T* Stack<T>::Peek(int32 index) const
{
	if (this->_size == 0)
	{
		return NULL;
	}

	if (index < 0)
	{
		index += this->_size;
	}

	if (index >= this->_size)
	{
		return NULL;
	}

	return this->_items[this->_size - index - 1];
}

template <class T>
void Stack<T>::Clear()
{
	if (this->_itemsLength == 0) return;

	this->_size = 0;
	this->_itemsLength = 0;

	delete[](this->_items);
	this->_items = NULL;
}

// Explicit template instantiation

template class Stack<ExecutionContext>;
template class Stack<IStackItem>;