import pandas as pd
import numpy as np

# ===== PARAMETRY GENERACJI =====
num_samples = 10000  # Ilość generowanych próbek
initial_difficulty = 5  # Startowy poziom trudności

# Zakres wartości dla różnych zmiennych
player_deaths_range = (0, 15)  # Ile razy gracz mógł umrzeć
enemies_defeated_range = (0, 200)  # Ile przeciwników pokonał
total_combat_time_range = (10, 5000)  # Czas walki w sekundach
potions_used_range = (0, 50)  # Liczba użytych mikstur
waypoints_discovered_range = (0, 20)  # Ilość odkrytych waypointów
zones_discovered_range = (0, 10)  # Ilość odkrytych stref
quests_completed_range = (0, 20)  # Liczba ukończonych misji
npc_interactions_range = (0, 50)  # Ile razy gracz wchodził w interakcję z NPC
achievements_unlocked_range = (0, 10)  # Liczba zdobytych osiągnięć

# ===== FUNKCJA OBLICZAJĄCA POZIOM TRUDNOŚCI =====
def calculate_difficulty(player_deaths, enemies_defeated, potions_used):
    difficulty = initial_difficulty
    difficulty -= player_deaths * 1.0  # Każdy zgon obniża trudność o 1
    difficulty += (enemies_defeated / 10)  # 10 zabitych podnosi trudność o 1
    difficulty -= (potions_used * 0.05)  # Każda mikstura obniża trudność o 0.05
    return max(1.0, min(10.0, difficulty))  # Trudność w zakresie 1-10

# ===== FUNKCJA OBLICZAJĄCA TYP GRACZA =====
def determine_dynamic_player_type(quests_completed, waypoints_discovered, zones_discovered, npc_interactions, enemies_defeated, total_combat_time, achievements_unlocked):
    achiever_score = (quests_completed * 2) + (achievements_unlocked * 2)
    explorer_score = (zones_discovered * 2) + waypoints_discovered
    socializer_score = (npc_interactions * 3) + quests_completed
    killer_score = (int(enemies_defeated * 0.5) + int(total_combat_time / 120))

    max_score = max(achiever_score, explorer_score, socializer_score, killer_score)

    if max_score == achiever_score:
        return "Achiever"
    elif max_score == explorer_score:
        return "Explorer"
    elif max_score == socializer_score:
        return "Socializer"
    elif max_score == killer_score:
        return "Killer"
    return "Undefined"

# ===== GENEROWANIE DANYCH =====
data = []
for _ in range(num_samples):
    player_deaths = np.random.randint(*player_deaths_range)
    enemies_defeated = np.random.randint(*enemies_defeated_range)
    total_combat_time = np.random.randint(*total_combat_time_range)
    potions_used = np.random.randint(*potions_used_range)
    waypoints_discovered = np.random.randint(*waypoints_discovered_range)
    zones_discovered = np.random.randint(*zones_discovered_range)
    quests_completed = np.random.randint(*quests_completed_range)
    npc_interactions = np.random.randint(*npc_interactions_range)
    achievements_unlocked = np.random.randint(*achievements_unlocked_range)

    # Obliczamy poziom trudności na podstawie reguł
    difficulty = calculate_difficulty(player_deaths, enemies_defeated, potions_used)

    # Obliczamy dynamiczny typ gracza
    player_type = determine_dynamic_player_type(
        quests_completed, waypoints_discovered, zones_discovered, npc_interactions, enemies_defeated, total_combat_time, achievements_unlocked
    )

    data.append([
        player_deaths, enemies_defeated, total_combat_time, potions_used,
        waypoints_discovered, zones_discovered, quests_completed, npc_interactions,
        achievements_unlocked, difficulty, player_type
    ])

# ===== TWORZENIE DATASETU =====
columns = [
    "Player Deaths", "Enemies Defeated", "Total Combat Time", "Potions Used",
    "Waypoints Discovered", "Zones Discovered", "Quests Completed", "NPC Interactions",
    "Achievements Unlocked", "Current Difficulty Multiplier", "Current Dynamic Player Type"
]

df = pd.DataFrame(data, columns=columns)

# ===== ZAPIS DO CSV =====
df.to_csv("dataset_generated.csv", index=False)
print(f"✅ Wygenerowano nowy dataset: dataset_generated.csv (liczba próbek: {len(df)})")
