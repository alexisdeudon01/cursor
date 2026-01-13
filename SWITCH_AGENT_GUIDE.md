# 🔄 Guide: Switcher vers le Nouvel Agent

## 📊 État Actuel

**Version actuelle disponible**: `thebestclient6`  
**Prochaine version**: `thebestclient7` (sera créée automatiquement)

## 🤔 Dois-tu switcher ?

### Pour toi (dans Cursor) : **OUI, mais pas encore**

**Actuellement** :
- `thebestclient6` existe et fonctionne
- `thebestclient7` n'existe **pas encore** (sera créé au prochain cycle GitHub Actions)

**Quand switcher** :
1. ✅ **Attendre** que le prochain cycle GitHub Actions crée `thebestclient7.md`
2. ✅ **Vérifier** que le fichier existe : `.cursor/agents/thebestclient7.md`
3. ✅ **Switcher** dans Cursor avec `@thebestclient7`

### Pour le workflow GitHub Actions : **NON, c'est automatique**

Le workflow détecte **automatiquement** la dernière version :
- Il scanne tous les fichiers `thebestclient*.md`
- Il utilise toujours la version la plus récente
- **Tu n'as rien à faire** ✅

## 🔄 Comment switcher (quand thebestclient7 sera créé)

### Méthode 1 : Via @ (Recommandé)

Dans le chat Cursor :

```
@thebestclient7
```

ou

```
@.cursor/agents/thebestclient7.md
```

### Méthode 2 : Vérifier d'abord

```bash
# Vérifier que thebestclient7 existe
ls -la .cursor/agents/thebestclient7.md

# Si oui, utiliser dans Cursor
@thebestclient7
```

## ⏰ Quand thebestclient7 sera créé

**Déclenchement automatique** :
- ✅ Toutes les 30 minutes (cron)
- ✅ Sur push vers `dev`
- ✅ Manuellement via GitHub Actions UI

**Après création** :
1. Le workflow GitHub Actions crée `thebestclient7.md`
2. Commit automatique sur `dev`
3. Push vers `origin/dev`
4. **Tu peux alors switcher** dans Cursor

## 📋 Checklist

### Avant de switcher
- [ ] Vérifier que `thebestclient7.md` existe
- [ ] Vérifier que le commit est sur `origin/dev`
- [ ] Faire `git pull origin dev` si nécessaire

### Pour switcher
- [ ] Dans Cursor, utiliser `@thebestclient7`
- [ ] Vérifier que l'agent répond avec sa description

## 🎯 Résumé

| Contexte | Action requise | Quand |
|----------|---------------|-------|
| **Cursor (toi)** | Switcher avec `@thebestclient7` | ✅ Après création par GitHub Actions |
| **GitHub Actions** | Rien à faire | ✅ Automatique (détecte dernière version) |

## 💡 Astuce

Pour savoir quand `thebestclient7` est créé :
1. Aller sur GitHub → **Actions**
2. Vérifier la dernière exécution de `Auto-Improve Project`
3. Voir le commit : `🤖 Auto-improve: Cycle ... - Thebestclient6 → Thebestclient7`
4. Alors tu peux switcher dans Cursor

---

**Réponse courte** : 
- **Pour toi (Cursor)** : Oui, mais **attendre** que `thebestclient7` soit créé par GitHub Actions
- **Pour GitHub Actions** : Non, c'est **automatique**
