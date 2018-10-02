TARGET_EXEC ?= Neo.HyperVM.so

ROOT_DIR:=$(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

LIBS_DIR ?= $(ROOT_DIR)/../build/libs
BUILD_DIR ?= ./bin
SRC_DIRS ?= ./Neo.HyperVM

SRCS := $(shell find $(SRC_DIRS) -name *.cpp -or -name *.c -or -name *.s)
OBJS := $(SRCS:%=$(BUILD_DIR)/%.o)
DEPS := $(OBJS:.o=.d)

INC_DIRS  := $(shell find $(SRC_DIRS) -type d) $(LIBS_DIR)/openssl/include
INC_FLAGS := $(addprefix -I,$(INC_DIRS))
LIBS := $(LIBS_DIR)/openssl/liblinux-openssl-crypto-x86_64.a

CPPFLAGS ?= $(INC_FLAGS) -Ofast -MMD -MP -Wall -fPIC --std=c++11 -ldl

$(BUILD_DIR)/$(TARGET_EXEC): $(OBJS)
	$(CXX) -fPIC -shared $(OBJS) $(LIBS) -o $@ $(LDFLAGS)

# assembly
$(BUILD_DIR)/%.s.o: %.s
	$(MKDIR_P) $(dir $@)
	$(AS) $(ASFLAGS) -c $< -o $@

# c++ source
$(BUILD_DIR)/%.cpp.o: %.cpp
	$(MKDIR_P) $(dir $@)
	$(CXX) $(CPPFLAGS) $(CXXFLAGS) -c $< -o $@

.PHONY: clean

clean:
	$(RM) -r $(BUILD_DIR)

-include $(DEPS)

MKDIR_P ?= mkdir -p
