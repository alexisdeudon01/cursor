#!/usr/bin/env python3
import argparse, os, pathlib, zipfile

def write_file(path: pathlib.Path, content: str, overwrite: bool):
    path.parent.mkdir(parents=True, exist_ok=True)
    if path.exists() and not overwrite:
        return
    path.write_text(content, encoding="utf-8")

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--repo", default=".", help="Path to your repo root")
    ap.add_argument("--zip-out", default="patch_bundle.zip", help="Zip output filename (created in repo root)")
    ap.add_argument("--overwrite", action="store_true", help="Overwrite existing files")
    args = ap.parse_args()

    repo = pathlib.Path(args.repo).resolve()
    if not (repo / ".git").exists():
        print("❌ Not a git repo (no .git found)")
        raise SystemExit(1)

    bundle_dir = pathlib.Path(__file__).resolve().parent
    content_map = {
        ".github/workflows/ci.yml": (bundle_dir / ".github/workflows/ci.yml").read_text(encoding="utf-8"),
        "scripts/unity_ai_loop.sh": (bundle_dir / "scripts/unity_ai_loop.sh").read_text(encoding="utf-8"),
        "scripts/claude_autofix.py": (bundle_dir / "scripts/claude_autofix.py").read_text(encoding="utf-8"),
    }

    for rel, content in content_map.items():
        write_file(repo / rel, content, args.overwrite)

    os.chmod(repo / "scripts/unity_ai_loop.sh", 0o755)
    os.chmod(repo / "scripts/claude_autofix.py", 0o755)

    zip_path = repo / args.zip_out
    if zip_path.exists() and args.overwrite:
        zip_path.unlink()

    with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as z:
        for rel in content_map.keys():
            z.write(repo / rel, rel)

    print(f"✅ Wrote files + zip: {zip_path}")

if __name__ == "__main__":
    main()
