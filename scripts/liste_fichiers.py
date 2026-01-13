#!/usr/bin/env python3
"""
Liste les fichiers du dépôt.

Par défaut, le script liste les fichiers *trackés* par git (équivalent à `git ls-files`).
Optionnellement, il peut inclure les fichiers non trackés (en respectant .gitignore)
via `--all`.
"""

from __future__ import annotations

import argparse
import os
import subprocess
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, List, Optional


@dataclass(frozen=True)
class GitPaths:
    repo_root: Path
    files: List[str]  # paths relative to repo root, posix style


def _run_git(args: List[str], cwd: Path) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        ["git", *args],
        cwd=str(cwd),
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        check=False,
    )


def _git_repo_root(cwd: Path) -> Optional[Path]:
    cp = _run_git(["rev-parse", "--show-toplevel"], cwd=cwd)
    if cp.returncode != 0:
        return None
    return Path(cp.stdout.strip())


def _git_ls_files(repo_root: Path, include_untracked: bool) -> GitPaths:
    # Use -z to be robust to special chars, then decode safely.
    tracked = _run_git(["ls-files", "-z"], cwd=repo_root)
    if tracked.returncode != 0:
        raise RuntimeError(tracked.stderr.strip() or "git ls-files failed")
    out = tracked.stdout
    paths = [p for p in out.split("\0") if p]

    if include_untracked:
        untracked = _run_git(["ls-files", "--others", "--exclude-standard", "-z"], cwd=repo_root)
        if untracked.returncode != 0:
            raise RuntimeError(untracked.stderr.strip() or "git ls-files --others failed")
        paths.extend([p for p in untracked.stdout.split("\0") if p])

    # Normalize separators and ensure stable ordering.
    normed = sorted({p.replace("\\", "/") for p in paths})
    return GitPaths(repo_root=repo_root, files=normed)


def _walk_files_fallback(repo_root: Path) -> List[str]:
    # Fallback when git is not available: walk the directory and ignore .git.
    files: List[str] = []
    for path in repo_root.rglob("*"):
        if path.is_dir():
            continue
        rel = path.relative_to(repo_root)
        # very small, explicit exclusions
        if rel.parts and rel.parts[0] == ".git":
            continue
        files.append(rel.as_posix())
    return sorted(files)


def _build_tree(paths: Iterable[str]) -> dict:
    root: dict = {}
    for p in paths:
        parts = [x for x in p.split("/") if x]
        node = root
        for i, part in enumerate(parts):
            is_leaf = i == len(parts) - 1
            if is_leaf:
                node.setdefault("__files__", []).append(part)
            else:
                node = node.setdefault(part, {})
    return root


def _render_tree(node: dict, prefix: str = "") -> List[str]:
    lines: List[str] = []

    dirs = sorted([k for k in node.keys() if k != "__files__"])
    files = sorted(node.get("__files__", []))

    entries: List[tuple[str, Optional[dict]]] = [(d, node[d]) for d in dirs] + [(f, None) for f in files]
    for idx, (name, child) in enumerate(entries):
        last = idx == len(entries) - 1
        branch = "└── " if last else "├── "
        lines.append(f"{prefix}{branch}{name}")
        if child is not None:
            extension = "    " if last else "│   "
            lines.extend(_render_tree(child, prefix + extension))
    return lines


def _print_lines(lines: Iterable[str]) -> None:
    for line in lines:
        sys.stdout.write(line)
        sys.stdout.write("\n")


def main(argv: Optional[List[str]] = None) -> int:
    parser = argparse.ArgumentParser(description="Liste les fichiers du dépôt (via git si possible).")
    parser.add_argument(
        "--all",
        action="store_true",
        help="Inclut aussi les fichiers non trackés (en respectant .gitignore).",
    )
    parser.add_argument(
        "--tree",
        action="store_true",
        help="Affiche une arborescence (tree) au lieu d’une liste simple.",
    )
    parser.add_argument(
        "--root",
        default=".",
        help="Répertoire de départ (par défaut: .). Si c’est un dépôt git, le root git est utilisé.",
    )

    args = parser.parse_args(argv)
    start = Path(args.root).resolve()

    repo_root = _git_repo_root(start)
    if repo_root is None:
        repo_root = start
        rel_paths = _walk_files_fallback(repo_root)
    else:
        rel_paths = _git_ls_files(repo_root, include_untracked=args.all).files

    if args.tree:
        tree = _build_tree(rel_paths)
        header = [f"{repo_root.name}/"]
        body = _render_tree(tree)
        _print_lines(header + body)
    else:
        _print_lines(rel_paths)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
