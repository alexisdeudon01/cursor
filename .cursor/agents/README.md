# Unity NGO Dedicated Server Agent

This directory contains the Cursor agent configuration for implementing a Unity 2D game with Netcode for GameObjects (NGO) and a dual-build system (client/server).

## Agent Files

- **cursor-ngo-dedicated-server.md** - Main agent configuration file
- **keep-only-ngo-agent.sh** - Helper script to remove other agents and keep only this one

## Usage

### Using the Agent

The agent is automatically recognized by Cursor when working in this repository. It provides specialized guidance for:

- Unity 2D + Netcode for GameObjects (NGO) development
- Authoritative dedicated server architecture
- Client/Server dual-build system
- Session management with FSM (Lobby → Starting → InGame → Ended)
- Network validation and testing

### Managing Agents

To keep only this agent and remove all others from the Cursor agents directory:

```bash
# From the repository root
./.cursor/agents/keep-only-ngo-agent.sh

# Or specify a different repository path
./.cursor/agents/keep-only-ngo-agent.sh /path/to/repo
```

For VS Code agents:

```bash
# From the repository root
./.vscode/agents/keep-only-ngo-agent.sh

# Or specify a different repository path
./.vscode/agents/keep-only-ngo-agent.sh /path/to/repo
```

## Agent Specification

The agent follows these key principles:

### Architecture
- **Client**: Multi-scene (Lobby, Game, Results), UI/FX/Sound, sends intentions only
- **Server**: Headless (-batchmode -nographics), authoritative simulation, strict validation
- **Shared**: Network protocols, DTOs, utilities

### Core Components
- `SessionRpcHub`: RPC hub (ServerRpc/ClientRpc) orchestrating session/game flows
- `SessionContainer`: FSM managing session lifecycle
- `SessionContainerManager`: Server-side multi-session manager
- `GameSessionManager` / `GameInstanceManager`: Game creation/management
- `ConnectionController`: Connection handling, client-to-session mapping
- `PlayerInputHandler`: Client intentions (inputs), never final state
- `NetworkManager` + `UnityTransport`: Single NGO transport

### Coding Conventions
- Avoid critical gameplay logic on the client
- Validate all commands on the server (cooldowns, bounds, collisions)
- Prefer serializable structs for snapshots (GameStateSnapshot, SnapshotEntity)
- Use `#if UNITY_SERVER` for server-specific code
- Dedicated assemblies for client/server/shared code

## Repository

- **Repository**: https://github.com/alexisdeudon01/cursor
- **Branch**: branche-1
- **Raw URL**: https://raw.githubusercontent.com/alexisdeudon01/cursor/branche-1/.cursor/agents/cursor-ngo-dedicated-server.md

## What the Agent Produces

When working with this agent, you can expect:

1. Implementation plan aligned with architecture diagrams
2. Code skeleton (C#) for main classes
3. Build pipeline (client/server) within the same Visual Studio solution
4. Network validation checklist (local tests, server + multi clients)
5. Linux scripts for agent management and deployment

## Notes

- This agent is specifically designed for Unity 2D projects
- No external services (auth, matchmaking, analytics, cloud) - everything is local
- Architecture must follow the provided diagrams (components, classes, states, sequences, packages, data schemas)
