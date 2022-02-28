#!/usr/bin/env bash

source_file="fill_memory.c"
executable="fill_memory"

run_test() {
    output=$(./$executable "$1")
    echo "$output"
    if ! echo "$output" | grep -q "$2" ; then
        echo -e "\nTest failed"
        exit 1
    else
        echo -e "\nSuccess!"
    fi
}

# Compile our program
# The container won't have gcc, as it takes ~150 MB in disk, so you'll
# need to install it for the tests, if you haven't it yet
if [ ! -f "$executable" ] ; then
    if ! type gcc 2> /dev/null | grep -qE ".+" ; then
        echo -e "Please install gcc to proceed.\n"
        sudo apt install -y gcc
    fi

    gcc "$source_file" -Os -Wall -o "$executable"
fi

echo -e "These are normal values for ulimit:\n"
ulimit -a

##########################################################################################
# Test normal functioning
echo -e "\nRunning first test...\n"


# ulimit uses kibibytes, so our program will too
program_memory=$(( 1024 * 256 )) # 256 MiB
expected_output="Finished. Total memory allocated = 262144 KiB."

run_test "$program_memory" "$expected_output"

##########################################################################################
# Test if ulimit can indeed halt our program
echo -e "\nRunning second test...\n"

program_memory=$(( 1024 * 1024 * 4 )) # 4 GiB
ulimit_memory=$(( 1024 * 896 )) # 896 MiB
expected_output="Could not allocate memory. Terminating..."

# As far as we know, the -Sm option does nothing, but, we are better off
# at least trying
echo -e "\nSetting max virtual memory to $ulimit_memory KiB."
echo -e "Setting max resident memory to $ulimit_memory KiB\n"
ulimit -Sv "$ulimit_memory" -Sm "$ulimit_memory"
ulimit -a

run_test "$program_memory" "$expected_output"

##########################################################################################
# Test if Docker will halt our program. Currently it doesn't, the way they limit
# resources is unsatisfactory (i.e., limiting memory does not limit swap memory)
# and we don't feel we can rely on it
echo -e "\nRunning third test... (we expect this to fail) \n"

program_memory=$(( 1024 * 1024 * 4 )) # 4 GiB
ulimit_memory="unlimited"
expected_output="Could not allocate memory. Terminating..."

echo -e "\nSetting max virtual memory to $ulimit_memory KiB."
echo -e "Setting max resident memory to $ulimit_memory KiB\n"
ulimit -Sv "$ulimit_memory" -Sm "$ulimit_memory"
ulimit -a

run_test "$program_memory" "$expected_output"
