# FOCA Python Rewrite (Scaffold)

This directory contains the initial Python scaffold for a full FOCA rewrite. The goal is
feature parity over time while delivering usable building blocks early.

## Current capabilities

- CLI entrypoint with subcommands for metadata extraction and search placeholders.
- Core metadata module that extracts basic file statistics and SHA-256 hash.

## Usage

```bash
python -m foca --help
python -m foca metadata /path/to/file
python -m foca search "example query" --engine duckduckgo
```

## Next steps

- Implement real search engine integrations.
- Add format-specific metadata extraction (Office/PDF/EXIF).
- Introduce plugin and UI layers.
