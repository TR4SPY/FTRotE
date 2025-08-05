#!/bin/bash

start_commit="523db6d"

count=$(git rev-list --count ${start_commit}..HEAD)

if [ "$count" -eq 0 ]; then
    echo "No version (waiting for next commit)"
    exit 0
fi

minor=$((count))
letter_index=$(( (count - 1) / 99 ))
letter=$(printf "\x$(printf %x $((65 + letter_index)))")

version="0.$(printf "%02d" $minor)$letter"
echo "$version"
