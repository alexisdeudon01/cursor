#!/usr/bin/env python3
"""
Génère des diagrammes UML (Mermaid) et les exporte en PNG.
Utilisé à chaque itération d'amélioration continue.
"""

import os
import json
import subprocess
import re
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Optional

# Configuration
DIAGRAMS_DIR = Path(".cursor/agents/diagrams")
PROJECT_ROOT = Path(".")

def ensure_diagrams_dir():
    """Crée le dossier des diagrammes si nécessaire."""
    DIAGRAMS_DIR.mkdir(parents=True, exist_ok=True)

def generate_architecture_diagram(version: int) -> Dict[str, str]:
    """Génère un diagramme d'architecture."""
    diagram = f"""```mermaid
classDiagram
    class NetworkManager {{
        +UnityTransport transport
        +NetworkConfig config
    }}
    
    class SessionRpcHub {{
        +static Instance
        +OnNetworkSpawn()
        +ServerRpc methods
        +ClientRpc methods
    }}
    
    class GameRegistry {{
        +static AllGames
        +static Register()
        +static GetGame()
    }}
    
    class SessionContainer {{
        +string sessionName
        +Vector3 worldOffset
        +GameInstanceManager gameManager
    }}
    
    class IGameDefinition {{
        <<interface>>
        +string GameId
        +MapConfigData CreateMapConfig()
        +Vector3 GetSpawnPosition()
    }}
    
    class NetworkingShared {{
        <<assembly>>
        SessionRpcHub
        DTOs
        Interfaces
    }}
    
    class NetworkingServer {{
        <<assembly>>
        ServerBootstrap
        ConnectionController
    }}
    
    class NetworkingClient {{
        <<assembly>>
        ClientBootstrap
        UI Components
    }}
    
    class Core {{
        <<assembly>>
        GameRegistry
        SessionContainer
        IGameDefinition
    }}
    
    NetworkManager --> SessionRpcHub
    SessionRpcHub --> GameRegistry
    SessionRpcHub --> SessionContainer
    SessionContainer --> IGameDefinition
    GameRegistry --> IGameDefinition
    
    NetworkingServer ..> NetworkingShared : references
    NetworkingClient ..> NetworkingShared : references
    NetworkingShared ..> Core : references
```
"""
    
    diagram_file = DIAGRAMS_DIR / f"architecture-v{version}.mmd"
    diagram_file.write_text(diagram, encoding='utf-8')
    
    return {
        "type": "architecture",
        "mermaid": diagram_file,
        "description": "Architecture générale du système"
    }

def generate_modularity_diagram(version: int) -> Dict[str, str]:
    """Génère un diagramme de modularité."""
    diagram = f"""```mermaid
graph TD
    A[GameRegistry] --> B[IGameDefinition]
    B --> C[SquareGameDefinition]
    B --> D[CircleGameDefinition]
    B --> E[NewGameDefinition]
    
    F[MapSystem] --> G[GridMapAsset]
    G --> H[MapConfigData]
    
    I[SessionSystem] --> J[GameSession]
    J --> K[SessionContainer]
    K --> L[GameInstanceManager]
    
    M[Resources/Games/] --> A
    N[Resources/Maps/] --> F
    
    style E fill:#90EE90
    style H fill:#87CEEB
    style L fill:#FFB6C1
```
"""
    
    diagram_file = DIAGRAMS_DIR / f"modularity-v{version}.mmd"
    diagram_file.write_text(diagram, encoding='utf-8')
    
    return {
        "type": "modularity",
        "mermaid": diagram_file,
        "description": "Système modulaire (jeux, maps, sessions)"
    }

def generate_client_server_diagram(version: int) -> Dict[str, str]:
    """Génère un diagramme Client/Serveur."""
    diagram = f"""```mermaid
graph LR
    subgraph Server["🖥️ Serveur (Server.unity)"]
        SM[ServerBootstrap]
        SR[SessionRpcHub]
        SC[SessionContainer]
        GM[GameInstanceManager]
        GR[GameRegistry]
    end
    
    subgraph Client["💻 Client (Client.unity, Game.unity)"]
        CB[ClientBootstrap]
        UI[UI Components]
        VC[View Components]
    end
    
    subgraph Shared["📦 Shared (Networking.Shared)"]
        DTO[DTOs]
        IF[Interfaces]
        EN[Enums]
    end
    
    SM --> SR
    SR --> SC
    SC --> GM
    GM --> GR
    
    CB --> UI
    UI --> VC
    
    SR -.->|uses| Shared
    CB -.->|uses| Shared
    SM -.->|uses| Shared
    
    SR <-->|RPC| CB
    
    style Server fill:#FF6B6B
    style Client fill:#4ECDC4
    style Shared fill:#FFE66D
```
"""
    
    diagram_file = DIAGRAMS_DIR / f"client-server-v{version}.mmd"
    diagram_file.write_text(diagram, encoding='utf-8')
    
    return {
        "type": "client-server",
        "mermaid": diagram_file,
        "description": "Séparation Client/Serveur"
    }

