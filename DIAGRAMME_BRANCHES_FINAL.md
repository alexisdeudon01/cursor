# 📊 Diagramme Final - Gestion des Branches

## Structure des Branches (État Actuel)

```mermaid
graph TD
    A[main<br/>Production] -->|Stable| B[Code en production]
    
    C[dev<br/>✅ BRANCHE ACTUELLE] -->|Développement| D[Améliorations continues]
    C -->|Auto-commit| E[GitHub Actions<br/>Toutes les 30min]
    C -->|Contient| F[Thebestclient5<br/>KEYS.txt local]
    
    G[dev-clean-final<br/>Branche propre] -->|Alternative| H[Si dev bloquée]
    
    I[dev-clean<br/>❌ SUPPRIMÉE] -->|Nettoyée| J[Plus utilisée]
    
    E -->|50% LLM jeux 2D| K[Entraînement LLM]
    E -->|50% Code| L[Amélioration code]
    E -->|Push auto| C
    
    style C fill:#95E1D3
    style F fill:#95E1D3
    style E fill:#FFE66D
    style K fill:#4ECDC4
    style L fill:#4ECDC4
    style I fill:#FF6B6B
```

## Flux de Travail Actuel

```mermaid
sequenceDiagram
    participant GH as GitHub Actions
    participant Dev as Branche dev
    participant LLM as LLM Jeux 2D
    participant Agent as Agent IA
    
    Note over GH,Agent: Cycle toutes les 30 minutes
    
    GH->>Agent: Déclenche cycle
    Agent->>LLM: 50% temps - Entraînement
    LLM->>LLM: Collecte règles
    LLM->>LLM: Crée dataset
    LLM->>LLM: Génère jeu 2D
    LLM->>LLM: Test compilation
    
    Agent->>Dev: 50% temps - Amélioration
    Agent->>Dev: Analyse code
    Agent->>Dev: Améliore
    Agent->>Dev: Crée thebestclientX+1.md
    Agent->>Dev: Commit automatique
    GH->>Dev: Push automatique
```

## Comparaison des Branches

| Branche | État | Usage | Clés |
|---------|------|-------|------|
| **main** | Production | Code stable | - |
| **dev** | ✅ **ACTIVE** | **Développement principal** | Local (KEYS.txt) |
| **dev-clean-final** | Alternative | Si dev bloquée | - |
| ~~dev-clean~~ | ❌ **SUPPRIMÉE** | Plus utilisée | - |

## Ce que tu dois faire

### 1. Tu es sur `dev` ✅
```bash
git branch --show-current
# Doit afficher: dev
```

### 2. KEYS.txt existe ✅
```bash
cat .github/KEYS.txt
# Doit afficher les clés
```

### 3. Ajouter clé dans GitHub Secrets
1. https://github.com/alexisdeudon01/cursor/settings/secrets/actions
2. New repository secret
3. Name: `ANTHROPIC_API_KEY`
4. Secret: (voir .github/KEYS.txt)
5. Add secret

### 4. Le système fonctionnera automatiquement! ✅

## Répartition 50/50

```
Cycle de 30 minutes:
├─ 15 minutes: Entraînement LLM jeux 2D
│  ├─ Collecte règles
│  ├─ Création dataset
│  ├─ Entraînement LLM
│  ├─ Génération jeu
│  └─ Test compilation
│
└─ 15 minutes: Amélioration code
   ├─ Analyse codebase
   ├─ Recherche patterns
   ├─ Améliorations
   ├─ Tests compilation
   └─ Génération diagrammes
```

## Résumé

- ✅ **Branche**: `dev` (active)
- ✅ **KEYS.txt**: Créé localement
- ✅ **Agent**: Thebestclient5 (50/50)
- ✅ **Branches inutiles**: Supprimées
- ✅ **Scripts**: setup-complete.sh, cleanup-branches.sh
- ⏳ **Action requise**: Ajouter clé dans GitHub Secrets

Une fois la clé ajoutée, le système fonctionnera automatiquement! 🚀
