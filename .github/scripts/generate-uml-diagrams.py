#!/usr/bin/env python3
from __future__ import annotations
from pathlib import Path
from typing import Dict, List
import sys

DIAGRAMS_DIR = Path(".cursor/agents/diagrams")
DIAGRAMS_DIR.mkdir(parents=True, exist_ok=True)

def write(name: str, content: str) -> Path:
    p = DIAGRAMS_DIR / name
    p.write_text(content, encoding="utf-8")
    return p

def arch(version:int)->Dict:
    m = f"""```mermaid
flowchart TB
  subgraph Build
    GH[GitHub Actions] --> CI[auto-improve.yml]
    CI --> DGM[generate-uml-diagrams.py]
    CI --> AI[auto-improve-ai.py / auto-improve.py]
    CI --> UB[unity-builder (docker)]
  end

  subgraph Runtime
    C[Client (NGO)] <--> S[Dedicated Server (authoritative)]
    C -->|RPC| NGO[Netcode for GameObjects]
    S -->|RPC| NGO
  end

  UB -->|Artifacts| C
  UB -->|Artifacts| S
```"""
    return {"type":"architecture","mermaid":write(f"architecture-v{version}.mmd", m), "desc":"Architecture CI + runtime (NGO)"}

def component(version:int)->Dict:
    m = f"""```mermaid
flowchart LR
  Client[Client App] --> NM[NetworkManager]
  Server[Dedicated Server] --> NM
  NM --> UT[UnityTransport]
  NM --> RPC[ServerRpc/ClientRpc]
  RPC --> AUTH[Authoritative Game Logic]
  AUTH --> STATE[Replicated State]
  STATE --> Client
```"""
    return {"type":"component","mermaid":write(f"component-v{version}.mmd", m), "desc":"Composants NGO + logique autoritaire"}

def sequence(version:int)->Dict:
    m = f"""```mermaid
sequenceDiagram
  participant Client
  participant NM as NGO/NetworkManager
  participant Server
  participant Game as GameLogic

  Client->>NM: StartClient()
  NM->>Server: Connect
  Server-->>NM: Accept + Spawn
  Server->>Game: Init authoritative state
  Server-->>Client: ClientRpc initial snapshot
  Client->>Client: Load scene / init UI
```"""
    return {"type":"sequence","mermaid":write(f"sequence-v{version}.mmd", m), "desc":"Séquence de connexion NGO"}

def fsm(version:int)->Dict:
    m = f"""```mermaid
stateDiagram-v2
  [*] --> Boot
  Boot --> MainMenu
  MainMenu --> Connecting: start client/server
  Connecting --> InLobby: connected
  InLobby --> InGame: start match
  InGame --> InLobby: end match
  InLobby --> MainMenu: disconnect
  Connecting --> MainMenu: fail/timeout
  InGame --> [*]: quit
```"""
    return {"type":"fsm","mermaid":write(f"fsm-v{version}.mmd", m), "desc":"FSM session"}

def er(version:int)->Dict:
    m = f"""```mermaid
erDiagram
  PLAYER ||--o{{ SESSION : joins
  SESSION ||--|| GAME : runs
  SESSION ||--o{{ ENTITY : has

  PLAYER {{
    string playerId
    string displayName
  }}
  SESSION {{
    string sessionId
    string mapId
  }}
  GAME {{
    string gameId
    string ruleset
  }}
  ENTITY {{
    string netId
    string prefab
  }}
```"""
    return {"type":"er","mermaid":write(f"er-v{version}.mmd", m), "desc":"ER logique"}

def links(version:int)->Dict:
    m = f"""```mermaid
graph LR
  Core --> NetworkingShared
  NetworkingShared --> NetworkingClient
  NetworkingShared --> NetworkingServer
  NetworkingServer --> DedicatedServer
  NetworkingClient --> ClientApp
```"""
    return {"type":"links","mermaid":write(f"links-v{version}.mmd", m), "desc":"Liens assemblies"}

def class_diag(version:int)->Dict:
    m = f"""```mermaid
classDiagram
  class NetworkManager
  class UnityTransport
  class ServerGame
  class ClientGame
  class ReplicatedState

  NetworkManager --> UnityTransport
  ServerGame --> ReplicatedState : writes
  ClientGame --> ReplicatedState : reads
```"""
    return {"type":"class","mermaid":write(f"class-v{version}.mmd", m), "desc":"Diagramme de classes (haut niveau)"}

def generate_all(version:int)->List[Dict]:
    return [
        arch(version),
        component(version),
        sequence(version),
        fsm(version),
        er(version),
        links(version),
        class_diag(version),
    ]

def main():
    version = int(sys.argv[1]) if len(sys.argv) > 1 else 1
    diags = generate_all(version)
    summary = DIAGRAMS_DIR / f"diagrams-v{version}.md"
    lines = [f"# Diagrams v{version}", ""]
    for d in diags:
        lines.append(f"## {d['type']}")
        lines.append(f"- {d['desc']}")
        lines.append(f"- Mermaid: `{d['mermaid']}`")
        lines.append("")
    summary.write_text("\n".join(lines), encoding="utf-8")
    print(f"✅ {len(diags)} diagramme(s) généré(s)")
    print(f"📄 Résumé: {summary}")

if __name__ == "__main__":
    main()
