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
# print(f"DEBUG: Received {len(sys.argv)-1} arguments from Unity: {sys.argv[1:]}", file=sys.stderr)
# if len(sys.argv) < 11:  # Sprawdzamy, czy są wszystkie 10 cech + nazwa pliku
#     print("Error: Not enough arguments received from Unity.", file=sys.stderr)
#     sys.exit(1)
# print(f"Received {len(sys.argv)-1} arguments from Unity: {sys.argv[1:]}", file=sys.stderr)

try:
    total_play_time = float(sys.argv[1].replace(",", "."))
    player_deaths = int(sys.argv[2])
    enemies_defeated = int(sys.argv[3])
    total_combat_time = float(sys.argv[4].replace(",", "."))
    npc_interactions = int(sys.argv[5])
    potions_used = int(sys.argv[6])
    zones_discovered = int(sys.argv[7])
    quests_completed = int(sys.argv[8])
    waypoints_discovered = int(sys.argv[9])
    unlocked_achievements = int(sys.argv[10])
except ValueError as e:
    print(f"Error parsing arguments: {e}", file=sys.stderr)
    sys.exit(1)

# Przygotowanie danych do trudności (Unity Input)
feature_names = ["Total Play Time", "Player Deaths", "Enemies Defeated", "Total Combat Time",
                 "NPC Interactions", "Potions Used", "Zones Discovered", "Quests Completed",
                 "Waypoints Discovered", "Unlocked Achievements"]

# Tworzymy wejście dla XGBoost (musi być 2D)
input_data = np.array([[total_play_time, player_deaths, enemies_defeated, total_combat_time, npc_interactions,
                         potions_used, zones_discovered, quests_completed, waypoints_discovered, 
                         unlocked_achievements]])

# Sprawdzamy, czy model dostał poprawne dane
if input_data.shape[1] != 10:
    print(f"Error: Incorrect number of input features! Expected 10, got {input_data.shape[1]}", file=sys.stderr)
    sys.exit(1)

# Predykcja trudności
try:
    predicted_difficulty = model.predict(input_data)[0]
    print(predicted_difficulty, flush=True)  # Unity odczyta tylko wynik
except Exception as e:
    print(f"Error during prediction: {e}", file=sys.stderr)
    sys.exit(1)

