#pragma once

// Types

typedef unsigned char byte;

typedef short int16;
typedef unsigned short uint16;

typedef int int32;
typedef unsigned int uint32;

typedef long int64;
typedef unsigned long uint64;

// ExecutionEngine Callbacks

typedef byte(__stdcall * InvokeInteropCallback)(const char* method);
typedef int32(__stdcall * GetScriptCallback)(const byte* scriptHash, byte* &script);
typedef int32(__stdcall * GetMessageCallback)(uint32 iteration, byte* &message);