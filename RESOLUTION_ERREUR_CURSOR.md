# 🔧 Résolution Erreur Cursor

## Erreur
```
Failed to checkout branch: Cannot read properties of undefined (reading 'uri')
```

Cette erreur est liée à **Cursor IDE**, pas à Git directement.

## ✅ Configuration actuelle (vérifiée)

- **Branche**: `dev` ✅
- **Remote**: Configuré ✅
- **Fichiers**: Présents ✅
- **KEYS.txt**: Existe ✅

## 🔍 Diagnostic

L'erreur vient probablement de Cursor qui essaie de faire un checkout mais a un problème avec la configuration.

## 🚀 Solutions

### Solution 1: Ignorer l'erreur Cursor (recommandé)

L'erreur est dans Cursor, pas Git. Tu peux continuer à utiliser Git en ligne de commande:

```bash
cd /home/tor/wkspaces/mo2

# Vérifier que tu es sur dev
git branch --show-current
# Doit afficher: dev

# Si pas sur dev, changer manuellement
git checkout dev

# Vérifier les fichiers
ls -la .cursor/agents/thebestclient5.md
ls -la .github/KEYS.txt
```

### Solution 2: Réinitialiser la configuration Cursor

Si Cursor continue à donner des erreurs:

1. **Fermer Cursor complètement**
2. **Réouvrir le dossier** `/home/tor/wkspaces/mo2`
3. **Utiliser Git en ligne de commande** pour les opérations

### Solution 3: Vérifier manuellement les fichiers

Même si Cursor a une erreur, les fichiers sont là. Vérifie:

```bash
cd /home/tor/wkspaces/mo2

# Vérifier l'agent
cat .cursor/agents/thebestclient5.md | head -10

# Vérifier KEYS.txt
cat .github/KEYS.txt | head -5

# Vérifier les scripts
ls -la setup-complete.sh cleanup-branches.sh
```

### Solution 4: Créer les fichiers manuellement (si manquants)

Si certains fichiers manquent sur ta machine:

```bash
cd /home/tor/wkspaces/mo2

# Créer KEYS.txt
mkdir -p .github
cat > .github/KEYS.txt << 'EOF'
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
EOF

# Vérifier
cat .github/KEYS.txt
```

## 📋 Commandes Git essentielles

```bash
# Voir la branche actuelle
git branch --show-current

# Voir toutes les branches
git branch -a

# Changer de branche (si nécessaire)
git checkout dev

# Récupérer les derniers changements
git fetch origin
git pull origin dev

# Voir les fichiers modifiés
git status

# Voir les commits récents
git log --oneline -10
```

## ✅ Vérification finale

Exécute ces commandes pour vérifier que tout est OK:

```bash
cd /home/tor/wkspaces/mo2

echo "Branche: $(git branch --show-current)"
echo "KEYS.txt: $(test -f .github/KEYS.txt && echo '✅' || echo '❌')"
echo "Agent: $(test -f .cursor/agents/thebestclient5.md && echo '✅' || echo '❌')"
echo "Scripts: $(ls -1 setup-complete.sh cleanup-branches.sh 2>/dev/null | wc -l)"
```

## 🎯 Action principale

**L'erreur Cursor peut être ignorée**. Utilise Git en ligne de commande:

1. Vérifie que tu es sur `dev`: `git branch --show-current`
2. Vérifie que KEYS.txt existe: `cat .github/KEYS.txt`
3. Ajoute la clé dans GitHub Secrets (interface web)
4. Le système fonctionnera automatiquement!

L'erreur Cursor n'empêche pas Git de fonctionner.
