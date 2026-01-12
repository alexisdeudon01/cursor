#!/usr/bin/env node
/**
 * Script Node.js pour gérer les agents Cursor
 * Usage: node manage-agents.js [list|info|verify|create]
 */

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const AGENTS_DIR = path.join(__dirname, '..', 'agents');
const PROJECT_ROOT = path.join(__dirname, '..', '..');

class CursorAgentManager {
    constructor() {
        this.agentsDir = AGENTS_DIR;
    }

    listAgents() {
        if (!fs.existsSync(this.agentsDir)) {
            console.log('❌ Agents directory does not exist:', this.agentsDir);
            return [];
        }

        const files = fs.readdirSync(this.agentsDir)
            .filter(f => f.endsWith('.md'))
            .map(f => {
                const filePath = path.join(this.agentsDir, f);
                const stats = fs.statSync(filePath);
                const content = fs.readFileSync(filePath, 'utf8');
                const lines = content.split('\n');
                
                // Extract description
                let description = 'No description';
                for (let i = 0; i < Math.min(20, lines.length); i++) {
                    if (lines[i].includes('Agent role') || lines[i].includes('description:')) {
                        if (i + 1 < lines.length) {
                            description = lines[i + 1].trim();
                            break;
                        }
                    }
                }

                return {
                    name: path.basename(f, '.md'),
                    file: f,
                    path: filePath,
                    size: stats.size,
                    lines: lines.length,
                    description: description,
                    modified: stats.mtime
                };
            });

        return files.sort((a, b) => a.name.localeCompare(b.name));
    }

    displayList() {
        const agents = this.listAgents();
        
        console.log('\n=== CURSOR AGENTS ===\n');
        
        if (agents.length === 0) {
            console.log('No agents found in:', this.agentsDir);
            return;
        }

        console.log(`Found ${agents.length} agent(s):\n`);
        
        agents.forEach((agent, index) => {
            console.log(`${index + 1}. ${agent.name}`);
            console.log(`   Description: ${agent.description}`);
            console.log(`   File: ${agent.file}`);
            console.log(`   Size: ${agent.size} bytes (${agent.lines} lines)`);
            console.log(`   Modified: ${agent.modified.toLocaleString()}`);
            console.log('');
        });
    }

    displayInfo(agentName) {
        const agents = this.listAgents();
        const agent = agents.find(a => a.name === agentName || a.file === agentName);
        
        if (!agent) {
            console.log(`❌ Agent not found: ${agentName}`);
            return;
        }

        console.log(`\n=== AGENT INFO: ${agent.name} ===\n`);
        console.log(`File: ${agent.file}`);
        console.log(`Path: ${agent.path}`);
        console.log(`Size: ${agent.size} bytes`);
        console.log(`Lines: ${agent.lines}`);
        console.log(`Modified: ${agent.modified.toLocaleString()}`);
        console.log(`\nDescription:\n${agent.description}\n`);
        
        // Show first 20 lines
        const content = fs.readFileSync(agent.path, 'utf8');
        const preview = content.split('\n').slice(0, 20).join('\n');
        console.log('Preview (first 20 lines):');
        console.log('─'.repeat(60));
        console.log(preview);
        console.log('─'.repeat(60));
    }

    verify() {
        console.log('\n=== VERIFYING CURSOR AGENT CONFIGURATION ===\n');
        
        const checks = [];
        
        // Check directory
        if (fs.existsSync(this.agentsDir)) {
            checks.push({ status: '✅', message: `Agents directory exists: ${this.agentsDir}` });
        } else {
            checks.push({ status: '❌', message: `Agents directory missing: ${this.agentsDir}` });
        }

        // Check agents
        const agents = this.listAgents();
        if (agents.length > 0) {
            checks.push({ status: '✅', message: `Found ${agents.length} agent file(s)` });
            agents.forEach(agent => {
                if (agent.size > 0) {
                    checks.push({ status: '  ✅', message: `  ${agent.name} (${agent.size} bytes)` });
                } else {
                    checks.push({ status: '  ⚠️', message: `  ${agent.name} is empty` });
                }
            });
        } else {
            checks.push({ status: '⚠️', message: 'No agent files found' });
        }

        checks.forEach(check => console.log(`${check.status} ${check.message}`));
        
        const allPassed = checks.every(c => !c.status.includes('❌') && !c.status.includes('⚠️'));
        console.log(allPassed ? '\n✅ All checks passed!' : '\n⚠️ Some issues found.');
    }

    createTemplate(name = 'new-agent-template') {
        const templatePath = path.join(this.agentsDir, `${name}.md`);
        
        if (!fs.existsSync(this.agentsDir)) {
            fs.mkdirSync(this.agentsDir, { recursive: true });
        }

        const template = `# Agent role
You are a Cursor agent specialized in [DESCRIBE YOUR AGENT'S PURPOSE].

# Mandatory context (must follow)
- [Add your mandatory context here]

# Target architecture
[Describe the target architecture]

# Implementation directives
1. [Directive 1]
2. [Directive 2]

# Coding conventions
- [Convention 1]
- [Convention 2]

# What the agent must produce
- [Output 1]
- [Output 2]
`;

        fs.writeFileSync(templatePath, template);
        console.log(`✅ Template created: ${templatePath}`);
    }
}

// CLI
const command = process.argv[2] || 'list';
const manager = new CursorAgentManager();

switch (command) {
    case 'list':
        manager.displayList();
        break;
    case 'info':
        const agentName = process.argv[3];
        if (!agentName) {
            console.log('Usage: node manage-agents.js info <agent-name>');
            process.exit(1);
        }
        manager.displayInfo(agentName);
        break;
    case 'verify':
        manager.verify();
        break;
    case 'create':
        const name = process.argv[3] || 'new-agent-template';
        manager.createTemplate(name);
        break;
    default:
        console.log('Usage: node manage-agents.js [list|info|verify|create] [options]');
        console.log('\nCommands:');
        console.log('  list              - List all agents');
        console.log('  info <name>       - Show detailed info about an agent');
        console.log('  verify            - Verify agent configuration');
        console.log('  create [name]     - Create a new agent template');
        process.exit(1);
}
