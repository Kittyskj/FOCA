import argparse
import json
from pathlib import Path

from foca.core.metadata import extract_basic_metadata
from foca.core.search import search_documents


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(prog="foca", description="FOCA Python rewrite scaffold")
    subparsers = parser.add_subparsers(dest="command", required=True)

    metadata_parser = subparsers.add_parser("metadata", help="Extract basic file metadata")
    metadata_parser.add_argument("path", type=Path, help="Path to the file")

    search_parser = subparsers.add_parser("search", help="Search for documents")
    search_parser.add_argument("query", help="Search query")
    search_parser.add_argument(
        "--engine",
        default="duckduckgo",
        choices=("duckduckgo", "bing", "google", "local"),
        help="Search engine placeholder",
    )
    search_parser.add_argument("--limit", type=int, default=10, help="Maximum results")

    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)

    if args.command == "metadata":
        result = extract_basic_metadata(args.path)
        print(json.dumps(result, indent=2, sort_keys=True))
        return 0

    if args.command == "search":
        results = search_documents(args.query, engine=args.engine, limit=args.limit)
        print(json.dumps([result.__dict__ for result in results], indent=2, sort_keys=True))
        return 0

    parser.error("Unknown command")
    return 1
