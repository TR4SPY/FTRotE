import pandas as pd
import random

# Liczba sztucznych graczy do wygenerowania
num_samples = 500
types = ["Killer", "Explorer", "Achiever", "Socializer"]

# Definicja zakresów wartości dla każdego typu gracza
type_ranges = {
    "Killer": {
        "Player Deaths": (0, 5),
        "Enemies Defeated": (50, 300),
        "Total Combat Time": (500, 5000),
        "NPC Interactions": (0, 5),
        "Potions Used": (5, 50),
        "Zones Discovered": (1, 5),
        "Quests Completed": (0, 10),
        "Waypoints Discovered": (1, 5),
        "Unlocked Achievements": (0, 5),
        "Total Play Time": (500, 5000),
        "Current Difficulty Multiplier": (0, 10),
    },
    "Explorer": {
        "Player Deaths": (0, 3),
        "Enemies Defeated": (5, 50),
        "Total Combat Time": (100, 1000),
        "NPC Interactions": (5, 50),
        "Potions Used": (1, 20),
        "Zones Discovered": (10, 50),
        "Quests Completed": (5, 30),
        "Waypoints Discovered": (10, 50),
        "Unlocked Achievements": (5, 20),
        "Total Play Time": (1000, 8000),
        "Current Difficulty Multiplier": (0, 10),
    },
    "Achiever": {
        "Player Deaths": (1, 4),
        "Enemies Defeated": (20, 150),
        "Total Combat Time": (300, 3000),
        "NPC Interactions": (10, 40),
        "Potions Used": (10, 40),
        "Zones Discovered": (5, 20),
        "Quests Completed": (20, 100),
        "Waypoints Discovered": (5, 30),
        "Unlocked Achievements": (10, 50),
        "Total Play Time": (1500, 7000),
        "Current Difficulty Multiplier": (0, 10),
    },
    "Socializer": {
        "Player Deaths": (0, 3),
        "Enemies Defeated": (0, 30),
        "Total Combat Time": (0, 500),
        "NPC Interactions": (20, 100),
        "Potions Used": (0, 10),
        "Zones Discovered": (3, 15),
        "Quests Completed": (5, 40),
        "Waypoints Discovered": (5, 30),
        "Unlocked Achievements": (5, 25),
        "Total Play Time": (2000, 9000),
        "Current Difficulty Multiplier": (0, 10),
    },
}

# Tworzenie pustej listy na dane
data = []

# Generowanie sztucznych graczy
for i in range(num_samples):
    player_type = random.choice(types)  # Losowy typ gracza
    values = {key: random.randint(*type_ranges[player_type][key]) for key in type_ranges[player_type]}
    
    # Przypisanie dynamicznego typu gracza
    values["Current Dynamic Player Type"] = player_type
    
    # Symulowany typ gracza na podstawie ankiety (może, ale nie musi zgadzać się z dynamicznym)
    values["Player Type based on questionnaire"] = random.choice(types)
    
    # Nazwa gracza (fikcyjna)
    values["Character Name"] = f"Player_{i+1}"
    
    data.append(values)

# Tworzenie DataFrame
df = pd.DataFrame(data)

# Ustawienie kolejności kolumn zgodnie z oryginalnym datasetem
df = df[
    ["Character Name", "Total Play Time", "Player Deaths", "Enemies Defeated", "Total Combat Time",
     "NPC Interactions", "Potions Used", "Zones Discovered", "Quests Completed", "Waypoints Discovered",
     "Unlocked Achievements", "Current Difficulty Multiplier", "Player Type based on questionnaire",
     "Current Dynamic Player Type"]
]

# Zapis do CSV
df.to_csv("generated_dataset.csv", index=False)

print("Sztuczne dane zostały wygenerowane i zapisane jako `generated_dataset.csv`!")
