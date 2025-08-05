#!/bin/bash

# Globalny offset — ostatni commit BEZ wersji (czyli commit #199)
offset=199

# Liczba commitów od początku repo
total=$(git rev-list --count HEAD)

# Liczba commitów od offsetu (czyli wersjonowanych)
count=$((total - offset))

# Jeśli jeszcze nie zaczęto wersjonowania
if [ "$count" -le 0 ]; then
    echo "No version (waiting for commit #$((offset + 1)))"
    exit 0
fi

# Numer minor (np. 2 → 0.02X)
minor=$count

# Numer literki na podstawie całości commitów
letter_index=$(( (total - 1) / 99 ))  # <-- kluczowa zmiana
letter=$(printf "\x$(printf %x $((65 + letter_index)))")

# Wersja
version="0.$(printf "%02d" $minor)$letter"
echo "$version"
