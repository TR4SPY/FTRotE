import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestRegressor
from sklearn.metrics import mean_absolute_error
import joblib

# Wczytanie danych z pliku CSV
CSV_PATH = "dataset.csv"  # Zmienić, jeśli plik CSV jest w innym miejscu
df = pd.read_csv(CSV_PATH)

# Wybór cech (X) i etykiety (y)
features = ["Player Deaths", "Enemies Defeated", "Total Combat Time", "NPC Interactions",
            "Quests Completed", "Waypoints Discovered", "Zones Discovered", "Unlocked Achievements"]
target = "Current Difficulty Multiplier"

X = df[features]
y = df[target]

# Dzielenie danych na zestaw treningowy i testowy (80/20)
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

# Trening modelu ML (Random Forest)
model = RandomForestRegressor(n_estimators=100, random_state=42)
model.fit(X_train, y_train)

# Przewidywanie na danych testowych
y_pred = model.predict(X_test)
mae = mean_absolute_error(y_test, y_pred)

print(f"Model trained! Mean Absolute Error: {mae:.4f}")

# Save model to file
MODEL_PATH = "../StreamingAssets/ai_model.pkl"  # Unity będzie go używać
joblib.dump(model, MODEL_PATH)

print(f"Model saved to {MODEL_PATH}")

