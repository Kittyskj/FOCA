# FOCA Python Rewrite (Scaffold)

This directory contains the initial Python scaffold for a full FOCA rewrite. The goal is
feature parity over time while delivering usable building blocks early.

## Current capabilities

- CLI entrypoint with metadata extraction and local filesystem search.
- Core metadata module that extracts basic file statistics, MIME type, and SHA-256 hash.

## Usage

```bash
python -m foca --help
python -m foca metadata /path/to/file
python -m foca search "example query" --engine local --root /path/to/scan
```

## Next steps

- Implement real search engine integrations (DuckDuckGo/Bing/Google).
- Add format-specific metadata extraction (Office/PDF/EXIF).
- Introduce plugin and UI layers.
