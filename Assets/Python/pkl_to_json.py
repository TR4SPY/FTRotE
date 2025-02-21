import pickle
import json

# Wczytaj scaler.pkl
with open("scaler.pkl", "rb") as f:
    scaler = pickle.load(f)

# Pobierz średnią i odchylenie standardowe
scaler_data = {
    "mean": scaler.mean_.tolist(),
    "std": scaler.scale_.tolist()
}

# Zapisz do pliku JSON
with open("scaler.json", "w") as f:
    json.dump(scaler_data, f, indent=4)

print("Zapisano scaler.json")

