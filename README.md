
<p align="center">
  <img src="https://img.shields.io/badge/Tests-Passing-brightgreen?style=flat-square" alt="Tests" />
  <img src="https://img.shields.io/badge/Version-0.60B%2B-informational?style=flat-square" alt="Version" />
  <img src="https://img.shields.io/badge/Build-June%202025-lightgrey?style=flat-square" alt="Build" />
  <img src="https://img.shields.io/badge/Unity-6000.0.25f1-blue?style=flat-square" alt="Unity" />
  <img src="https://img.shields.io/badge/License-Restricted-red?style=flat-square" alt="License" />
</p>

# AI-DDA (Artificial Intelligence Dynamic Difficulty Adjustment)

This project explores the integration of machine learning techniques, specifically through Unity ML-Agents and custom AI models, to dynamically adjust the difficulty in an RPG game environment. The system also incorporates player behavior modeling and psychological player profiling (Bartle's Player Types) as part of an academic research project at the Master's degree in Computer Science at University of Bath.

---

## **Project Overview**

### **Objective**
The primary goal of this project is to design and implement an AI system capable of:
- Dynamically adjusting game difficulty to suit the player's skill level and playstyle.
- Enhancing player engagement by providing a personalized gaming experience.
- Utilizing advanced AI techniques for real-time player modeling and difficulty scaling.
- Integrating player profiling based on Bartle's Player Types taxonomy.

### **Current Status**

The project is built upon a heavily modified version of the **ARPG Project** asset and utilizes the **Gaia Pro 2023** terrain generation asset to create an immersive game environment. The prototype focuses on combining player behavioral analysis with AI-driven adaptive difficulty.

#### ✔ **Completed**

- **AI Agent Implementation**: AI can explore, interact with NPCs, engage in combat, and navigate waypoints.
- **Data Logging & Player Modeling**: Comprehensive system logs player behavior (kills, deaths, interactions, exploration).
- **Bartle’s Player Type Classification**: Initial player survey and dynamic behavioral analysis determine the player's evolving profile (Achiever, Explorer, Socializer, Killer).
- **ML-Agents Integration**: AI operates within Unity ML-Agents for behavioral learning.
- **AI Model Integration**: MLP (ONNX) and Reinforcement Learning (ONNX integrated with ML-Agents) models predict and fine-tune difficulty.
- **Experimental Validation**: Controlled experiments conducted with player groups to evaluate AI-DDA effectiveness.

#### 🚧 **In Progress**

- **AI System Optimization**: Improving enemy AI behaviors and responsiveness to difficulty adjustments.
- **Final Documentation and Academic Publication**: Preparing materials for thesis submission and publication of experimental results.
- **Gameplay Expansion**: Further development of game mechanics for extended testing scenarios.
- **Data Analysis & Refinement**: Post-experiment data analysis to refine player modeling and difficulty scaling.

---

## **Technical Details**

### **Technology Stack**
- **Game Engine**: Unity (URP 6+)
- **ML Toolkit**: Unity ML-Agents, Unity Sentis (ONNX integration)
- **Assets**:
  - ARPG Project (custom-modified for research purposes)
  - Gaia Pro 2023 (procedural terrain generation and environment design)
- **Programming Languages**: 
  - C# (Unity game logic)
  - Python (training AI models and data processing)

### **AI Models**
- **Multilayer Perceptron (MLP) — ONNX format**: Performs initial difficulty prediction based on real-time player behavior data.
- **Reinforcement Learning (RL) — ONNX format**: Fine-tunes difficulty dynamically in response to player performance and actions.
- Both models collaborate for layered, adaptive scaling of game difficulty.

### **Core Components**
- **Dynamic Difficulty Adjustment (DDA)**: AI adapts enemy stats, spawn rates, and mechanics based on player behavior and AI predictions.
- **Player Behavior Logging**: Real-time tracking of player actions (kills, deaths, potion use, quest completion, exploration).
- **Bartle’s Player Type Profiling**: Combines initial questionnaire with continuous in-game behavioral analysis to determine player type.
- **AI-Driven Game Adjustment**: Game world adapts dynamically in response to AI difficulty predictions.
- **Experimental Data Collection**: System logs player behavior and AI activity for academic analysis.

---

## **Experiment Design**

The experimental phase of the project has been completed, with data collected from player groups under controlled conditions:

- **Group A (DDA ON)**: AI dynamically adjusts difficulty throughout gameplay.
- **Group B (DDA OFF)**: Static, pre-defined difficulty setting.

Data collection included:
- Real-time player activity logs.
- AI decision-making records.
- Comparison of declared vs. dynamically inferred player types.
- In-game performance metrics.

The focus now shifts to final data analysis, optimization, and preparation for academic publication.

---

## **Training the AI Models**

Before training, install [Unity ML-Agents](https://github.com/Unity-Technologies/ml-agents) and ensure your Python environment meets the package requirements. The Unity project must also have the ML-Agents package enabled.

Two example trainer configurations are provided:

- `config/ppo/AI_TestAgent.yaml` – PPO setup for the prototype AI agent. **(Currently not used / used previously to test AI companion).**
- `config/rlmodel.yaml` – parameters for the difficulty adjustment model.

To launch training, run `mlagents-learn` with the desired configuration. The commands below assume you have a built training environment or run from the Unity Editor:

```bash
# Train the test agent
mlagents-learn config/ppo/AI_TestAgent.yaml --run-id=AI_TestAgent --train

# Train the reinforcement learning difficulty model
mlagents-learn config/rlmodel.yaml --run-id=RLModel --train
```

Training results, including TensorBoard logs and the exported `.onnx` files, are written to the `results/<run-id>` directory. After completion, copy the generated ONNX models into `Assets/StreamingAssets/` so the game can load them at runtime.



## **In-Game Command Reference**

| Command | Description |
|---------|-------------|
| `/help` | Displays a list of available commands. |
| `/help [command]` | Displays detailed help for the specified command. |
| `/money [amount]` | Adds the specified amount of gold to the player. |
| `/drop [group] [id] [level] [attr] [durability]` | Spawns an item with the specified parameters. |
| `/tp x y z` | Teleports the player to the given coordinates. |
| `/wp [index]` | Teleports the player to the waypoint at the specified index. |
| `/listwp` | Lists all available waypoints in the current scene. |
| `/whereami` | Displays the player's current coordinates. |
| `/summon [enemyId]` | Spawns an enemy with the specified ID. |
| `/heal` | Fully restores the player's health and mana. |
| `/kill` | Kills the player character. |
| `/addexp [value]` | Adds the specified amount of experience points. |
| `/lvlup` | Increases the player's level by one. |
| `/godmode` | Toggles invincibility mode for the player. |
| `/time [day, night]` | Changes the time of day in-game. |
| `/weather [sun, rain, snow]` | Changes the in-game weather. |
| `/dda log` | Displays the AI-DDA activity log. |
| `/dda reset` | Resets the AI-DDA data for the current player session. |
| `/dda export` | Exports player data to CSV for analysis. |
| `/dda force` | Forces an immediate recalculation of difficulty. |
| `/dda type` | Displays the dynamically estimated player type. |
| `/dda diff [value]` | Manually sets the difficulty level. |
| `/dda achievements` | Lists unlocked achievements. |
| `/dda quests` | Lists completed quests. |
| `/clear` | Clears the in-game chat log. |

---

## **How to Contribute**
This project is part of an academic research effort and is not open to external contributions at the moment. However, feedback and suggestions are always welcome!

You can reach me through my official [Contact Page](https://tr4spy.com/portfolio/kontakt/) for any questions or discussions about the project.

---

## **License**
This project is for academic purposes only. It includes third-party assets (e.g., ARPG Project, Gaia Pro 2023) that are subject to their respective licenses.
