# AI-DDA (Artificial Intelligence Dynamic Difficulty Adjustment)

This project explores the integration of machine learning techniques, specifically through Unity ML-Agents and potentially other AI models, to dynamically adjust the difficulty in an RPG game environment.

---

## **Project Overview**

### **Objective**
The primary goal of this project is to design and implement an AI system capable of:
- Adapting game difficulty dynamically to suit the player's skill level and preferences.
- Enhancing player engagement by providing a tailored gaming experience.
- Leveraging advanced AI techniques for real-time player modeling and difficulty adjustment.

### **Current Status**
The project is built upon a heavily modified version of the **ARPG Project** asset and utilizes the **Gaia Pro 2023** terrain generation asset for an immersive environment. The game prototype currently focuses on integrating machine learning systems for AI-driven adaptations.

#### ✔ **Completed**

- **AI Agent Implementation**: AI can explore, interact with NPCs, fight enemies, and discover zones and waypoints.
- **Data Logging & Player Modeling**: System logs player and AI behavior (kills, deaths, interactions, exploration, etc.).
- **Bartle’s Player Type Classification**: AI classifies players based on behavior (Achiever, Explorer, Socializer, Killer).
- **ML-Agents Integration**: AI functions within Unity ML-Agents for behavior learning.
- **Final DDA Implementation**: Prototype system for difficulty adjustment is in place.

#### 🚧 **In Progress**

- **Finalizing DDA Implementation**: AI should dynamically adjust difficulty based on player behavior.
- **Training AI Based on Player Data**: AI should learn from real players' behavior to adjust difficulty effectively.
- **Experiment Setup & Testing**: Two test groups:
Group A (DDA ON) – AI adjusts difficulty dynamically
Group B (DDA OFF) – Static difficulty
- **Optimizing Enemy AI Behavior**: Ensuring enemies behave correctly when AI Agent interacts with them.
- **Refinement of Player Modeling & Difficulty Scaling**

---

## **Technical Details**

### **Technology Stack**
- **Game Engine**: Unity (URP 6+)
- **ML Toolkit**: Unity ML-Agents
- **Assets**:
  - ARPG Project (heavily modified to fit the purpose of the project)
  - Gaia Pro 2023 (terrain generation and environment design)
- **Programming Languages**: C# (Unity), Python (for ML model training)

### **Core Components**
- **Dynamic Difficulty Adjustment (DDA)**: AI adapts enemy strength, spawn rates, and mechanics based on the player.
- **Player Modeling**: AI analyzes player interactions to classify playstyle.
- **Bartle’s Player Types**: Comparison of player-declared vs. dynamically classified types.
- **ML-Agents Training**: AI learns to adjust game mechanics based on real player data.
- **Experimental Validation**: AI's impact on player experience is tested with two groups of participants.

---

## **How to Contribute**
This project is part of an academic research effort and is not open to external contributions at the moment. However, feedback and suggestions are always welcome!

You can reach me through my official [Contact Page](https://tr4spy.co.uk/portfolio/kontakt/) for any questions or discussions about the project.

---

## **License**
This project is for academic purposes only. It includes third-party assets (e.g., ARPG Project, Gaia Pro 2023) that are subject to their respective licenses.
