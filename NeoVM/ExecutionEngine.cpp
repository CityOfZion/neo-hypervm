#include "ExecutionEngine.h"
#include "ByteArrayStackItem.h"
#include "IntegerStackItem.h"
#include "BoolStackItem.h"
#include "MapStackItem.h"
#include "ArrayStackItem.h"
#include "Crypto.h"

// Methods

byte ExecutionEngine::InvokeInterop(const char* method)
{
	// Fill defaults interops here

	return this->InteropCallback(method);
}

// Getters

ExecutionContextStack* ExecutionEngine::GetInvocationStack() { return this->InvocationStack; }
StackItems* ExecutionEngine::GetEvaluationStack() { return this->EvaluationStack; }
StackItems* ExecutionEngine::GetAltStack() { return this->AltStack; }

// Constructor

ExecutionEngine::ExecutionEngine
(
	InvokeInteropCallback invokeInteropCallback, GetScriptCallback getScriptCallback, GetMessageCallback getMessageCallback
)
	: InteropCallback(invokeInteropCallback), ScriptCallback(getScriptCallback), MessageCallback(getMessageCallback)
{
	this->InvocationStack = new ExecutionContextStack();
	this->EvaluationStack = new StackItems();
	this->AltStack = new StackItems();
}

void ExecutionEngine::LoadScript(byte * script, int32 scriptLength)
{
	ExecutionContext * context = new ExecutionContext(script, scriptLength, false, 0);
	this->InvocationStack->Push(context);
}

void ExecutionEngine::LoadPushOnlyScript(byte * script, int32 scriptLength)
{
	ExecutionContext * context = new ExecutionContext(script, scriptLength, true, 0);
	this->InvocationStack->Push(context);
}

ExecutionContext* ExecutionEngine::GetCurrentContext()
{
	return this->InvocationStack->Peek(0);
}

ExecutionContext* ExecutionEngine::GetCallingContext()
{
	return this->InvocationStack->Count() > 1 ? this->InvocationStack->Peek(1) : NULL;
}

ExecutionContext* ExecutionEngine::GetEntryContext()
{
	return this->InvocationStack->Peek(this->InvocationStack->Count() - 1);
}

byte ExecutionEngine::GetState()
{
	return this->State;
}

EVMState ExecutionEngine::Execute()
{
	do
	{
		this->StepInto();
	} while (this->State == EVMState::NONE);

	return this->State;
}

void ExecutionEngine::StepOut()
{
	int32 c = this->InvocationStack->Count();

	while (this->State == EVMState::NONE && this->InvocationStack->Count() >= c)
		this->StepInto();
}

void ExecutionEngine::StepOver()
{
	if (this->State != EVMState::NONE) return;

	int32 c = this->InvocationStack->Count();

	do
	{
		this->StepInto();
	} while (this->State == EVMState::NONE && this->InvocationStack->Count() > c);
}

