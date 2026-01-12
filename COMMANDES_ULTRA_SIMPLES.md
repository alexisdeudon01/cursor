# 🎯 Commandes Ultra Simples - Copier-Coller

## Option 1: Script automatique (RECOMMANDÉ)

```bash
cd /home/tor/wkspaces/mo2
curl -s https://raw.githubusercontent.com/alexisdeudon01/cursor/dev/verify-everything.sh > verify-everything.sh
chmod +x verify-everything.sh
./verify-everything.sh
```

## Option 2: Commandes manuelles étape par étape

### Étape 1: Aller dans le projet
```bash
cd /home/tor/wkspaces/mo2
```

### Étape 2: Vérifier Git
```bash
git status
git branch --show-current
```

### Étape 3: Récupérer depuis GitHub
```bash
git fetch origin
git checkout dev
git pull origin dev
```

### Étape 4: Créer KEYS.txt
```bash
mkdir -p .github
cat > .github/KEYS.txt << 'END'
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
END
```

### Étape 5: Vérifier
```bash
ls -la .github/KEYS.txt
ls -la .cursor/agents/thebestclient5.md
```

## Option 3: Tout en une commande

```bash
cd /home/tor/wkspaces/mo2 && git fetch origin && git checkout dev && git pull origin dev && mkdir -p .github && cat > .github/KEYS.txt << 'END'
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
END
echo ".github/KEYS.txt" >> .gitignore && echo "✅ Terminé!"
```

## Vérification finale

```bash
echo "Branche: $(git branch --show-current)"
echo "KEYS.txt: $(test -f .github/KEYS.txt && echo '✅' || echo '❌')"
echo "Agent: $(test -f .cursor/agents/thebestclient5.md && echo '✅' || echo '❌')"
```

## Si rien ne fonctionne

1. Vérifie que tu es dans le bon dossier: `pwd`
2. Vérifie que Git fonctionne: `git --version`
3. Vérifie la connexion: `git ls-remote origin`
4. Si le repo est vide, clone-le: `git clone https://github.com/alexisdeudon01/cursor.git mo2`
