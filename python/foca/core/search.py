from dataclasses import dataclass
from datetime import datetime
from pathlib import Path
from typing import Iterable, Sequence


@dataclass(frozen=True)
class SearchResult:
    title: str
    url: str
    engine: str
    retrieved_utc: str


def search_documents(
    query: str,
    *,
    engine: str,
    limit: int,
    root: Path | None = None,
) -> Sequence[SearchResult]:
    """
    Search documents based on the requested engine.

    For now, only the local filesystem search is implemented. Other engines are
    stubs that return a placeholder result.
    """
    retrieved = datetime.utcnow().isoformat() + "Z"
    limit = max(0, min(limit, 50))
    if limit == 0:
        return []

    if engine == "local":
        return _search_local(query, root or Path.cwd(), limit, retrieved)

    return [
        SearchResult(
            title=f"TODO: integrate {engine} search for: {query}",
            url="https://example.com",
            engine=engine,
            retrieved_utc=retrieved,
        )
    ]


def _search_local(query: str, root: Path, limit: int, retrieved: str) -> list[SearchResult]:
    if not root.exists():
        raise FileNotFoundError(f"Search root does not exist: {root}")

    results: list[SearchResult] = []
    query_lower = query.lower()
    for path in root.rglob("*"):
        if path.is_dir():
            continue
        if query_lower in path.name.lower():
            results.append(
                SearchResult(
                    title=path.name,
                    url=str(path.resolve().as_uri()),
                    engine="local",
                    retrieved_utc=retrieved,
                )
            )
            if len(results) >= limit:
                break
    return results
