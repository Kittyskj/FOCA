from dataclasses import dataclass
from datetime import datetime
from typing import Iterable


@dataclass(frozen=True)
class SearchResult:
    title: str
    url: str
    engine: str
    retrieved_utc: str


def search_documents(query: str, *, engine: str, limit: int) -> Iterable[SearchResult]:
    """
    Placeholder implementation for search.

    This returns a deterministic stub result to provide a baseline CLI flow while
    the real engine integrations are migrated.
    """
    retrieved = datetime.utcnow().isoformat() + "Z"
    limit = max(0, min(limit, 50))
    if limit == 0:
        return []
    return [
        SearchResult(
            title=f"TODO: integrate {engine} search for: {query}",
            url="https://example.com",
            engine=engine,
            retrieved_utc=retrieved,
        )
    ]