void ExecutionEngine::StepInto()
{
	if (this->State != EVMState::NONE)
	{
		return;
	}

	ExecutionContext * context = this->InvocationStack->TryPeek(0);

	if (context == NULL)
	{
		this->State = EVMState::HALT;
		return;
	}

	EVMOpCode opcode = context->ReadNextInstruction();

	// Check PushOnly

	if (context->IsPushOnly && opcode > EVMOpCode::PUSH16 && opcode != EVMOpCode::RET)
	{
		this->State = EVMState::FAULT;
		return;
	}

ExecuteOpCode:

	// Check PushBytes

	if (opcode >= EVMOpCode::PUSHBYTES1 && opcode <= EVMOpCode::PUSHBYTES75)
	{
		byte length = (byte)opcode;
		byte *data = new byte[length];

		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}

	// Check Push

	if (opcode >= EVMOpCode::PUSH1 && opcode <= EVMOpCode::PUSH16)
	{
		this->EvaluationStack->Push(new IntegerStackItem(((byte)opcode - (byte)EVMOpCode::PUSH1) + 1));
		return;
	}

	// Execute opcode

	switch (opcode)
	{

#pragma region Push value

	case EVMOpCode::PUSH0:
	{
		this->EvaluationStack->Push(new ByteArrayStackItem(NULL, 0, false));
		return;
	}
	case EVMOpCode::PUSHDATA1:
	{
		byte length = 0;
		if (!context->ReadUInt8(length))
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte *data = new byte[length];
		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}
	case EVMOpCode::PUSHDATA2:
	{
		uint16 length = 0;
		if (!context->ReadUInt16(length))
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte *data = new byte[length];
		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}
	case EVMOpCode::PUSHDATA4:
	{
		int32 length = 0;
		if (!context->ReadInt32(length) || length < 0)
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte *data = new byte[length];
		if (context->Read(data, length) != length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}
	case EVMOpCode::PUSHM1:
	{
		this->EvaluationStack->Push(new IntegerStackItem(new BigInteger(BigInteger::MinusOne), true));
		return;
	}

#pragma endregion

#pragma region Control

	case EVMOpCode::NOP: return;
	case EVMOpCode::JMP:
	case EVMOpCode::JMPIF:
	case EVMOpCode::JMPIFNOT:
	{
		int16 offset = 0;
		if (!context->ReadInt16(offset))
		{
			this->State = EVMState::FAULT;
			return;
		}

		offset = context->InstructionPointer + offset - 3;

		if (offset < 0 || offset > context->ScriptLength)
		{
			this->State = EVMState::FAULT;
			return;
		}

		if (opcode > EVMOpCode::JMP)
		{
			if (this->EvaluationStack->Count() < 1)
			{
				this->State = EVMState::FAULT;
				return;
			}

			IStackItem *bitem = this->EvaluationStack->Pop();

			bool fValue;
			if (opcode == EVMOpCode::JMPIFNOT)
				fValue = !bitem->GetBoolean();
			else
				fValue = bitem->GetBoolean();

			IStackItem::Free(bitem);

			if (fValue)
				context->InstructionPointer = offset;
		}
		else
		{
			context->InstructionPointer = offset;
		}
		return;
	}
	case EVMOpCode::CALL:
	{
		if (context == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		ExecutionContext * clone = context->Clone();

		this->InvocationStack->Push(clone);
		context->InstructionPointer += 2;

		// Jump

		opcode = EVMOpCode::JMP;
		context = clone;
		goto ExecuteOpCode;
	}
	case EVMOpCode::RET:
	{
		if (context != NULL)
		{
			context = NULL;
			this->InvocationStack->Drop();
		}

		if (this->InvocationStack->Count() == 0)
			this->State = EVMState::HALT;

		return;
	}
	case EVMOpCode::APPCALL:
	case EVMOpCode::TAILCALL:
	{
		if (ScriptCallback == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte script_hash[20];
		if (context->Read(script_hash, 20) != 20)
		{
			this->State = EVMState::FAULT;
			return;
		}

		bool isEmpty = true;
		for (int32 x = 0; x < 20; x++)
			if (script_hash[x] != 0x00) { isEmpty = false; break; }

		if (isEmpty)
		{
			if (this->EvaluationStack->Count() < 1)
			{
				this->State = EVMState::FAULT;
				return;
			}

			IStackItem* item = this->EvaluationStack->Pop();
			if (item->ReadByteArray(&script_hash[0], 0, 20) != 20)
			{
				IStackItem::Free(item);
				this->State = EVMState::FAULT;
				return;
			}

			IStackItem::Free(item);
		}

		byte* script = NULL;
		int32 length = ScriptCallback(&(script_hash[0]), script);

		if (length <= 0)
		{
			this->State = EVMState::FAULT;
			return;
		}

		if (opcode == EVMOpCode::TAILCALL)
		{
			delete(this->InvocationStack->Pop());
		}

		this->LoadScript(script, length);
		return;
	}
	case EVMOpCode::SYSCALL:
	{
		int64 length = 0;
		if (!context->ReadVarBytes(length, 252) || length < 0)
		{
			this->State = EVMState::FAULT;
			return;
		}

		char *data = new char[length + 1];
		data[length] = 0x00;

		if (context->Read((byte*)data, length) != length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		if (this->InvokeInterop(data) != 0x01)
			this->State = EVMState::FAULT;

		delete[](data);
		return;
	}

#pragma endregion

#pragma region Stack ops

	case EVMOpCode::DUPFROMALTSTACK:
	{
		if (this->AltStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *it = this->AltStack->Peek(0);
		this->EvaluationStack->Push(it);
		return;
	}
	case EVMOpCode::TOALTSTACK:
	{
		this->AltStack->Push(this->EvaluationStack->Pop());
		return;
	}
	case EVMOpCode::FROMALTSTACK:
	{
		this->EvaluationStack->Push(this->AltStack->Pop());
		return;
	}
	case EVMOpCode::XDROP:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = this->EvaluationStack->Pop();

		int32 n = 0;
		if (!it->GetInt32(n) || n < 0 || ic <= n + 1)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Remove(n);
		IStackItem::Free(it);
		return;
	}
	case EVMOpCode::XSWAP:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = this->EvaluationStack->Pop();

		int32 n = 0;
		if (!it->GetInt32(n) || n < 0 || ic <= n + 1)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		if (n == 0) return;

		IStackItem* xn = this->EvaluationStack->Peek(n);

		it = this->EvaluationStack->Remove(0);
		this->EvaluationStack->Insert(0, xn);

		this->EvaluationStack->Remove(n);
		this->EvaluationStack->Insert(n, it);
		return;
	}
	case EVMOpCode::XTUCK:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = this->EvaluationStack->Pop();

		int32 n = 0;
		if (!it->GetInt32(n) || n <= 0 || ic < n + 1)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		this->EvaluationStack->Insert(n, this->EvaluationStack->Peek(0));
		return;
	}
	case EVMOpCode::DEPTH:
	{
		IntegerStackItem* item = new IntegerStackItem(this->EvaluationStack->Count());
		this->EvaluationStack->Push(item);
		return;
	}
	case EVMOpCode::DROP:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *it = this->EvaluationStack->Pop();
		IStackItem::Free(it);
		return;
	}
	case EVMOpCode::DUP:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *it = this->EvaluationStack->Peek(0);
		this->EvaluationStack->Push(it);
		return;
	}
	case EVMOpCode::NIP:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Pop();

		this->EvaluationStack->Push(x2);
		IStackItem::Free(x1);
		return;
	}
	case EVMOpCode::OVER:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Peek(0);

		this->EvaluationStack->Push(x2);
		this->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::PICK:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = this->EvaluationStack->Pop();

		int32 n = 0;
		if (!it->GetInt32(n) || n < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		if (ic <= n)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(this->EvaluationStack->Peek(n));
		return;
	}
	case EVMOpCode::ROLL:
	{
		int32 ic = this->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = this->EvaluationStack->Pop();

		int32 n = 0;
		if (!it->GetInt32(n) || n < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		if (ic <= n)
		{
			this->State = EVMState::FAULT;
			return;
		}

		if (n == 0) return;

		this->EvaluationStack->Push(this->EvaluationStack->Remove(n));
		return;
	}
	case EVMOpCode::ROT:
	{
		if (this->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x3 = this->EvaluationStack->Pop();
		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Pop();

		this->EvaluationStack->Push(x2);
		this->EvaluationStack->Push(x3);
		this->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::SWAP:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Pop();

		this->EvaluationStack->Push(x2);
		this->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::TUCK:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x2 = this->EvaluationStack->Pop();
		IStackItem *x1 = this->EvaluationStack->Pop();

		this->EvaluationStack->Push(x2);
		this->EvaluationStack->Push(x1);
		this->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::CAT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * x2 = this->EvaluationStack->Pop();
		IStackItem * x1 = this->EvaluationStack->Pop();
		int32 size2 = x2->ReadByteArraySize();
		int32 size1 = x1->ReadByteArraySize();

		if (size2 < 0 || size1 < 0)
		{
			IStackItem::Free(x2);
			IStackItem::Free(x1);

			this->State = EVMState::FAULT;
			return;
		}

		byte * data = new byte[size2 + size1];
		x1->ReadByteArray(&data[0], 0, size1);
		x2->ReadByteArray(&data[size1], 0, size2);

		IStackItem::Free(x2);
		IStackItem::Free(x1);

		ByteArrayStackItem *bi = new ByteArrayStackItem(data, size1 + size2, true);
		this->EvaluationStack->Push(bi);
		return;
	}
	case EVMOpCode::SUBSTR:
	{
		if (this->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = this->EvaluationStack->Pop();
		int32 count = 0;

		if (!it->GetInt32(count) || count < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Pop();
		int32 index = 0;

		if (!it->GetInt32(index) || index < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Pop();

		byte * data = new byte[count];
		if (it->ReadByteArray(&data[0], index, count) != count)
		{
			IStackItem::Free(it);
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		ByteArrayStackItem *bi = new ByteArrayStackItem(data, count, true);
		this->EvaluationStack->Push(bi);
		return;
	}
	case EVMOpCode::LEFT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = this->EvaluationStack->Pop();
		int32 count = 0;

		if (!it->GetInt32(count) || count < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Pop();

		byte * data = new byte[count];
		if (it->ReadByteArray(&data[0], 0, count) != count)
		{
			IStackItem::Free(it);
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		ByteArrayStackItem *bi = new ByteArrayStackItem(data, count, true);
		this->EvaluationStack->Push(bi);
		return;
	}
	case EVMOpCode::RIGHT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = this->EvaluationStack->Pop();
		int32 count = 0;

		if (!it->GetInt32(count) || count < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = this->EvaluationStack->Pop();

		byte * data = new byte[count];
		if (it->ReadByteArray(&data[0], it->ReadByteArraySize() - count, count) != count)
		{
			IStackItem::Free(it);
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		ByteArrayStackItem *bi = new ByteArrayStackItem(data, count, true);
		this->EvaluationStack->Push(bi);
		return;
	}
	case EVMOpCode::SIZE:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();
		IStackItem::Free(item);

		if (size < 0)
		{
			this->State = EVMState::FAULT;
			return;
		}

		item = new IntegerStackItem(size);
		this->EvaluationStack->Push(item);
		return;
	}

#pragma endregion

#pragma region Bitwise logic

	case EVMOpCode::INVERT:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = bi->Invert();
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::AND:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = i1->And(i2);

		delete(i2);
		delete(i1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::OR:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = i1->Or(i2);

		delete(i2);
		delete(i1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::XOR:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = i1->Xor(i2);

		delete(i2);
		delete(i1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::EQUAL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		this->EvaluationStack->Push(new BoolStackItem(x1->Equals(x2)));
		return;
	}

#pragma endregion

#pragma region Numeric

	case EVMOpCode::INC:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *add = new BigInteger(BigInteger::One);
		BigInteger *ret = bi->Add(add);
		delete(add);
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::DEC:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *add = new BigInteger(BigInteger::One);
		BigInteger *ret = bi->Sub(add);
		delete(add);
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::SIGN:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int ret = bi->GetSign();
		delete(bi);

		this->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::NEGATE:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = bi->Negate();
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::ABS:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = bi->Abs();
		delete(bi);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::NOT:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();
		BoolStackItem *bnew = new BoolStackItem(!item->GetBoolean());
		IStackItem::Free(item);

		this->EvaluationStack->Push(bnew);
		return;
	}
	case EVMOpCode::NZ:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x = this->EvaluationStack->Pop();
		BigInteger *i = x->GetBigInteger();
		IStackItem::Free(x);

		if (i == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i->CompareTo(BigInteger::Zero) != 0);
		delete(i);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::ADD:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Add(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::SUB:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Sub(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::MUL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Mul(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::DIV:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Div(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::MOD:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = this->EvaluationStack->Pop();
		IStackItem* i1 = this->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2);
		IStackItem::Free(i1);

		if (x2 == NULL || x1 == NULL)
		{
			if (x2 != NULL) delete(x2);
			if (x1 != NULL) delete(x1);

			this->State = EVMState::FAULT;
			return;
		}

		BigInteger *ret = x1->Mod(x2);
		delete(x2);
		delete(x1);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::SHL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* n = this->EvaluationStack->Pop();
		IStackItem* x = this->EvaluationStack->Pop();

		BigInteger *in = n->GetBigInteger();
		BigInteger *ix = x->GetBigInteger();

		IStackItem::Free(n);
		IStackItem::Free(x);

		int32 iin;
		if (in == NULL || ix == NULL || !in->ToInt32(iin))
		{
			if (ix != NULL) delete(ix);
			if (in != NULL) delete(in);

			this->State = EVMState::FAULT;
			return;
		}

		delete(in);
		BigInteger *ret = ix->Shl(iin);
		delete(ix);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::SHR:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* n = this->EvaluationStack->Pop();
		IStackItem* x = this->EvaluationStack->Pop();

		BigInteger *in = n->GetBigInteger();
		BigInteger *ix = x->GetBigInteger();

		IStackItem::Free(n);
		IStackItem::Free(x);

		int32 iin;
		if (in == NULL || ix == NULL || !in->ToInt32(iin))
		{
			if (ix != NULL) delete(ix);
			if (in != NULL) delete(in);

			this->State = EVMState::FAULT;
			return;
		}

		delete(in);
		BigInteger *ret = ix->Shr(iin);
		delete(ix);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		this->EvaluationStack->Push(new IntegerStackItem(ret, true));
		return;
	}
	case EVMOpCode::BOOLAND:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BoolStackItem * ret = new BoolStackItem(x1->GetBoolean() && x2->GetBoolean());
		IStackItem::Free(x2);
		IStackItem::Free(x1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::BOOLOR:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BoolStackItem * ret = new BoolStackItem(x1->GetBoolean() || x2->GetBoolean());
		IStackItem::Free(x2);
		IStackItem::Free(x1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::NUMEQUAL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) == 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::NUMNOTEQUAL:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) != 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::LT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) < 0);

		delete(i1);
		delete(i2);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::GT:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) > 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::LTE:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) <= 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::GTE:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i1->CompareTo(i2) >= 0);

		delete(i2);
		delete(i1);

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::MIN:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret;
		if (i1->CompareTo(i2) >= 0)
		{
			delete(i1);
			ret = new IntegerStackItem(i2, true);
		}
		else
		{
			delete(i2);
			ret = new IntegerStackItem(i1, true);
		}

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::MAX:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = this->EvaluationStack->Pop();
		IStackItem* x1 = this->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1);
		IStackItem::Free(x2);

		if (i2 == NULL || i1 == NULL)
		{
			if (i2 != NULL) delete(i2);
			if (i1 != NULL) delete(i1);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret;
		if (i1->CompareTo(i2) >= 0)
		{
			delete(i2);
			ret = new IntegerStackItem(i1, true);
		}
		else
		{
			delete(i1);
			ret = new IntegerStackItem(i2, true);
		}

		this->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::WITHIN:
	{
		if (this->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* b = this->EvaluationStack->Pop();
		IStackItem* a = this->EvaluationStack->Pop();
		IStackItem* x = this->EvaluationStack->Pop();

		BigInteger *ib = b->GetBigInteger();
		BigInteger *ia = a->GetBigInteger();
		BigInteger *ix = x->GetBigInteger();

		IStackItem::Free(b);
		IStackItem::Free(a);
		IStackItem::Free(x);

		if (ib == NULL || ia == NULL || ix == NULL)
		{
			if (ib != NULL) delete(ib);
			if (ia != NULL) delete(ia);
			if (ix != NULL) delete(ix);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(ia->CompareTo(ix) <= 0 && ix->CompareTo(ib) < 0);

		delete(ib);
		delete(ia);
		delete(ix);

		this->EvaluationStack->Push(ret);
		return;
	}

#pragma endregion

#pragma region Crypto

	case EVMOpCode::SHA1:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		IStackItem::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		byte *hash = new byte[Crypto::SHA1_LENGTH];
		Crypto::ComputeSHA1(data, size, hash);
		delete[]data;

		this->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::SHA1_LENGTH, true));
		return;
	}
	case EVMOpCode::SHA256:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		IStackItem::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		byte *hash = new byte[Crypto::SHA256_LENGTH];
		Crypto::ComputeSHA256(data, size, hash);
		delete[]data;

		this->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::SHA256_LENGTH, true));
		return;
	}
	case EVMOpCode::HASH160:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		IStackItem::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		byte *hash = new byte[Crypto::HASH160_LENGTH];
		Crypto::ComputeHash160(data, size, hash);
		delete[]data;

		this->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::HASH160_LENGTH, true));
		return;
	}
	case EVMOpCode::HASH256:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();

		if (size < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		byte* data = new byte[size];
		size = item->ReadByteArray(data, 0, size);
		IStackItem::Free(item);

		if (size < 0)
		{
			delete[]data;
			this->State = EVMState::FAULT;
			return;
		}

		byte *hash = new byte[Crypto::HASH256_LENGTH];
		Crypto::ComputeHash256(data, size, hash);
		delete[]data;

		this->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::HASH256_LENGTH, true));
		return;
	}
	case EVMOpCode::CHECKSIG:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ipubKey = this->EvaluationStack->Pop();
		IStackItem *isignature = this->EvaluationStack->Pop();

		int pubKeySize = ipubKey->ReadByteArraySize();
		int signatureSize = isignature->ReadByteArraySize();

		if (MessageCallback == NULL || pubKeySize < 33 || signatureSize < 32)
		{
			IStackItem::Free(ipubKey);
			IStackItem::Free(isignature);

			this->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		// Read message

		// TODO: dangerous way to get the message

		byte* msg;
		int32 msgL = MessageCallback(this->Iteration, msg);
		if (msgL <= 0)
		{
			IStackItem::Free(ipubKey);
			IStackItem::Free(isignature);

			this->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		// Read public Key

		byte * pubKey = new byte[pubKeySize];
		pubKeySize = ipubKey->ReadByteArray(pubKey, 0, pubKeySize);

		// Read signature

		byte * signature = new byte[signatureSize];
		signatureSize = isignature->ReadByteArray(signature, 0, signatureSize);

		bool ret = Crypto::VerifySignature(msg, msgL, signature, signatureSize, pubKey, pubKeySize);

		delete[](pubKey);
		delete[](signature);

		IStackItem::Free(ipubKey);
		IStackItem::Free(isignature);

		this->EvaluationStack->Push(new BoolStackItem(ret));
		return;
	}
	case EVMOpCode::CHECKMULTISIG:
	{
		/*
		int32 n;
		byte[][] pubkeys;
		StackItem item = EvaluationStack.Pop();
		if (item is VMArray array1)
		{
			pubkeys = array1.Select(p = > p.GetByteArray()).ToArray();
			n = pubkeys.Length;
			if (n == 0)
			{
				State |= VMState.FAULT;
				return;
			}
		}
		else
		{
			n = (int)item.GetBigInteger();
			if (n < 1 || n > EvaluationStack.Count)
			{
				State |= VMState.FAULT;
				return;
			}
			pubkeys = new byte[n][];
			for (int32 i = 0; i < n; i++)
				pubkeys[i] = EvaluationStack.Pop().GetByteArray();
		}
		int32 m;
		byte[][] signatures;
		item = EvaluationStack.Pop();
		if (item is VMArray array2)
		{
			signatures = array2.Select(p = > p.GetByteArray()).ToArray();
			m = signatures.Length;
			if (m == 0 || m > n)
			{
				State |= VMState.FAULT;
				return;
			}
		}
		else
		{
			m = (int)item.GetBigInteger();
			if (m < 1 || m > n || m > EvaluationStack.Count)
			{
				State |= VMState.FAULT;
				return;
			}
			signatures = new byte[m][];
			for (int32 i = 0; i < m; i++)
				signatures[i] = EvaluationStack.Pop().GetByteArray();
		}
		byte[] message = ScriptContainer.GetMessage();
		bool fSuccess = true;
		try
		{
			for (int32 i = 0, j = 0; fSuccess && i < m && j < n;)
			{
				if (Crypto.VerifySignature(message, signatures[i], pubkeys[j]))
					i++;
				j++;
				if (m - i > n - j)
					fSuccess = false;
			}
		}
		catch (ArgumentException)
		{
			fSuccess = false;
		}
		EvaluationStack.Push(fSuccess);
		*/
		return;
	}

#pragma endregion

#pragma region Array

	case EVMOpCode::ARRAYSIZE:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 size;
		IStackItem *item = this->EvaluationStack->Pop();

		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *ar = (ArrayStackItem*)item;
			size = ar->Count();
			break;
		}
		case EStackItemType::Map:
		{
			MapStackItem *ar = (MapStackItem*)item;
			size = ar->Count();
			break;
		}
		default:
		{
			size = item->ReadByteArraySize();
			break;
		}
		}

		IStackItem::Free(item);

		if (size < 0)
		{
			this->State = EVMState::FAULT;
		}
		else
		{
			this->EvaluationStack->Push(new IntegerStackItem(size));
		}

		return;
	}
	case EVMOpCode::PACK:
	{
		int32 ec = this->EvaluationStack->Count();
		if (ec < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();

		int32 size = 0;
		if (!item->GetInt32(size))
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}
		IStackItem::Free(item);

		if (size < 0 || size >(ec - 1))
		{
			this->State = EVMState::FAULT;
			return;
		}

		ArrayStackItem *items = new ArrayStackItem(false);

		for (int32 i = 0; i < size; i++)
		{
			items->Add(this->EvaluationStack->Pop());
		}

		this->EvaluationStack->Push(items);
		return;
	}
	case EVMOpCode::UNPACK:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();

		if (item->Type == EStackItemType::Array)
		{
			ArrayStackItem *array = (ArrayStackItem*)item;
			int32 count = array->Count();

			for (int32 i = count - 1; i >= 0; i--)
			{
				IStackItem * v = array->Get(i);
				this->EvaluationStack->Push(v);
				array->RemoveAt(i, false);
			}

			IStackItem::Free(item);

			IntegerStackItem *itcount = new IntegerStackItem(count);
			this->EvaluationStack->Push(itcount);
		}
		else
		{
			IStackItem::Free(item);

			this->State = EVMState::FAULT;
			return;
		}
		return;
	}
	case EVMOpCode::PICKITEM:
	{
		if (this->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *key = this->EvaluationStack->Pop();
		if (key->Type == EStackItemType::Map || key->Type == EStackItemType::Array)
		{
			IStackItem::Free(key);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				IStackItem::Free(key);

				this->State = EVMState::FAULT;
				return;
			}
			IStackItem::Free(key);

			IStackItem *ret = arr->Get(index);

			arr->Set(index, NULL, false);

			this->EvaluationStack->Push(ret);
			IStackItem::Free(item);
			return;
		}
		case EStackItemType::Map:
		{
			IStackItem::Free(key);
			IStackItem::Free(item);

			/*
			if (map.TryGetValue(key, out StackItem value))
			{
				EvaluationStack.Push(value);
			}
			else
			}*/
			// {
			this->State = EVMState::FAULT;
			return;
			// }
		}
		default:
		{
			IStackItem::Free(key);
			IStackItem::Free(item);

			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::SETITEM:
	{
		if (this->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *value = this->EvaluationStack->Pop();
		if (value->Type == EStackItemType::Struct)
		{
			IStackItem * clone = value->Clone();
			IStackItem::Free(value);
			value = clone;
		}
		IStackItem *key = this->EvaluationStack->Pop();

		if (key->Type == EStackItemType::Map || key->Type == EStackItemType::Array)
		{
			IStackItem::Free(key);
			IStackItem::Free(value);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = this->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				IStackItem::Free(key);
				IStackItem::Free(value);

				this->State = EVMState::FAULT;
				return;
			}
			arr->Set(index, value, true);

			IStackItem::Free(key);
			IStackItem::Free(item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;
			arr->Set(key, value);

			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(key);
			IStackItem::Free(value);
			IStackItem::Free(item);

			this->State = EVMState::FAULT;
			return;
		}
		}
	}
	case EVMOpCode::NEWARRAY:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem *item = this->EvaluationStack->Pop();

		if (!item->GetInt32(count) || count < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(item);
		this->EvaluationStack->Push(new ArrayStackItem(false, count));
		return;
	}
	case EVMOpCode::NEWSTRUCT:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem *item = this->EvaluationStack->Pop();

		if (!item->GetInt32(count) || count < 0)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(item);
		this->EvaluationStack->Push(new ArrayStackItem(true, count));
		return;
	}
	case EVMOpCode::NEWMAP:
	{
		this->EvaluationStack->Push(new MapStackItem());
		return;
	}
	case EVMOpCode::APPEND:
	{
		/*
		StackItem newItem = EvaluationStack.Pop();
	if (newItem is Types.Struct s)
	{
	newItem = s.Clone();
	}
	StackItem arrItem = EvaluationStack.Pop();
	if (arrItem is VMArray array)
	{
	array.Add(newItem);
	}
	else
	{
	State |= VMState.FAULT;
	return;
	}
	*/
		return;
	}
	case EVMOpCode::REVERSE:
	{
		/*
	StackItem arrItem = EvaluationStack.Pop();
	if (arrItem is VMArray array)
	{
	array.Reverse();
	}
	else
	{
	State |= VMState.FAULT;
	return;
	}
	*/
		return;
	}
	case EVMOpCode::REMOVE:
	{
		/*
		StackItem key = EvaluationStack.Pop();
		if (key is ICollection)
		{
			State |= VMState.FAULT;
			return;
		}
		switch (EvaluationStack.Pop())
		{
		case VMArray array:
			int32 index = (int)key.GetBigInteger();
			if (index < 0 || index >= array.Count)
			{
				State |= VMState.FAULT;
				return;
			}
			array.RemoveAt(index);
			return;
		case Map map :
			map.Remove(key);
			return;
		default:
			State |= VMState.FAULT;
			return;
		}
		*/
		return;
	}
	case EVMOpCode::HASKEY:
	{
		/*
		StackItem key = EvaluationStack.Pop();
		if (key is ICollection)
		{
			State |= VMState.FAULT;
			return;
		}
		switch (EvaluationStack.Pop())
		{
		case VMArray array:
			int32 index = (int)key.GetBigInteger();
			if (index < 0)
			{
				State |= VMState.FAULT;
				return;
			}
			EvaluationStack.Push(index < array.Count);
			return;
		case Map map :
			EvaluationStack.Push(map.ContainsKey(key));
			return;
		default:
			State |= VMState.FAULT;
			return;
		}
		*/
		return;
	}
	case EVMOpCode::KEYS:
	{
		/*
		switch (EvaluationStack.Pop())
		{
		case Map map :
			EvaluationStack.Push(new VMArray(map.Keys));
			return;
		default:
			State |= VMState.FAULT;
			return;
		}
		*/
		return;
	}
	case EVMOpCode::VALUES:
	{
		/*
		ICollection<StackItem> values;
		switch (EvaluationStack.Pop())
		{
		case VMArray array:
			values = array;
			return;
		case Map map :
			values = map.Values;
			return;
		default:
			State |= VMState.FAULT;
			return;
		}
		List<StackItem> newArray = new List<StackItem>(values.Count);
		foreach(StackItem item in values)
			if (item is Struct s)
				newArray.Add(s.Clone());
			else
				newArray.Add(item);
		EvaluationStack.Push(new VMArray(newArray));
		*/
		return;
	}

#pragma endregion

#pragma region Exceptions

	default:
	case EVMOpCode::THROW:
	{
		this->State = EVMState::FAULT;
		return;
	}
	case EVMOpCode::THROWIFNOT:
	{
		if (this->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = this->EvaluationStack->Pop();

		if (!item->GetBoolean())
		{
			this->State = EVMState::FAULT;
		}

		IStackItem::Free(item);
		return;
	}

#pragma endregion

	}
}

// Destructor

ExecutionEngine::~ExecutionEngine()
{
	// Clean callbacks

	this->ScriptCallback = NULL;
	this->InteropCallback = NULL;
	this->MessageCallback = NULL;

	// Clean pointers

	delete(this->InvocationStack);
	delete(this->EvaluationStack);
	delete(this->AltStack);
}