#!/usr/bin/env bash
# Unpacks the dataset archives in datasets-archive/ into datasets/ so the results explorer can show them.
# An existing dataset folder is left alone (delete it first to restore it again).
set -euo pipefail
cd "$(dirname "$0")/.."
mkdir -p datasets
for archive in datasets-archive/*.tar.gz; do
  id="$(basename "$archive" .tar.gz)"
  if [ -d "datasets/$id" ]; then
    echo "datasets/$id already exists; skipped."
  else
    tar -C datasets -xzf "$archive"
    echo "Restored datasets/$id."
  fi
done
