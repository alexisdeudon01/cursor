# 🔍 Script de Vérification Complète

## 🚀 Utilisation

### Sur ta machine locale

```bash
cd /home/tor/wkspaces/mo2

# Télécharger le script depuis GitHub
curl -s https://raw.githubusercontent.com/alexisdeudon01/cursor/dev/verify-everything.sh > verify-everything.sh
chmod +x verify-everything.sh

# Exécuter
./verify-everything.sh
```

### Ou copier-coller directement

```bash
cd /home/tor/wkspaces/mo2
bash <(curl -s https://raw.githubusercontent.com/alexisdeudon01/cursor/dev/verify-everything.sh)
```

## 📋 Ce que le script vérifie

1. ✅ **Dossier du projet** - Existe et accessible
2. ✅ **Repository Git** - Initialisé et configuré
3. ✅ **Remote GitHub** - Configuré et accessible
4. ✅ **Branche dev** - Existe et est active
5. ✅ **Fichiers clés** - Agent, KEYS.txt, scripts
6. ✅ **.gitignore** - KEYS.txt est ignoré
7. ✅ **Commits** - Historique présent
8. ✅ **Connexion GitHub** - Fonctionne
9. ✅ **Fichiers manquants** - Crée automatiquement

## 🔧 Ce que le script fait automatiquement

- Crée KEYS.txt si manquant
- Configure le remote si manquant
- Crée la branche dev si manquante
- Ajoute KEYS.txt à .gitignore
- Télécharge les fichiers manquants depuis GitHub

## 📊 Résultat

Le script affiche un résumé complet avec:
- ✅ Ce qui est OK
- ❌ Ce qui manque
- ⚠️  Ce qui nécessite attention

## 🎯 Après exécution

1. Vérifie le résumé affiché
2. Corrige les problèmes identifiés
3. Ajoute la clé dans GitHub Secrets
4. Le système fonctionnera automatiquement!

---

**Le script est sur GitHub dans la branche dev!**
