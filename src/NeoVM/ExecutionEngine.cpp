#include "ExecutionEngine.h"
#include "ExecutionScript.h"
#include "ByteArrayStackItem.h"
#include "IntegerStackItem.h"
#include "BoolStackItem.h"
#include "MapStackItem.h"
#include "ArrayStackItem.h"
#include "Crypto.h"

// Getters

ExecutionContextStack* ExecutionEngine::GetInvocationStack() { return this->InvocationStack; }
StackItems* ExecutionEngine::GetResultStack() { return this->ResultStack; }

// Setters

void ExecutionEngine::SetLogCallback(OnStepIntoCallback logCallback) { this->Log = logCallback; }

void ExecutionEngine::Clean(uint32 iteration)
{
	this->Iteration = iteration;
	this->State = EVMState::NONE;

	this->InvocationStack->Clear();
	this->ResultStack->Clear();
}

// Constructor

ExecutionEngine::ExecutionEngine
(
	InvokeInteropCallback invokeInterop, LoadScriptCallback loadScript, GetMessageCallback getMessage
) :
	Iteration(0),
	State(EVMState::NONE),
	Log(NULL),

	OnGetMessage(getMessage),
	OnLoadScript(loadScript),
	OnInvokeInterop(invokeInterop),

	ResultStack(new StackItems()),
	InvocationStack(new ExecutionContextStack())
{ }

ExecutionContext* ExecutionEngine::LoadScript(ExecutionScript * script, int32 rvcount)
{
	ExecutionContext* context = new ExecutionContext(script, 0, rvcount);
	this->InvocationStack->Push(context);
	return context;
}

bool ExecutionEngine::LoadScript(byte scriptIndex, int32 rvcount)
{
	ExecutionScript * sc = NULL;

	if (this->Scripts.size() > scriptIndex)
	{
		std::list<ExecutionScript*>::iterator it = this->Scripts.begin();
		std::advance(it, scriptIndex);

		sc = (ExecutionScript*)*it;
	}

	if (sc == NULL) return false;

	ExecutionContext * context = new ExecutionContext(sc, 0, rvcount);
	this->InvocationStack->Push(context);
	return true;
}

int32 ExecutionEngine::LoadScript(byte * script, int32 scriptLength, int32 rvcount)
{
	int32 index = Scripts.size();

	ExecutionScript * sc = new ExecutionScript(script, scriptLength);
	sc->Claim();

	Scripts.push_back(sc);

	ExecutionContext * context = new ExecutionContext(sc, 0, rvcount);
	this->InvocationStack->Push(context);
	return index;
}

ExecutionContext* ExecutionEngine::GetCurrentContext()
{
	return this->InvocationStack->TryPeek(0);
}

ExecutionContext* ExecutionEngine::GetCallingContext()
{
	return this->InvocationStack->TryPeek(1);
}

