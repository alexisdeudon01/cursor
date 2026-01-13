# TheBestClient

Full Authoritative Server game using Unity Netcode for GameObjects.

## Architecture

- **Server**: Full authority (game state, physics, validation)
- **Client**: Display only (input sending, rendering)
- **DOD**: Data-Oriented Design with struct DTOs

## Usage

```bash
./TheBestGame --server --port 7777
./TheBestGame --client --ip 192.168.1.100 --port 7777
```

## CI/CD

Auto-improvement runs every 15 minutes via GitHub Actions.

## Tech Stack

- Unity 6000.0.23f1
- Netcode for GameObjects 2.0.0
- Unity Transport 2.3.0
