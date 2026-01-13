# 🐳 Configuration Docker Unity

## Image Docker utilisée

**Image de base**: `unityci/editor:6000.3.0f1-linux-il2cpp-1.0.0`

**Version Unity**: `6000.3.0f1` (identique à `ProjectSettings/ProjectVersion.txt`)

## Dockerfile

Le Dockerfile est configuré pour :
- ✅ Unity 6000.3.0f1
- ✅ Build Linux (Client et Serveur)
- ✅ IL2CPP (pour performances)
- ✅ Dépendances système nécessaires

## Utilisation

### Build de l'image Docker

```bash
docker build -t unity-6000.3.0f1-builder .
```

### Build avec docker-compose

```bash
# Build Client
docker-compose run unity-builder /usr/local/bin/build-unity.sh client

# Build Serveur
docker-compose run unity-builder /usr/local/bin/build-unity.sh server

# Build complet (Client + Serveur)
docker-compose run unity-builder /usr/local/bin/build-unity.sh all
```

### Build manuel

```bash
docker run --rm \
  -v $(pwd):/workspace \
  -v unity-cache:/root/.unity3d \
  unity-6000.3.0f1-builder \
  /usr/local/bin/build-unity.sh all
```

## Structure des builds

### Build Client
- **Scènes**: Menu.unity, Client.unity, Game.unity
- **Target**: StandaloneLinux64
- **Output**: `Build/Client/Client.x86_64`

### Build Serveur
- **Scène**: Server.unity
- **Target**: LinuxServer
- **Output**: `Build/Server/Server.x86_64`

## Licence Unity

⚠️ **Important**: Pour utiliser Unity dans Docker, vous devez avoir une licence Unity valide.

Options :
1. **Licence personnelle** : Utiliser votre licence Unity personnelle
2. **Licence Unity Cloud Build** : Utiliser les services cloud Unity
3. **Licence serveur** : Pour builds automatisés

## Intégration GitHub Actions

Le Dockerfile peut être utilisé dans GitHub Actions pour :
- ✅ Tests de compilation automatiques
- ✅ Builds CI/CD
- ✅ Validation des changements

Exemple workflow :
```yaml
- name: Build Unity Client
  run: |
    docker build -t unity-builder .
    docker run --rm -v $PWD:/workspace unity-builder /usr/local/bin/build-unity.sh client
```

## Dependencies

Le Dockerfile installe automatiquement :
- Git
- Curl
- Bibliothèques graphiques (libgl1-mesa-glx, etc.)
- Bibliothèques système nécessaires pour Unity

## Cache Unity

Le volume `unity-cache` est utilisé pour :
- Cache des assets Unity
- Bibliothèque Unity
- Réduction du temps de build

## Notes

- Le Dockerfile utilise l'image officielle `unityci/editor`
- Version exacte : `6000.3.0f1-linux-il2cpp-1.0.0`
- Compatible avec les builds Linux uniquement
- IL2CPP activé pour meilleures performances

---

**Créé par**: Thebestclient6 Agent
**Date**: 2026-01-12