ExecutionContext* ExecutionEngine::GetEntryContext()
{
	return this->InvocationStack->TryPeek(this->InvocationStack->Count() - 1);
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

	if (this->Log != NULL)
	{
		this->Log(context);
	}

	EVMOpCode opcode = context->ReadNextInstruction();

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

		context->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}

	// Check Push

	if (opcode >= EVMOpCode::PUSH1 && opcode <= EVMOpCode::PUSH16)
	{
		context->EvaluationStack->Push(new IntegerStackItem((opcode - EVMOpCode::PUSH1) + 1));
		return;
	}

	// Execute opcode

	switch (opcode)
	{
		// Push value

	case EVMOpCode::PUSH0:
	{
		context->EvaluationStack->Push(new ByteArrayStackItem(NULL, 0, false));
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

		context->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
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

		context->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}
	case EVMOpCode::PUSHDATA4:
	{
		int32 length = 0;
		if (!context->ReadInt32(length) || length < 0 || length > MAX_ITEM_LENGTH)
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

		context->EvaluationStack->Push(new ByteArrayStackItem(data, length, true));
		return;
	}
	case EVMOpCode::PUSHM1:
	{
		BigInteger* bi = new BigInteger(BigInteger::MinusOne);
		context->EvaluationStack->Push(new IntegerStackItem(bi));
		return;
	}

	// Control

	case EVMOpCode::NOP: return;
	case EVMOpCode::JMP:
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

		context->InstructionPointer = offset;
		return;
	}
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

		if (offset < 0 || offset > context->ScriptLength || context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *bitem = context->EvaluationStack->Pop();

		if (opcode == EVMOpCode::JMPIFNOT)
		{
			if (!bitem->GetBoolean())
				context->InstructionPointer = offset;
		}
		else
		{
			if (bitem->GetBoolean())
				context->InstructionPointer = offset;
		}

		IStackItem::Free(bitem);
		return;
	}
	case EVMOpCode::CALL:
	{
		if (context == NULL || this->InvocationStack->Count() >= MAX_INVOCATION_STACK_SIZE)
		{
			this->State = EVMState::FAULT;
			return;
		}

		ExecutionContext *clone = this->LoadScript(context->Script, -1);
		context->EvaluationStack->CopyTo(clone->EvaluationStack, -1);
		clone->InstructionPointer = context->InstructionPointer;
		context->InstructionPointer += 2;
		context->EvaluationStack->Clear();

		// Jump

		opcode = EVMOpCode::JMP;
		context = clone;
		goto ExecuteOpCode;
	}
	// Stack isolation (NEP8)
	case EVMOpCode::CALL_I:
	{
		if (context == NULL || this->InvocationStack->Count() >= MAX_INVOCATION_STACK_SIZE)
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte rvcount = 0;
		if (!context->ReadUInt8(rvcount))
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte pcount = 0;
		if (!context->ReadUInt8(pcount))
		{
			this->State = EVMState::FAULT;
			return;
		}

		if (context->EvaluationStack->Count() < pcount)
		{
			this->State = EVMState::FAULT;
			return;
		}

		ExecutionContext *clone = this->LoadScript(context->Script, rvcount);
		context->EvaluationStack->CopyTo(clone->EvaluationStack, pcount);
		clone->InstructionPointer = context->InstructionPointer;
		context->InstructionPointer += 2;

		for (int i = 0; i < pcount; i++)
			context->EvaluationStack->Drop();

		// Jump

		opcode = EVMOpCode::JMP;
		context = clone;
		goto ExecuteOpCode;
	}
	case EVMOpCode::CALL_E:
	case EVMOpCode::CALL_ED:
	case EVMOpCode::CALL_ET:
	case EVMOpCode::CALL_EDT:
	{
		if (this->OnLoadScript == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte rvcount = 0;
		if (!context->ReadUInt8(rvcount))
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte pcount = 0;
		if (!context->ReadUInt8(pcount))
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 ec = context->EvaluationStack->Count();
		if (ec < pcount)
		{
			this->State = EVMState::FAULT;
			return;
		}

		if (opcode == EVMOpCode::CALL_ET || opcode == EVMOpCode::CALL_EDT)
		{
			if (context->RVCount != rvcount)
			{
				this->State = EVMState::FAULT;
				return;
			}
		}

		const int32 scriptLength = 20;
		byte script_hash[scriptLength];

		bool isDynamicInvoke = opcode == EVMOpCode::CALL_ED || opcode == EVMOpCode::CALL_EDT;
		bool search = true;

		if (isDynamicInvoke)
		{
			// Require one element more

			if (ec < pcount + 1)
			{
				this->State = EVMState::FAULT;
				return;
			}

			// Get hash from the evaluation stack

			IStackItem *it = context->EvaluationStack->Pop();
			int32 size = it->ReadByteArraySize();

			if (size != scriptLength || it->ReadByteArray(script_hash, 0, scriptLength) != scriptLength)
			{
				this->State = EVMState::FAULT;
				IStackItem::UnclaimAndFree(it);
				return;
			}

			IStackItem::UnclaimAndFree(it);
		}
		else
		{
			if (context->Read(script_hash, scriptLength) != scriptLength)
			{
				this->State = EVMState::FAULT;
				return;
			}

			// try to find in cache when is not dynamic call

			for (std::list<ExecutionScript*>::iterator it = this->Scripts.begin(); it != this->Scripts.end(); ++it)
			{
				ExecutionScript* ptr = (ExecutionScript*)*it;
				if (ptr->IsTheSameHash(script_hash, scriptLength))
				{
					this->LoadScript(ptr, rvcount);
					search = false;
					break;
				}
			}
		}

		if (search && this->OnLoadScript(script_hash, isDynamicInvoke ? 0x01 : 0x00, rvcount) != 0x01)
		{
			this->State = EVMState::FAULT;
			return;
		}

		ExecutionContext *context_new = this->InvocationStack->Peek(0);
		context->EvaluationStack->CopyTo(context_new->EvaluationStack, pcount);

		if (opcode == EVMOpCode::CALL_ET || opcode == EVMOpCode::CALL_EDT)
		{
			this->InvocationStack->Remove(1);
		}
		else
		{
			for (int i = 0; i < pcount; i++)
				context->EvaluationStack->Drop();
		}

		return;
	}
	case EVMOpCode::RET:
	{
		if (context != NULL)
		{
			context->Claim();
			this->InvocationStack->Drop();

			int32 rvcount = context->RVCount;

			if (rvcount == -1)
			{
				rvcount = context->EvaluationStack->Count();
			}
			else
			{
				if (rvcount > 0 && context->EvaluationStack->Count() < rvcount)
				{
					ExecutionContext::UnclaimAndFree(context);
					this->State = EVMState::FAULT;
					return;
				}
			}

			if (rvcount > 0)
			{
				if (this->InvocationStack->Count() == 0)
					context->EvaluationStack->CopyTo(this->ResultStack, rvcount);
				else
					context->EvaluationStack->CopyTo(this->GetCurrentContext()->EvaluationStack, rvcount);
			}

			ExecutionContext::UnclaimAndFree(context);
		}

		if (this->InvocationStack->Count() == 0)
			this->State = EVMState::HALT;

		return;
	}
	case EVMOpCode::APPCALL:
	case EVMOpCode::TAILCALL:
	{
		if (this->OnLoadScript == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		const int32 scriptLength = 20;
		byte script_hash[scriptLength];
		if (context->Read(script_hash, scriptLength) != scriptLength)
		{
			this->State = EVMState::FAULT;
			return;
		}

		bool isDynamicInvoke = true;
		for (int32 x = 0; x < scriptLength; ++x)
			if (script_hash[x] != 0x00) { isDynamicInvoke = false; break; }

		bool search = true;
		if (isDynamicInvoke)
		{
			if (context->EvaluationStack->Count() < 1)
			{
				this->State = EVMState::FAULT;
				return;
			}

			IStackItem* item = context->EvaluationStack->Pop();
			if (item->ReadByteArray(&script_hash[0], 0, scriptLength) != scriptLength)
			{
				IStackItem::Free(item);
				this->State = EVMState::FAULT;
				return;
			}

			IStackItem::Free(item);
		}
		else
		{
			// try to find in cache when is not dynamic call

			for (std::list<ExecutionScript*>::iterator it = this->Scripts.begin(); it != this->Scripts.end(); ++it)
			{
				ExecutionScript* ptr = (ExecutionScript*)*it;
				if (ptr->IsTheSameHash(script_hash, scriptLength))
				{
					this->LoadScript(ptr, -1);
					search = false;
					break;
				}
			}
		}

		if (search && this->OnLoadScript(script_hash, isDynamicInvoke ? 0x01 : 0x00, -1) != 0x01)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->EvaluationStack->CopyTo(this->GetCurrentContext()->EvaluationStack, -1);

		if (this->InvocationStack->Count() >= MAX_INVOCATION_STACK_SIZE)
		{
			this->State = EVMState::FAULT;
			return;
		}

		if (opcode == EVMOpCode::TAILCALL)
		{
			this->InvocationStack->Remove(1);
		}
		else
		{
			context->EvaluationStack->Clear();
		}

		return;
	}
	case EVMOpCode::SYSCALL:
	{
		uint32 length = 0;
		if (this->OnInvokeInterop == NULL || !context->ReadVarBytes(length, 252) || length == 0)
		{
			this->State = EVMState::FAULT;
			return;
		}

		byte *data = new byte[length + 1];
		if (context->Read((byte*)data, length) != (int32)length)
		{
			delete[](data);
			this->State = EVMState::FAULT;
			return;
		}

		data[length] = 0x00;

		if (this->OnInvokeInterop(data, (byte)length) != 0x01)
			this->State = EVMState::FAULT;

		delete[](data);
		return;
	}

	// Stack ops

	case EVMOpCode::DUPFROMALTSTACK:
	{
		if (context->AltStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->EvaluationStack->Push(context->AltStack->Peek(0));
		return;
	}
	case EVMOpCode::TOALTSTACK:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->AltStack->Push(context->EvaluationStack->Pop());
		return;
	}
	case EVMOpCode::FROMALTSTACK:
	{
		if (context->AltStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->EvaluationStack->Push(context->AltStack->Pop());
		return;
	}
	case EVMOpCode::XDROP:
	{
		int32 ic = context->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * it = context->EvaluationStack->Pop();

		int32 n = 0;
		if (!it->GetInt32(n) || n < 0 || n >= ic - 1)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = context->EvaluationStack->Remove(n);
		IStackItem::Free(it);
		return;
	}
	case EVMOpCode::XSWAP:
	{
		int32 ic = context->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 n = 0;
		IStackItem * it = context->EvaluationStack->Pop();

		if (!it->GetInt32(n) || n < 0 || n >= ic - 1)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		if (n == 0) return;

		IStackItem* xn = context->EvaluationStack->Peek(n);

		it = context->EvaluationStack->Remove(0);
		context->EvaluationStack->Insert(0, xn);
		context->EvaluationStack->Remove(n);
		context->EvaluationStack->Insert(n, it);
		return;
	}
	case EVMOpCode::XTUCK:
	{
		int32 ic = context->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 n = 0;
		IStackItem * it = context->EvaluationStack->Pop();

		if (!it->GetInt32(n) || n <= 0 || n > ic - 1)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		context->EvaluationStack->Insert(n, context->EvaluationStack->Peek(0));
		return;
	}
	case EVMOpCode::DEPTH:
	{
		context->EvaluationStack->Push(new IntegerStackItem(context->EvaluationStack->Count()));
		return;
	}
	case EVMOpCode::DROP:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *it = context->EvaluationStack->Pop();
		IStackItem::Free(it);
		return;
	}
	case EVMOpCode::DUP:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->EvaluationStack->Push(context->EvaluationStack->Peek(0));
		return;
	}
	case EVMOpCode::NIP:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x1 = context->EvaluationStack->Remove(1);
		IStackItem::Free(x1);
		return;
	}
	case EVMOpCode::OVER:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x1 = context->EvaluationStack->Peek(1);
		context->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::PICK:
	{
		int32 ic = context->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 n = 0;
		IStackItem * it = context->EvaluationStack->Pop();

		if (!it->GetInt32(n) || n < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		if (n >= ic - 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->EvaluationStack->Push(context->EvaluationStack->Peek(n));
		return;
	}
	case EVMOpCode::ROLL:
	{
		int32 ic = context->EvaluationStack->Count();
		if (ic < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 n = 0;
		IStackItem * it = context->EvaluationStack->Pop();

		if (!it->GetInt32(n) || n < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);

		if (n >= ic - 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		if (n == 0) return;

		context->EvaluationStack->Push(context->EvaluationStack->Remove(n));
		return;
	}
	case EVMOpCode::ROT:
	{
		if (context->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x1 = context->EvaluationStack->Remove(2);
		context->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::SWAP:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x1 = context->EvaluationStack->Remove(1);
		context->EvaluationStack->Push(x1);
		return;
	}
	case EVMOpCode::TUCK:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *x1 = context->EvaluationStack->Peek(0);
		context->EvaluationStack->Insert(2, x1);
		return;
	}
	case EVMOpCode::CAT:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * x2 = context->EvaluationStack->Pop();
		IStackItem * x1 = context->EvaluationStack->Pop();
		int32 size2 = x2->ReadByteArraySize();
		int32 size1 = x1->ReadByteArraySize();

		if (size2 < 0 || size1 < 0 || size1 + size2 > MAX_ITEM_LENGTH)
		{
			IStackItem::Free(x2, x1);

			this->State = EVMState::FAULT;
			return;
		}

		byte * data = new byte[size2 + size1];
		x1->ReadByteArray(&data[0], 0, size1);
		x2->ReadByteArray(&data[size1], 0, size2);

		IStackItem::Free(x2, x1);

		context->EvaluationStack->Push(new ByteArrayStackItem(data, size1 + size2, true));
		return;
	}
	case EVMOpCode::SUBSTR:
	{
		if (context->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem * it = context->EvaluationStack->Pop();

		if (!it->GetInt32(count) || count < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = context->EvaluationStack->Pop();
		int32 index = 0;

		if (!it->GetInt32(index) || index < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = context->EvaluationStack->Pop();

		byte * data = new byte[count];
		if (it->ReadByteArray(&data[0], index, count) != count)
		{
			delete[]data;
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		context->EvaluationStack->Push(new ByteArrayStackItem(data, count, true));
		return;
	}
	case EVMOpCode::LEFT:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem * it = context->EvaluationStack->Pop();

		if (!it->GetInt32(count) || count < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = context->EvaluationStack->Pop();

		byte * data = new byte[count];
		if (it->ReadByteArray(&data[0], 0, count) != count)
		{
			delete[]data;
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		context->EvaluationStack->Push(new ByteArrayStackItem(data, count, true));
		return;
	}
	case EVMOpCode::RIGHT:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem * it = context->EvaluationStack->Pop();

		if (!it->GetInt32(count) || count < 0)
		{
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		it = context->EvaluationStack->Pop();

		byte * data = new byte[count];
		if (it->ReadByteArray(&data[0], it->ReadByteArraySize() - count, count) != count)
		{
			delete[]data;
			IStackItem::Free(it);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(it);
		context->EvaluationStack->Push(new ByteArrayStackItem(data, count, true));
		return;
	}
	case EVMOpCode::SIZE:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem * item = context->EvaluationStack->Pop();
		int32 size = item->ReadByteArraySize();
		IStackItem::Free(item);

		if (size < 0)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->EvaluationStack->Push(new IntegerStackItem(size));
		return;
	}

	// Bitwise logic

	case EVMOpCode::INVERT:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = context->EvaluationStack->Pop();
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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::AND:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::OR:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::XOR:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::EQUAL:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		context->EvaluationStack->Push(new BoolStackItem(x1->Equals(x2)));

		IStackItem::Free(x2, x1);
		return;
	}

	// Numeric

	case EVMOpCode::INC:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = context->EvaluationStack->Pop();
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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::DEC:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = context->EvaluationStack->Pop();
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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::SIGN:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = context->EvaluationStack->Pop();
		BigInteger* bi = item->GetBigInteger();
		IStackItem::Free(item);

		if (bi == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int ret = bi->GetSign();
		delete(bi);

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::NEGATE:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = context->EvaluationStack->Pop();
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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::ABS:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = context->EvaluationStack->Pop();
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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::NOT:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = context->EvaluationStack->Pop();
		BoolStackItem *bnew = new BoolStackItem(!item->GetBoolean());
		IStackItem::Free(item);

		context->EvaluationStack->Push(bnew);
		return;
	}
	case EVMOpCode::NZ:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x = context->EvaluationStack->Pop();
		BigInteger *i = x->GetBigInteger();
		IStackItem::Free(x);

		if (i == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *ret = new BoolStackItem(i->CompareTo(BigInteger::Zero) != 0);
		delete(i);

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::ADD:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = context->EvaluationStack->Pop();
		IStackItem* i1 = context->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2, i1);

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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::SUB:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = context->EvaluationStack->Pop();
		IStackItem* i1 = context->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2, i1);

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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::MUL:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = context->EvaluationStack->Pop();
		IStackItem* i1 = context->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2, i1);

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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::DIV:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = context->EvaluationStack->Pop();
		IStackItem* i1 = context->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2, i1);

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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::MOD:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* i2 = context->EvaluationStack->Pop();
		IStackItem* i1 = context->EvaluationStack->Pop();
		BigInteger* x2 = i2->GetBigInteger();
		BigInteger* x1 = i1->GetBigInteger();
		IStackItem::Free(i2, i1);

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

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::SHL:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* n = context->EvaluationStack->Pop();
		IStackItem* x = context->EvaluationStack->Pop();

		BigInteger *in = n->GetBigInteger();
		BigInteger *ix = x->GetBigInteger();

		IStackItem::Free(n, x);

		int32 ishift;
		if (in == NULL || ix == NULL || !in->ToInt32(ishift) || (ishift > MAX_SHL_SHR || ishift < MIN_SHL_SHR))
		{
			if (ix != NULL) delete(ix);
			if (in != NULL) delete(in);

			this->State = EVMState::FAULT;
			return;
		}

		delete(in);
		BigInteger *ret = ix->Shl(ishift);
		delete(ix);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::SHR:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* n = context->EvaluationStack->Pop();
		IStackItem* x = context->EvaluationStack->Pop();

		BigInteger *in = n->GetBigInteger();
		BigInteger *ix = x->GetBigInteger();

		IStackItem::Free(n, x);

		int32 ishift;
		if (in == NULL || ix == NULL || !in->ToInt32(ishift) || (ishift > MAX_SHL_SHR || ishift < MIN_SHL_SHR))
		{
			if (ix != NULL) delete(ix);
			if (in != NULL) delete(in);

			this->State = EVMState::FAULT;
			return;
		}

		delete(in);
		BigInteger *ret = ix->Shr(ishift);
		delete(ix);

		if (ret == NULL)
		{
			this->State = EVMState::FAULT;
			return;
		}

		context->EvaluationStack->Push(new IntegerStackItem(ret));
		return;
	}
	case EVMOpCode::BOOLAND:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BoolStackItem * ret = new BoolStackItem(x1->GetBoolean() && x2->GetBoolean());
		IStackItem::Free(x2, x1);

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::BOOLOR:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BoolStackItem * ret = new BoolStackItem(x1->GetBoolean() || x2->GetBoolean());
		IStackItem::Free(x2, x1);

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::NUMEQUAL:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::NUMNOTEQUAL:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::LT:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::GT:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::LTE:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::GTE:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::MIN:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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
			ret = new IntegerStackItem(i2);
		}
		else
		{
			delete(i2);
			ret = new IntegerStackItem(i1);
		}

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::MAX:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* x2 = context->EvaluationStack->Pop();
		IStackItem* x1 = context->EvaluationStack->Pop();

		BigInteger *i2 = x2->GetBigInteger();
		BigInteger *i1 = x1->GetBigInteger();

		IStackItem::Free(x1, x2);

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
			ret = new IntegerStackItem(i1);
		}
		else
		{
			delete(i1);
			ret = new IntegerStackItem(i2);
		}

		context->EvaluationStack->Push(ret);
		return;
	}
	case EVMOpCode::WITHIN:
	{
		if (context->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* b = context->EvaluationStack->Pop();
		IStackItem* a = context->EvaluationStack->Pop();
		IStackItem* x = context->EvaluationStack->Pop();

		BigInteger *ib = b->GetBigInteger();
		BigInteger *ia = a->GetBigInteger();
		BigInteger *ix = x->GetBigInteger();

		IStackItem::Free(b, a, x);

		if (ib == NULL || ia == NULL || ix == NULL)
		{
			if (ib != NULL) delete(ib);
			if (ia != NULL) delete(ia);
			if (ix != NULL) delete(ix);

			this->State = EVMState::FAULT;
			return;
		}

		context->EvaluationStack->Push(new BoolStackItem(ia->CompareTo(ix) <= 0 && ix->CompareTo(ib) < 0));

		delete(ib);
		delete(ia);
		delete(ix);
		return;
	}

	// Crypto

	case EVMOpCode::SHA1:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
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

		context->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::SHA1_LENGTH, true));
		return;
	}
	case EVMOpCode::SHA256:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
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

		context->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::SHA256_LENGTH, true));
		return;
	}
	case EVMOpCode::HASH160:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
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

		context->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::HASH160_LENGTH, true));
		return;
	}
	case EVMOpCode::HASH256:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
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

		context->EvaluationStack->Push(new ByteArrayStackItem(hash, Crypto::HASH256_LENGTH, true));
		return;
	}
	case EVMOpCode::CHECKSIG:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* ipubKey = context->EvaluationStack->Pop();
		IStackItem* isignature = context->EvaluationStack->Pop();

		int pubKeySize = ipubKey->ReadByteArraySize();
		int signatureSize = isignature->ReadByteArraySize();

		if (this->OnGetMessage == NULL || pubKeySize < 33 || signatureSize < 32)
		{
			IStackItem::Free(ipubKey, isignature);
			context->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		// Read message

		// TODO: dangerous way to get the message

		byte* msg;
		int32 msgL = this->OnGetMessage(this->Iteration, msg);
		if (msgL <= 0)
		{
			IStackItem::Free(ipubKey, isignature);
			context->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		// Read public Key

		byte * pubKey = new byte[pubKeySize];
		pubKeySize = ipubKey->ReadByteArray(pubKey, 0, pubKeySize);

		// Read signature

		byte * signature = new byte[signatureSize];
		signatureSize = isignature->ReadByteArray(signature, 0, signatureSize);

		int16 ret = Crypto::VerifySignature(msg, msgL, signature, signatureSize, pubKey, pubKeySize);

		delete[](pubKey);
		delete[](signature);

		IStackItem::Free(ipubKey, isignature);

		context->EvaluationStack->Push(new BoolStackItem(ret == 0x01));
		return;
	}
	case EVMOpCode::VERIFY:
	{
		if (context->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* ipubKey = context->EvaluationStack->Pop();
		IStackItem* isignature = context->EvaluationStack->Pop();
		IStackItem* imsg = context->EvaluationStack->Pop();

		int pubKeySize = ipubKey->ReadByteArraySize();
		int signatureSize = isignature->ReadByteArraySize();
		int msgSize = imsg->ReadByteArraySize();

		if (pubKeySize < 33 || signatureSize < 32 || msgSize < 0)
		{
			IStackItem::Free(ipubKey, isignature, imsg);
			this->State = EVMState::FAULT;
			return;
		}

		// Read message

		byte * msg = new byte[msgSize];
		msgSize = imsg->ReadByteArray(msg, 0, msgSize);

		// Read public Key

		byte * pubKey = new byte[pubKeySize];
		pubKeySize = ipubKey->ReadByteArray(pubKey, 0, pubKeySize);

		// Read signature

		byte * signature = new byte[signatureSize];
		signatureSize = isignature->ReadByteArray(signature, 0, signatureSize);

		int16 ret = Crypto::VerifySignature(msg, msgSize, signature, signatureSize, pubKey, pubKeySize);

		delete[](msg);
		delete[](pubKey);
		delete[](signature);

		IStackItem::Free(imsg, ipubKey, isignature);

		context->EvaluationStack->Push(new BoolStackItem(ret == 0x01));
		return;
	}
	case EVMOpCode::CHECKMULTISIG:
	{
		int ic = context->EvaluationStack->Count();

		if (ic < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		/*
		byte ** pubKeys;
		byte ** signatures;
		int * pubKeysL;
		int * signaturesL;
		int pubKeysCount, signaturesCount;

		for (byte x = 0; x < 2; ++x)
		{
			IStackItem* item = context->EvaluationStack->Pop();
			ic--;

			if (item->Type == EStackItemType::Array || item->Type == EStackItemType::Struct)
			{
				ArrayStackItem* arr = (ArrayStackItem*)item;
				int32 v = arr->Count();

				if (v <= 0)
				{
					this->State = EVMState::FAULT;
				}
				else
				{
					byte **data = new byte*[v];
					int32 *dataL = new int32[v];

					for (int32 i = 0; i < v; ++i)
					{
						IStackItem* ret = arr->Get(i);

						int32 c = ret->ReadByteArraySize();
						if (c < 0)
						{
							data[i] = NULL;
							dataL[i] = c;
							continue;
						}

						data[i] = new byte[c];
						dataL[i] = ret->ReadByteArray(data[i], 0, c);
					}

					// Equal

					if (x == 0)
					{
						pubKeys = data;
						pubKeysL = dataL;
						pubKeysCount = v;
					}
					else
					{
						signatures = data;
						signaturesL = dataL;
						signaturesCount = v;
					}
				}
			}
			else
			{
				int32 v = 0;
				if (!item->GetInt32(v) || v < 1 || v > ic)
				{
					this->State = EVMState::FAULT;
				}
				else
				{
					byte **data = new byte*[v];
					int32 *dataL = new int32[v];

					for (int32 i = 0; i < v; ++i)
					{
						IStackItem* ret = context->EvaluationStack->Pop();
						ic--;

						int32 c = ret->ReadByteArraySize();
						if (c < 0)
						{
							data[i] = NULL;
							dataL[i] = c;
							continue;
						}

						data[i] = new byte[c];
						dataL[i] = ret->ReadByteArray(data[i], 0, c);
						IStackItem::Free(ret);
					}

					// Equal

					if (x == 0)
					{
						pubKeys = data;
						pubKeysL = dataL;
						pubKeysCount = v;
					}
					else
					{
						signatures = data;
						signaturesL = dataL;
						signaturesCount = v;
					}
				}
			}

			IStackItem::Free(item);
		}

		if (this->State == EVMState::FAULT || this->OnGetMessage == NULL ||
			pubKeysCount <= 0 || signaturesCount <= 0 || signaturesCount > pubKeysCount)
		{
			if (pubKeys != NULL)	delete[](pubKeys);
			if (signatures != NULL) delete[](signatures);
			if (pubKeysL != NULL)	delete[](pubKeysL);
			if (signaturesL != NULL)delete[](signaturesL);

			context->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		// Read message

		// TODO: dangerous way to get the message

		byte* msg;
		int32 msgL = this->OnGetMessage(this->Iteration, msg);
		if (msgL <= 0)
		{
			if (pubKeys != NULL)	delete[](pubKeys);
			if (signatures != NULL) delete[](signatures);
			if (pubKeysL != NULL)	delete[](pubKeysL);
			if (signaturesL != NULL)delete[](signaturesL);

			context->EvaluationStack->Push(new BoolStackItem(false));
			return;
		}

		bool fSuccess = true;
		for (int32 i = 0, j = 0; fSuccess && i < signaturesCount && j < pubKeysCount;)
		{
			if (Crypto::VerifySignature(msg, msgL, signatures[i], signaturesL[i], pubKeys[j], pubKeysL[j]))
				++i;

			j++;

			if (signaturesCount - i > pubKeysCount - j)
			{
				fSuccess = false;
				break;
			}
		}

		if (pubKeys != NULL)	delete[](pubKeys);
		if (signatures != NULL) delete[](signatures);
		if (pubKeysL != NULL)	delete[](pubKeysL);
		if (signaturesL != NULL)delete[](signaturesL);

		context->EvaluationStack->Push(new BoolStackItem(fSuccess));
		*/
		return;
	}

	// Array

	case EVMOpCode::ARRAYSIZE:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 size;
		IStackItem *item = context->EvaluationStack->Pop();

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
			context->EvaluationStack->Push(new IntegerStackItem(size));
		}

		return;
	}
	case EVMOpCode::PACK:
	{
		int32 ec = context->EvaluationStack->Count();
		if (ec < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 size = 0;
		IStackItem *item = context->EvaluationStack->Pop();

		if (!item->GetInt32(size) || size < 0 || size >(ec - 1) || size > MAX_ARRAY_SIZE)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(item);
		ArrayStackItem *items = new ArrayStackItem(false);

		for (int32 i = 0; i < size; ++i)
		{
			items->Add(context->EvaluationStack->Pop());
		}

		context->EvaluationStack->Push(items);
		return;
	}
	case EVMOpCode::UNPACK:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();

		if (item->Type == EStackItemType::Array)
		{
			ArrayStackItem *array = (ArrayStackItem*)item;
			int32 count = array->Count();

			for (int32 i = count - 1; i >= 0; i--)
			{
				context->EvaluationStack->Push(array->Get(i));
			}

			IStackItem::Free(item);
			context->EvaluationStack->Push(new IntegerStackItem(count));
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
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *key = context->EvaluationStack->Pop();

		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			IStackItem::Free(key);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				IStackItem::Free(key, item);

				this->State = EVMState::FAULT;
				return;
			}

			IStackItem *ret = arr->Get(index);
			context->EvaluationStack->Push(ret);
			IStackItem::Free(key, item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem* map = (MapStackItem*)item;

			IStackItem* val = map->Get(key);

			IStackItem::Free(key, item);

			if (val != NULL)
			{
				context->EvaluationStack->Push(val);
			}
			else
			{
				this->State = EVMState::FAULT;
			}
			return;
		}
		default:
		{
			IStackItem::Free(key, item);

			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::SETITEM:
	{
		if (context->EvaluationStack->Count() < 3)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *value = context->EvaluationStack->Pop();
		if (value->Type == EStackItemType::Struct)
		{
			IStackItem * clone = ((ArrayStackItem*)value)->Clone();
			IStackItem::Free(value);
			value = clone;
		}

		IStackItem *key = context->EvaluationStack->Pop();
		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			IStackItem::Free(key, value);

			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				IStackItem::Free(key, item, value);

				this->State = EVMState::FAULT;
				return;
			}

			arr->Set(index, value);

			IStackItem::Free(key, item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			if (arr->Set(key, value) && arr->Count() > MAX_ARRAY_SIZE)
			{
				// Overflow in one the MAX_ARRAY_SIZE, but is more optimized than check if exists before

				this->State = EVMState::FAULT;
			}

			IStackItem::Free(key, value, item);
			return;
		}
		default:
		{
			IStackItem::Free(key, value, item);

			this->State = EVMState::FAULT;
			return;
		}
		}
	}
	case EVMOpCode::NEWARRAY:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem *item = context->EvaluationStack->Pop();

		if (!item->GetInt32(count) || count < 0 || count > MAX_ARRAY_SIZE)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(item);
		context->EvaluationStack->Push(new ArrayStackItem(false, count));
		return;
	}
	case EVMOpCode::NEWSTRUCT:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		int32 count = 0;
		IStackItem *item = context->EvaluationStack->Pop();

		if (!item->GetInt32(count) || count < 0 || count > MAX_ARRAY_SIZE)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem::Free(item);
		context->EvaluationStack->Push(new ArrayStackItem(true, count));
		return;
	}
	case EVMOpCode::NEWMAP:
	{
		context->EvaluationStack->Push(new MapStackItem());
		return;
	}
	case EVMOpCode::APPEND:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *newItem = context->EvaluationStack->Pop();
		if (newItem->Type == EStackItemType::Struct)
		{
			IStackItem * clone = ((ArrayStackItem*)newItem)->Clone();
			IStackItem::Free(newItem);
			newItem = clone;
		}

		IStackItem *item = context->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			if ((arr->Count() + 1) > MAX_ARRAY_SIZE)
			{
				this->State = EVMState::FAULT;
			}
			else
			{
				arr->Add(newItem);
			}

			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(newItem, item);

			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::REVERSE:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
		if (item->Type != EStackItemType::Array && item->Type != EStackItemType::Struct)
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}

		ArrayStackItem *arr = (ArrayStackItem*)item;
		arr->Reverse();
		IStackItem::Free(item);
		return;
	}
	case EVMOpCode::REMOVE:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *key = context->EvaluationStack->Pop();
		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			IStackItem::Free(key);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0 || index >= arr->Count())
			{
				IStackItem::Free(key, item);

				this->State = EVMState::FAULT;
				return;
			}

			arr->RemoveAt(index);

			IStackItem::Free(key, item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			arr->Remove(key);

			IStackItem::Free(key, item);
			return;
		}
		default:
		{
			IStackItem::Free(key, item);

			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::HASKEY:
	{
		if (context->EvaluationStack->Count() < 2)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *key = context->EvaluationStack->Pop();
		if (key->Type == EStackItemType::Map ||
			key->Type == EStackItemType::Array ||
			key->Type == EStackItemType::Struct)
		{
			IStackItem::Free(key);
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;

			int32 index = 0;
			if (!key->GetInt32(index) || index < 0)
			{
				IStackItem::Free(key, item);

				this->State = EVMState::FAULT;
				return;
			}

			context->EvaluationStack->Push(new BoolStackItem(index < arr->Count()));

			IStackItem::Free(key, item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			context->EvaluationStack->Push(new BoolStackItem(arr->Get(key) != NULL));

			IStackItem::Free(key, item);
			return;
		}
		default:
		{
			IStackItem::Free(key, item);
			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::KEYS:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			ArrayStackItem *ap = new ArrayStackItem(false);
			arr->FillKeys(ap);
			context->EvaluationStack->Push(ap);

			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}
	case EVMOpCode::VALUES:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem *item = context->EvaluationStack->Pop();
		switch (item->Type)
		{
		case EStackItemType::Array:
		case EStackItemType::Struct:
		{
			ArrayStackItem *arr = (ArrayStackItem*)item;
			context->EvaluationStack->Push(arr->Clone());
			IStackItem::Free(item);
			return;
		}
		case EStackItemType::Map:
		{
			MapStackItem *arr = (MapStackItem*)item;

			ArrayStackItem *ap = new ArrayStackItem(false);
			arr->FillValues(ap);
			context->EvaluationStack->Push(ap);

			IStackItem::Free(item);
			return;
		}
		default:
		{
			IStackItem::Free(item);
			this->State = EVMState::FAULT;
			return;
		}
		}
		return;
	}

	// Exceptions

	default:
	case EVMOpCode::THROW:
	{
		this->State = EVMState::FAULT;
		return;
	}
	case EVMOpCode::THROWIFNOT:
	{
		if (context->EvaluationStack->Count() < 1)
		{
			this->State = EVMState::FAULT;
			return;
		}

		IStackItem* item = context->EvaluationStack->Pop();

		if (!item->GetBoolean())
		{
			this->State = EVMState::FAULT;
		}

		IStackItem::Free(item);
		return;
	}
	}
}

// Destructor

ExecutionEngine::~ExecutionEngine()
{
	// Clean callbacks

	this->OnLoadScript = NULL;
	this->OnInvokeInterop = NULL;
	this->OnGetMessage = NULL;
	this->Log = NULL;

	// Clean stacks

	delete(this->InvocationStack);
	delete(this->ResultStack);

	// Clean scripts

	for (std::list<ExecutionScript*>::iterator it = this->Scripts.begin(); it != this->Scripts.end(); ++it)
	{
		ExecutionScript* ptr = (ExecutionScript*)*it;
		ExecutionScript::UnclaimAndFree(ptr);
	}

	this->Scripts.clear();
}