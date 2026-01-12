# 🔄 Comment Switcher d'Agent

## 📋 Agents disponibles

Actuellement dans `.cursor/agents/` :
- `thebestclient2.md` - Agent v2
- `thebestclient3.md` - Agent v3
- `thebestclient4.md` - Agent v4
- `thebestclient5.md` - Agent v5 (actuel) ⭐
- `cursor-ngo-dedicated-server.md` - Agent original Unity NGO

## 🎯 Méthode 1 : Switcher dans Cursor (pour toi)

### Option A : Via la commande @ (Recommandé)

Dans le chat Cursor, utilise directement `@` avec le nom du fichier :

```
@thebestclient5
```

ou avec le chemin complet :

```
@.cursor/agents/thebestclient5.md
```

Pour switcher vers un autre agent :

```
@thebestclient4
```

ou

```
@.cursor/agents/thebestclient4.md
```

### Option B : Via mention dans le prompt

Commence ton message par :

```
Utilise l'agent thebestclient5 défini dans .cursor/agents/thebestclient5.md

[Ta question ici]
```

### Option C : Via les paramètres Cursor

1. Ouvrir les paramètres : `Ctrl+,` (ou `Cmd+,`)
2. Rechercher "agent" ou "custom agent"
3. Dans "Custom Agents", ajouter/modifier le chemin :
   - `.cursor/agents/thebestclient5.md`
   - Ou chemin absolu : `/home/tor/wkspaces/mo2/.cursor/agents/thebestclient5.md`

### Option D : Référence directe dans le code

Dans le chat, tu peux dire :

```
@.cursor/agents/thebestclient5.md

Analyse le code et propose des améliorations.
```

## 🤖 Méthode 2 : Switcher dans le Workflow GitHub Actions (automatique)

Le workflow GitHub Actions détecte **automatiquement** la dernière version de l'agent.

### Comment ça fonctionne

Le script `.github/scripts/auto-improve-ai.py` :

1. **Détecte automatiquement** la dernière version :
   ```python
   def get_latest_agent_version() -> int:
       pattern = re.compile(r'thebestclient(\d+)\.md')
       # Scan tous les fichiers thebestclient*.md
       # Retourne le numéro le plus élevé
   ```

2. **Utilise cette version** pour l'analyse :
   ```python
   current_version = get_latest_agent_version()  # Ex: 5
   agent_instructions = read_agent_instructions(current_version)
   ```

3. **Crée la prochaine version** automatiquement :
   ```python
   next_version = current_version + 1  # Ex: 6
   # Crée thebestclient6.md
   ```

### Switcher manuellement la version utilisée

Si tu veux forcer une version spécifique, modifie `.github/scripts/auto-improve-ai.py` :

```python
# Ligne 332, remplacer :
current_version = get_latest_agent_version()

# Par :
current_version = 4  # Force l'utilisation de thebestclient4.md
```

**⚠️ Note:** Ce n'est pas recommandé car le système est conçu pour utiliser automatiquement la dernière version.

## 📊 Vérifier quel agent est utilisé

### Dans Cursor

Demande simplement :

```
Quel agent utilises-tu actuellement ?
```

ou

```
Quelle est ta version ?
```

### Dans le workflow GitHub Actions

1. Aller sur GitHub → **Actions**
2. Ouvrir la dernière exécution de `Auto-Improve Project (Thebestclient)`
3. Regarder les logs :
   ```
   📊 Version actuelle: 5
   📊 Prochaine version: 6
   ```

## 🔄 Processus automatique de versioning

Le système crée automatiquement de nouvelles versions :

1. **Cycle 1** : Utilise `thebestclient5.md`
   - Analyse le code
   - Crée `thebestclient6.md` avec améliorations

2. **Cycle 2** : Utilise `thebestclient6.md` (détecté automatiquement)
   - Analyse le code
   - Crée `thebestclient7.md` avec améliorations

3. **Et ainsi de suite...**

## 🎯 Cas d'usage

### Utiliser un agent spécifique dans Cursor

Si tu veux utiliser `thebestclient3` au lieu de `thebestclient5` :

```
@.cursor/agents/thebestclient3.md

Analyse le projet et propose des améliorations.
```

### Revenir à une version précédente

Si `thebestclient6` a des problèmes, tu peux :

1. **Dans Cursor** : Utiliser `@thebestclient5`
2. **Dans le workflow** : Supprimer `thebestclient6.md` (le système utilisera v5)

### Créer un agent personnalisé

1. Créer un nouveau fichier : `.cursor/agents/mon-agent-custom.md`
2. Utiliser dans Cursor : `@.cursor/agents/mon-agent-custom.md`
3. Le workflow continuera d'utiliser `thebestclientX` (il ne détecte que ces fichiers)

## 📝 Structure d'un agent

Chaque agent doit avoir ce format :

```markdown
---
name: Thebestclient5
description: Agent AI v5 - ...
model: default
readonly: false
---

# Rôle
Tu es un **agent AI** qui...

## Instructions
...
```

## ✅ Résumé rapide

### Pour toi (Cursor) :
```
@thebestclient5          # Utilise v5
@thebestclient4          # Utilise v4
@.cursor/agents/thebestclient5.md  # Chemin complet
```

### Pour le workflow (automatique) :
- ✅ Détecte automatiquement la dernière version
- ✅ Utilise `thebestclient5.md` actuellement
- ✅ Créera `thebestclient6.md` au prochain cycle
- ✅ Passera automatiquement à v6 au cycle suivant

---

**💡 Astuce :** Le workflow utilise toujours la **dernière version** automatiquement. Tu n'as rien à faire !
