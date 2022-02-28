#include <stdio.h>
#include <stdlib.h>

int main(int argc, char * argv[])
{
    if (argc != 2) {
        printf("Usage: %s <KiB_TO_FILL>\n", argv[0]);
        return 1;
    }
    char * endptr;
    const long memory_to_fill_kib = strtol(argv[1], &endptr, 10);

    if (!(*argv[1] != '\0' && *endptr == '\0')) {
        printf("Invalid string in base 10: '%s'\n", argv[1]);
        return 1;
    }

    if (memory_to_fill_kib < 0) {
        printf("Argument must be non-negative integer.\n");
        return 1;
    }

    const int page_size_bytes = 4 * 1024; // 4 KiB
    const int alloc_size_bytes = 1024 * 1024 * 128; // 128 MiB

    long total_alloc_kib;
    for (
        total_alloc_kib = 0;
        total_alloc_kib < memory_to_fill_kib;
        total_alloc_kib += 128 * 1024
    ) {
        printf("Filling memory... %ld KiB consumed.\n", total_alloc_kib);
        int * ptr = malloc(alloc_size_bytes);
        if (ptr == NULL) {
            printf("Could not allocate memory. Terminating...\n");
            return 1;
        }

        // We need to write to the memory due to lazy mapping
        for (int j = 0; j < alloc_size_bytes / sizeof(int); j++) {
            // Just writing a random value
            ptr[j] = (j >> 5) - j + page_size_bytes ;
        }

    }
    printf("Finished. Total memory allocated = %ld KiB.\n", total_alloc_kib);
    return 0;
}
