# 🔧 Commandes Git - Ce que tu dois faire

## 📍 Étape 1: Vérifier où tu es

```bash
cd /home/tor/wkspaces/mo2
pwd
# Doit afficher: /home/tor/wkspaces/mo2

git branch --show-current
# Doit afficher: dev (ou une autre branche)
```

## 📥 Étape 2: Récupérer les changements depuis GitHub

```bash
# Récupérer toutes les branches et changements
git fetch origin

# Voir les branches disponibles
git branch -a

# Vérifier si tu es sur dev
git checkout dev

# Récupérer les derniers changements de dev
git pull origin dev
```

## 📋 Étape 3: Vérifier les fichiers

```bash
# Vérifier que l'agent existe
ls -la .cursor/agents/thebestclient*.md

# Vérifier les scripts
ls -la .github/scripts/*.py
ls -la setup-complete.sh cleanup-branches.sh

# Vérifier KEYS.txt (peut ne pas exister si pas encore créé localement)
ls -la .github/KEYS.txt
```

## 🔑 Étape 4: Créer KEYS.txt (si manquant)

```bash
# Créer le dossier si nécessaire
mkdir -p .github

# Créer le fichier KEYS.txt
cat > .github/KEYS.txt << 'EOF'
# ⚠️ FICHIER LOCAL - NE JAMAIS COMMITER
# Ce fichier contient les clés d'accès

# Clé API Anthropic
ANTHROPIC_API_KEY=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

# Clé SSH Privée
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=

# Clé SSH Publique
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com

# Passphrase SSH
SSH_PASSPHRASE=alexis
EOF

# Vérifier que le fichier est créé
cat .github/KEYS.txt | head -5
```

## ✅ Étape 5: Vérifier .gitignore

```bash
# Vérifier que KEYS.txt est dans .gitignore
grep "KEYS.txt" .gitignore

# Si pas présent, l'ajouter
echo ".github/KEYS.txt" >> .gitignore
```

## 🚀 Étape 6: Exécuter setup-complete.sh

```bash
# Rendre exécutable si nécessaire
chmod +x setup-complete.sh

# Exécuter
./setup-complete.sh
```

## 📊 Étape 7: Vérifier l'état final

```bash
# Vérifier la branche
git branch --show-current

# Vérifier les fichiers clés
ls -la .cursor/agents/thebestclient5.md
ls -la .github/KEYS.txt
ls -la setup-complete.sh cleanup-branches.sh

# Vérifier les commits récents
git log --oneline -5
```

## 🔍 Si les fichiers ne sont toujours pas là

```bash
# Vérifier le remote
git remote -v

# Forcer la récupération
git fetch origin --all

# Voir toutes les branches
git branch -a

# Essayer de récupérer depuis dev-clean-final si dev ne fonctionne pas
git fetch origin dev-clean-final
git checkout -b dev-clean-final origin/dev-clean-final
```

## 📝 Commandes complètes (copier-coller)

```bash
cd /home/tor/wkspaces/mo2
git fetch origin
git checkout dev
git pull origin dev
mkdir -p .github
cat > .github/KEYS.txt << 'EOF'
# ⚠️ FICHIER LOCAL - NE JAMAIS COMMITER
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
EOF
echo ".github/KEYS.txt" >> .gitignore
chmod +x setup-complete.sh cleanup-branches.sh 2>/dev/null || true
./setup-complete.sh
```
