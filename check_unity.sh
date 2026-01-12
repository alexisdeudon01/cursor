#!/bin/bash

# 1. Trouver le binaire Unity installé
echo "--- Recherche de l'exécutable Unity ---"
UNITY_PATH=$(which unity-editor 2>/dev/null || which unity 2>/dev/null || find /opt /home/$USER/Unity -name "Unity" -type f -executable -print -quit 2>/dev/null)

if [ -z "$UNITY_PATH" ]; then
    echo "ERREUR : Unity n'a pas été trouvé dans le PATH ou dans /opt."
else
    INSTALLED_VERSION=$($UNITY_PATH -version | head -n 1)
    echo "Installé : $INSTALLED_VERSION ($UNITY_PATH)"
fi

# 2. Vérifier la version requise par le projet actuel
echo -e "\n--- Analyse du projet Unity (répertoire courant) ---"
if [ -f "ProjectSettings/ProjectVersion.txt" ]; then
    PROJECT_VERSION=$(grep "m_EditorVersion:" ProjectSettings/ProjectVersion.txt | cut -d' ' -f2)
    echo "Requis par le projet : $PROJECT_VERSION"

    # 3. Comparaison
    if [ "$INSTALLED_VERSION" == "$PROJECT_VERSION" ]; then
        echo -e "\n✅ MATCH : La version installée correspond au projet."
    else
        echo -e "\n⚠️ ATTENTION : La version installée ($INSTALLED_VERSION) diffère de celle du projet ($PROJECT_VERSION)."
    fi
else
    echo "ERREUR : Aucun dossier de projet Unity détecté ici (ProjectSettings/ProjectVersion.txt introuvable)."
fi
