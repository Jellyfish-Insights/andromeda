#!/usr/bin/env bash
# set -x

produce_table() {
    cd ../../../logs/

    step_size_mb=100
    max_bars=28

    tmpfile_1=$( mktemp )
    tmpfile_2=$( mktemp )
    cat memory_* | sort -g > "$tmpfile_1"
    total_lines=$( wc -l "$tmpfile_1" | awk '{print $1}' )
    min_value_mb=$( head -1 "$tmpfile_1" )
    max_value_mb=$( tail -1 "$tmpfile_1" )

    printf "%16s %17s %9s %9s\n" "LOWER BOUND (MB)" "UPPER BOUND (MB)" "COUNT" "(%)"

    for i in {0..100} ; do
        lower_mb=$(( i * step_size_mb ))
        upper_mb=$(( (i + 1) * step_size_mb ))
        count=$( awk ''"$lower_mb"' < $1 && $1 < '"$upper_mb"'' "$tmpfile_1" | wc -l )
        if [[ "$i" != 0 && "$count" == 0 ]] ; then
            break
        fi
        percentage=$( echo "scale = 3; 100.0 * $count / $total_lines  " | bc )
        printf "%16d %17d %9d %9s \n" \
            "$lower_mb" "$upper_mb" "$count" "$percentage%" \
            >> "$tmpfile_2"
    done

    greatest_freq=$( sort -grk3 "$tmpfile_2" | head -1 | awk '{print $3}' )
    awk_command='
    {
        n_bars = '"$max_bars"' * $3 / '"$greatest_freq"';
        printf $0;
        format = sprintf("%%-%ds", n_bars);
        bars = sprintf(format, "");
        gsub(/ /, "#", bars);
        printf "%-s\n", bars;
    }'
    awk "$awk_command" "$tmpfile_2"

    printf "%0.s-" {1..54}
    echo
    one_hundred=$( echo "scale = 3; 100.0 " | bc )
    printf "%16s %17s %9d %9s\n" "" "" "$total_lines" "$one_hundred%"
    echo
    printf "%44s %9s\n" "MIN VALUE (MB)" "$min_value_mb"
    printf "%44s %9s\n" "MAX VALUE (MB)" "$max_value_mb"


    rm "$tmpfile_1"
    rm "$tmpfile_2"
    cd - &> /dev/null
}


produce_table
