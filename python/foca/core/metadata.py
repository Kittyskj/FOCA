import hashlib
from dataclasses import asdict, dataclass
from datetime import datetime
from pathlib import Path


@dataclass(frozen=True)
class MetadataResult:
    path: str
    size_bytes: int
    modified_utc: str
    sha256: str


def extract_basic_metadata(path: Path) -> dict[str, str | int]:
    if not path.exists():
        raise FileNotFoundError(f"File not found: {path}")

    stat = path.stat()
    sha256 = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            sha256.update(chunk)

    result = MetadataResult(
        path=str(path),
        size_bytes=stat.st_size,
        modified_utc=datetime.utcfromtimestamp(stat.st_mtime).isoformat() + "Z",
        sha256=sha256.hexdigest(),
    )
    return asdict(result)
