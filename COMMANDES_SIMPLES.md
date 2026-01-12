# 🎯 Commandes Simples - Copier-Coller

## Commandes à exécuter dans ton terminal

### 1. Vérifier où tu es

```bash
cd /home/tor/wkspaces/mo2
pwd
git branch --show-current
```

### 2. Récupérer les changements

```bash
git fetch origin
git pull origin dev
```

### 3. Vérifier les fichiers

```bash
ls -la .cursor/agents/thebestclient5.md
ls -la .github/KEYS.txt
ls -la setup-complete.sh
```

### 4. Créer KEYS.txt (si manquant)

```bash
mkdir -p .github
cat > .github/KEYS.txt << 'END'
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
END
```

### 5. Vérifier que tout est OK

```bash
echo "Branche: $(git branch --show-current)"
echo "KEYS.txt: $(test -f .github/KEYS.txt && echo '✅ Existe' || echo '❌ Manquant')"
echo "Agent: $(test -f .cursor/agents/thebestclient5.md && echo '✅ Existe' || echo '❌ Manquant')"
```

## ⚠️ Erreur Cursor

Si tu vois l'erreur "Cannot read properties of undefined (reading 'uri')":
- **C'est une erreur de Cursor IDE, pas Git**
- **Tu peux l'ignorer**
- **Utilise Git en ligne de commande** (les commandes ci-dessus)

## ✅ Une fois fait

1. Ajouter la clé dans GitHub Secrets (interface web)
2. Le système fonctionnera automatiquement!
