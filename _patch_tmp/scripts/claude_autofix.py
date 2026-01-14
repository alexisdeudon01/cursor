#!/usr/bin/env python3
import argparse
import json
import os
import sys
import urllib.request

CLAUDE_API_URL = "https://api.anthropic.com/v1/messages"
DEFAULT_MODEL = os.environ.get("ANTHROPIC_MODEL") or "claude-3-5-sonnet-20241022"

SYSTEM = """You are an expert Unity + C# + CI engineer.
You will receive a Unity Editor log from a GitHub Actions runner.
Goal: produce a minimal git patch (unified diff) that fixes the compile/test failure.

Rules:
- Output ONLY a unified diff (git apply compatible). No prose.
- Keep changes minimal and targeted.
- Prefer compile fixes over refactors.
- Do not touch .github/workflows in this patch unless strictly necessary.
- Assume Unity project root is repository root.
"""

USER_TEMPLATE = """Context: {title}

Unity log:
---LOG START---
{log}
---LOG END---

Repository tree hints (paths seen in the error):
- Assets/Scripts/Networking/RpcHandlers/Interfaces/ISessionRpcHandler.cs
- Assets/Scripts/Networking/RpcHandlers/Base/BaseRpcHandler.cs
- Assets/Scripts/Networking/Player/SessionRpcHub.cs

Now produce a unified diff patch that fixes the issue.
"""

def call_anthropic(api_key: str, model: str, user: str) -> str:
    payload = {
        "model": model,
        "max_tokens": 2000,
        "temperature": 0.2,
        "system": SYSTEM,
        "messages": [{"role": "user", "content": user}],
    }
    data = json.dumps(payload).encode("utf-8")

    req = urllib.request.Request(CLAUDE_API_URL, data=data, method="POST")
    req.add_header("Content-Type", "application/json")
    req.add_header("x-api-key", api_key)
    req.add_header("anthropic-version", "2023-06-01")

    with urllib.request.urlopen(req, timeout=60) as resp:
        raw = resp.read().decode("utf-8", errors="replace")
    j = json.loads(raw)

    blocks = j.get("content") or []
    out = ""
    for b in blocks:
        if b.get("type") == "text":
            out += b.get("text", "")
    return out.strip()

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--title", required=True)
    ap.add_argument("--log-file", required=True)
    ap.add_argument("--out", required=True)
    args = ap.parse_args()

    api_key = os.environ.get("ANTHROPIC_API_KEY", "").strip()
    if not api_key:
        print("Missing ANTHROPIC_API_KEY", file=sys.stderr)
        sys.exit(2)

    model = (os.environ.get("ANTHROPIC_MODEL") or DEFAULT_MODEL).strip()

    try:
        log = open(args.log_file, "r", encoding="utf-8", errors="ignore").read()
    except FileNotFoundError:
        print(f"Log file not found: {args.log_file}", file=sys.stderr)
        sys.exit(2)

    # keep request small-ish
    if len(log) > 35000:
        log = log[-35000:]

    user = USER_TEMPLATE.format(title=args.title, log=log)
    patch = call_anthropic(api_key, model, user)

    # guard: ensure it looks like a diff
    if "diff --git" not in patch and not patch.startswith("--- "):
        patch = ""

    with open(args.out, "w", encoding="utf-8") as f:
        f.write(patch)

    print(f"Wrote patch to {args.out} (len={len(patch)})")

if __name__ == "__main__":
    main()