def convert_mermaid_to_png(mermaid_file: Path, output_png: Path) -> bool:
    """Convertit un fichier Mermaid en PNG."""
    try:
        # Utiliser mermaid-cli si disponible, sinon utiliser API en ligne
        result = subprocess.run(
            ["which", "mmdc"],
            capture_output=True,
            text=True
        )
        
        if result.returncode == 0:
            # mermaid-cli installé localement
            subprocess.run(
                ["mmdc", "-i", str(mermaid_file), "-o", str(output_png)],
                check=True
            )
            return True
        else:
            # Utiliser API en ligne (mermaid.ink)
            mermaid_content = mermaid_file.read_text(encoding='utf-8')
            # Extraire le contenu mermaid (sans les backticks)
            mermaid_code = re.sub(r'```mermaid\n', '', mermaid_content)
            mermaid_code = re.sub(r'```\n?$', '', mermaid_code)
            
            # Encoder pour URL
            import urllib.parse
            encoded = urllib.parse.quote(mermaid_code)
            url = f"https://mermaid.ink/img/{encoded}"
            
            # Télécharger l'image
            import requests
            response = requests.get(url, timeout=30)
            if response.status_code == 200:
                output_png.write_bytes(response.content)
                return True
    except Exception as e:
        print(f"⚠️ Erreur conversion Mermaid → PNG: {e}")
        return False
    
    return False

def generate_all_diagrams(version: int) -> List[Dict]:
    """Génère tous les diagrammes UML pour une version."""
    ensure_diagrams_dir()
    
    diagrams = []
    
    # Générer les diagrammes Mermaid
    arch_diag = generate_architecture_diagram(version)
    mod_diag = generate_modularity_diagram(version)
    cs_diag = generate_client_server_diagram(version)
    
    diagrams.extend([arch_diag, mod_diag, cs_diag])
    
    # Convertir en PNG
    print("🖼️ Conversion des diagrammes Mermaid en PNG...")
    for diagram in diagrams:
        mermaid_file = diagram["mermaid"]
        png_file = mermaid_file.with_suffix('.png')
        
        if convert_mermaid_to_png(mermaid_file, png_file):
            diagram["png"] = png_file
            print(f"  ✅ {diagram['type']}: {png_file.name}")
        else:
            print(f"  ⚠️ {diagram['type']}: PNG non généré (Mermaid disponible)")
    
    return diagrams

def create_diagrams_summary(version: int, diagrams: List[Dict]) -> Path:
    """Crée un fichier récapitulatif des diagrammes."""
    summary = f"""# Diagrammes UML - Version {version}
**Date**: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}

## Diagrammes générés

"""
    
    for diagram in diagrams:
        summary += f"### {diagram['type'].capitalize()}\n"
        summary += f"**Description**: {diagram['description']}\n\n"
        
        if "mermaid" in diagram:
            summary += f"**Fichier Mermaid**: `{diagram['mermaid'].name}`\n"
        if "png" in diagram:
            summary += f"**Fichier PNG**: `{diagram['png'].name}`\n"
        
        summary += "\n"
    
    summary_file = DIAGRAMS_DIR / f"diagrams-v{version}.md"
    summary_file.write_text(summary, encoding='utf-8')
    
    return summary_file

def main():
    """Fonction principale."""
    import sys
    version = int(sys.argv[1]) if len(sys.argv) > 1 else 3
    
    print(f"📊 Génération des diagrammes UML pour version {version}...")
    
    diagrams = generate_all_diagrams(version)
    summary = create_diagrams_summary(version, diagrams)
    
    print(f"✅ {len(diagrams)} diagramme(s) généré(s)")
    print(f"📄 Résumé: {summary}")

if __name__ == "__main__":
    main()
