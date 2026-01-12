# 🎯 Commandes Ultime - La Plus Simple

## Une seule commande (copier-coller)

```bash
cd /home/tor/wkspaces/mo2 && git fetch origin && git checkout dev && git pull origin dev && mkdir -p .github && cat > .github/KEYS.txt << 'END'
ANTHROPIC_API_KEY=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
SSH_PRIVATE_KEY=b3BlbnNzaC1rZXktdjEAAAAACmFlczI1Ni1jdHIAAAAGYmNyeXB0AAAAGAAAABBrWlGBzGysO3xsV6UFaOjNAAAAGAAAAAEAAAAzAAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZqAAAAoJkqxhUPrbIS6wxVa64SV89zuHnm3vpSZlPqMC53ivTQj2lHzOVaYHWrdeKup6GYPxqjx4S5zN9JzAIA9ZDw/Tk2S8JC72iouJ/SaSFHrRLwFrsafkiRX35q0IccCHANZKtlSdcb52ZGRpzSykxw9LRno+FjnfCviM+imkrQiIOlLRnl+FW3ZXCkJ+/D2Oj9bOXBA8r+/k+FB6Zk/59BJaI=
SSH_PUBLIC_KEY=ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIGmC9hs4sioYyFFC9C8t6qBmk3jBgTFySFYV7DkZAEZq alexisdeudon01@gmail.com
SSH_PASSPHRASE=alexis
END
echo ".github/KEYS.txt" >> .gitignore && echo "✅ TERMINÉ! Vérification:" && ls -la .github/KEYS.txt && git branch --show-current
```

## Ou utiliser le script

```bash
cd /home/tor/wkspaces/mo2
chmod +x setup-tout.sh
./setup-tout.sh
```

## Vérification finale

```bash
echo "KEYS.txt: $(test -f .github/KEYS.txt && echo '✅' || echo '❌')"
echo "Branche: $(git branch --show-current)"
echo "Agent: $(test -f .cursor/agents/thebestclient5.md && echo '✅' || echo '❌')"
```

## Si erreur sur lignes 771-777

Ces lignes n'existent dans aucun fichier. Si tu vois une erreur:
1. Dis-moi le **nom du fichier**
2. Dis-moi l'**erreur exacte**
3. Je corrigerai directement

## Action finale

Ajouter la clé dans GitHub Secrets:
https://github.com/alexisdeudon01/cursor/settings/secrets/actions
