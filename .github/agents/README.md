# Unity NGO Dedicated Server Agent (GitHub)

This directory contains the GitHub agent configuration for implementing a Unity 2D game with Netcode for GameObjects (NGO) and a dual-build system (client/server).

## Agent Files

- **cursor-ngo-dedicated-server.md** - Main agent configuration file (same as Cursor agent)
- **keep-only-ngo-agent.sh** - Helper script to remove other agents and keep only this one

## Usage

See the main documentation in `.cursor/agents/README.md` for complete usage instructions.

### Quick Command

To keep only this agent in GitHub:

```bash
./.github/agents/keep-only-ngo-agent.sh
```

## Note

This is a mirror of the Cursor agent configuration, ensuring compatibility across different development environments including GitHub Copilot and other GitHub-integrated tools.
