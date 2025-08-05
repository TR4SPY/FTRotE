#!/bin/bash

# OFFSET — commit numer, od którego zaczynamy liczenie (199 w tym przypadku)
offset=199

# Liczba commitów w repo
total=$(git rev-list --count HEAD)

# Ile commitów od startu wersjonowania
count=$((total - offset))

if [ "$count" -le 0 ]; then
    echo "No version (waiting for commit #$((offset + 1)))"
    exit 0
fi

# Wylicz numer wersji
minor=$((count))
letter_index=$(( (minor - 1) / 99 ))
letter=$(printf "\x$(printf %x $((65 + letter_index)))")
version="0.$(printf "%02d" $minor)$letter"

echo "$version"
