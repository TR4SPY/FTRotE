import json
import joblib
import numpy as np
import xgboost as xgb
from http.server import BaseHTTPRequestHandler, HTTPServer
import time

# Wczytanie modelu XGBoost
model_path = "ai_model.pkl"
model = joblib.load(model_path)

# Parametry RL
learning_rate = 0.1  # Jak bardzo AI dostosowuje trudność
difficulty = 5.0  # Początkowa trudność
inactive_penalty = -1.0  # Kara za brak aktywności
combat_threshold = 2  # Minimalna liczba zabitych przeciwników do wzrostu trudności
death_threshold = 3  # Jeśli gracz umiera 3+ razy, RL wymusza obniżenie trudności
potion_threshold = 5  # Nadużywanie mikstur zmniejsza trudność
npc_threshold = 3  # Min. liczba interakcji z NPC, żeby nie uznać gracza za nieaktywnego
time_decay = -0.5  # Kara za długą nieaktywność

# **Historia statystyk gracza**
last_kills = 0
last_npc_interactions = 0
last_update_time = time.time()

# **Historia trudności**
last_difficulty = difficulty  # Trzymamy poprzednią wartość trudności

class RLHandler(BaseHTTPRequestHandler):
    def do_POST(self):
        global difficulty, last_kills, last_npc_interactions, last_update_time, last_difficulty

        content_length = int(self.headers['Content-Length'])
        post_data = self.rfile.read(content_length)
        data = json.loads(post_data)

        print(f"[RL] Received Data: {data}")

        # Pobieranie statystyk
        total_play_time = data["totalPlayTime"]
        deaths = data["playerDeaths"]
        kills = data["enemiesDefeated"]
        combat_time = data["totalCombatTime"]
        npc_interactions = data["npcInteractions"]
        potions_used = data["potionsUsed"]
        quests_completed = data["questsCompleted"]
        waypoints_discovered = data["waypointsDiscovered"]
        zones_discovered = data["zonesDiscovered"]
        unlocked_achievements = data["unlockedAchievements"]

        # Obliczamy różnice w statystykach
        delta_kills = kills - last_kills
        delta_npc_interactions = npc_interactions - last_npc_interactions
        time_since_last_update = time.time() - last_update_time

        # Aktualizacja historii
        last_kills = kills
        last_npc_interactions = npc_interactions
        last_update_time = time.time()

        # Analiza aktywności gracza
        if delta_kills < combat_threshold and delta_npc_interactions < npc_threshold:
            print(f"[RL] Player inactive for {time_since_last_update:.1f}s, applying decay...")
            rl_adjustment = time_decay
        elif deaths >= death_threshold:
            print("[RL] Player is dying too often! Lowering difficulty...")
            rl_adjustment = -2
        elif potions_used > potion_threshold and delta_kills < combat_threshold:
            print("[RL] Player is overusing potions! Lowering difficulty slightly...")
            rl_adjustment = -0.5
        else:
            reward = (delta_kills * 3) - (deaths * 2.5) - (combat_time * 0.002) + (delta_npc_interactions * 2) + \
                     (quests_completed * 3) + (waypoints_discovered * 1.5) + (zones_discovered * 1.2) + \
                     (unlocked_achievements * 2)
            rl_adjustment = learning_rate * reward

        print(f"[RL] Final difficulty adjustment: {rl_adjustment}")

        # Predykcja trudności na podstawie modelu
        input_data = np.array([[total_play_time, deaths, kills, combat_time, npc_interactions,
                                potions_used, quests_completed, waypoints_discovered, zones_discovered, unlocked_achievements]])

        predicted_difficulty = model.predict(input_data)[0]
        predicted_difficulty = min(max(predicted_difficulty, 1), 10)
        print(f"[RL] Model predicted difficulty: {predicted_difficulty}")

        # Nowa trudność uwzględniająca RL
        final_difficulty = min(max(predicted_difficulty + rl_adjustment, 1), 10)
        print(f"[RL] Adjusted difficulty after RL correction: {final_difficulty}")

        # **Sprawdzamy, czy zmieniła się trudność**
        if final_difficulty != last_difficulty:
            # Aktualizacja modelu tylko jeśli trudność się zmieniła
            print("[RL] Difficulty changed, updating model...")
            new_label = np.array([final_difficulty])
            dtrain = xgb.DMatrix(input_data, label=new_label)
            model.fit(input_data, new_label)

            # Zapis modelu
            joblib.dump(model, model_path)
            print("[RL] Model updated and saved.")
            last_difficulty = final_difficulty  # Zaktualizuj ostatnią trudność
        else:
            print("[RL] Difficulty not changed, skipping model update.")

        # Odpowiedź do Unity – RL modyfikuje tylko XGBoost
        self.send_response(200)
        self.send_header("Content-type", "application/json")
        self.end_headers()
        response = json.dumps({"difficulty": final_difficulty - predicted_difficulty})

        self.wfile.write(response.encode())

# Uruchamianie serwera REST dla Unity
def run():
    server_address = ('', 5000)
    httpd = HTTPServer(server_address, RLHandler)
    print("[RL] Server running on port 5000...")
    httpd.serve_forever()

if __name__ == "__main__":
    run()
