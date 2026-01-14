import os, subprocess, textwrap
from openai import OpenAI

MODEL = os.getenv("CODEX_MODEL", "gpt-5-codex")

TARGET_FILES = [
    ".github/workflows/ci.yml",
    "Dockerfile.ci",
    "Assets/Editor/CI/BuildLinuxMono.cs",
]

SYSTEM = """You are a CI/CD + Unity build expert.
Goal: propose small, safe improvements to the repo's CI pipeline (Unity + GameCI + Docker).
Rules:
- Only modify the files listed in TARGET_FILES.
- Changes must be minimal and reversible.
- Output MUST be a unified diff patch (git apply compatible), and nothing else.
"""

def sh(cmd: str) -> str:
    return subprocess.check_output(cmd, shell=True, text=True).strip()

def read_file(p: str) -> str:
    try:
        with open(p, "r", encoding="utf-8") as f:
            return f.read()
    except FileNotFoundError:
        return ""

def main():
    if not os.getenv("OPENAI_API_KEY"):
        print("OPENAI_API_KEY missing -> skipping codex step.")
        return

    client = OpenAI()

    repo_state = "\n\n".join(
        f"--- {p} ---\n{read_file(p)}" for p in TARGET_FILES
    )

    prompt = textwrap.dedent(f"""
    Here are the current files. Propose improvements (reliability, caching, clarity, security).
    Return ONLY a unified diff patch.

    {repo_state}
    """)

    resp = client.responses.create(
        model=MODEL,
        input=[
            {"role": "system", "content": SYSTEM},
            {"role": "user", "content": prompt},
        ],
    )

    patch = (resp.output_text or "").strip()
    if not patch.startswith("diff --git"):
        print("No valid diff patch produced -> skipping.")
        return

    with open("codex.patch", "w", encoding="utf-8") as f:
        f.write(patch + "\n")

    sh("git config user.name 'codex-bot'")
    sh("git config user.email 'codex-bot@users.noreply.github.com'")

    branch = "codex/ci-autotune"
    sh(f"git checkout -B {branch}")

    subprocess.check_call("git apply codex.patch", shell=True)

    if sh("git status --porcelain") == "":
        print("No changes after applying patch.")
        return

    sh("git add " + " ".join(TARGET_FILES))
    sh("git commit -m 'chore(ci): auto-tune pipeline'")
    sh(f"git push -f origin {branch}")

    # PR via GitHub CLI (déjà dispo sur ubuntu-latest)
    subprocess.check_call(
        'gh pr create --title "chore(ci): auto-tune pipeline (Codex)" '
        '--body "Automated small CI improvements proposed by Codex agent." '
        "--base main --head codex/ci-autotune || true",
        shell=True,
    )

if __name__ == "__main__":
    main()
