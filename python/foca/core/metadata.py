import hashlib
import mimetypes
from dataclasses import asdict, dataclass
from datetime import datetime, timezone
from pathlib import Path


@dataclass(frozen=True)
class MetadataResult:
    path: str
    size_bytes: int
    mime_type: str
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

    mime_type, _ = mimetypes.guess_type(path.name)
    result = MetadataResult(
        path=str(path),
        size_bytes=stat.st_size,
        mime_type=mime_type or "application/octet-stream",
        modified_utc=datetime.fromtimestamp(stat.st_mtime, tz=timezone.utc).isoformat(),
        sha256=sha256.hexdigest(),
    )
    return asdict(result)
