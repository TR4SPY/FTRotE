import sys
import numpy as np
import joblib
import pandas as pd
import os

# Pobranie folderu, w którym znajduje się `ai_model.py`
script_dir = os.path.dirname(os.path.abspath(__file__))  
model_path = os.path.join(script_dir, "ai_model.pkl")  

# Bezpieczne ładowanie modelu
try:
    model = joblib.load(model_path)
except Exception as e:
    print(f"Error loading model: {e}", file=sys.stderr)
    sys.exit(1)

# Pobieranie argumentów z Unity
if len(sys.argv) < 9:
    print("Error: Not enough arguments received from Unity.", file=sys.stderr)
    sys.exit(1)

try:
    player_deaths = int(sys.argv[1])
    enemies_defeated = int(sys.argv[2])
    total_combat_time = float(sys.argv[3].replace(",", "."))  # Zamiana ',' na '.'
    npc_interactions = int(sys.argv[4])
    quests_completed = int(sys.argv[5])
    waypoints_discovered = int(sys.argv[6])
    zones_discovered = int(sys.argv[7])
    unlocked_achievements = int(sys.argv[8])
except ValueError as e:
    print(f"Error parsing arguments: {e}")
    sys.exit(1)

# Przygotowanie danych do trudności (Unity Input)
feature_names = ["Player Deaths", "Enemies Defeated", "Total Combat Time", "NPC Interactions",
                 "Quests Completed", "Waypoints Discovered", "Zones Discovered", "Unlocked Achievements"]

input_data = pd.DataFrame([[player_deaths, enemies_defeated, total_combat_time, npc_interactions,
                            quests_completed, waypoints_discovered, zones_discovered, unlocked_achievements]],
                          columns=feature_names)

# Predykcja trudności
predicted_difficulty = model.predict(input_data)[0]
print(predicted_difficulty, flush=True)  # Unity odczyta tylko wynik