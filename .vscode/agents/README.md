# Unity NGO Dedicated Server Agent (VS Code)

This directory contains the VS Code agent configuration for implementing a Unity 2D game with Netcode for GameObjects (NGO) and a dual-build system (client/server).

## Agent Files

- **cursor-ngo-dedicated-server.md** - Main agent configuration file (same as Cursor agent)
- **keep-only-ngo-agent.sh** - Helper script to remove other agents and keep only this one

## Usage

See the main documentation in `.cursor/agents/README.md` for complete usage instructions.

### Quick Command

To keep only this agent in VS Code:

```bash
./.vscode/agents/keep-only-ngo-agent.sh
```

## Note

This is a mirror of the Cursor agent configuration, ensuring compatibility across different development environments.
