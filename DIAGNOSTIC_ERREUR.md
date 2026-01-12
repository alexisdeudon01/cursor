# 🔍 Diagnostic de l'erreur

## Erreur rencontrée
```
Failed to checkout branch: Cannot read properties of undefined (reading 'uri')
```

Cette erreur semble être liée à l'IDE Cursor, pas à Git directement.

## Vérifications à faire

### 1. Vérifier l'état Git

```bash
cd /home/tor/wkspaces/mo2
git status
git branch --show-current
```

### 2. Vérifier les remotes

```bash
git remote -v
```

Si pas de remote, ajouter:
```bash
git remote add origin https://github.com/alexisdeudon01/cursor.git
```

### 3. Vérifier les branches

```bash
git branch -a
```

### 4. Forcer la récupération

```bash
git fetch origin --all
```

### 5. Créer/créer la branche dev localement

```bash
# Si dev n'existe pas localement
git checkout -b dev origin/dev

# Ou créer depuis main
git checkout -b dev
```

## Solution alternative: Cloner le repo

Si les problèmes persistent, cloner le repo:

```bash
cd /home/tor/wkspaces
git clone https://github.com/alexisdeudon01/cursor.git mo2-new
cd mo2-new
git checkout dev
```

## Vérifier les fichiers manuellement

Même si Git ne fonctionne pas, tu peux créer les fichiers manuellement:

```bash
cd /home/tor/wkspaces/mo2

# Créer .github/KEYS.txt
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
