import pandas as pd
import joblib
from sklearn.model_selection import train_test_split, GridSearchCV
from xgboost import XGBRegressor
from sklearn.metrics import mean_absolute_error

# Wczytanie danych
df = pd.read_csv("generated_dataset.csv")

# Dodanie nowej cechy
features = ["Total Play Time", "Player Deaths", "Enemies Defeated", "Total Combat Time",
            "NPC Interactions", "Potions Used", "Zones Discovered", "Quests Completed",
            "Waypoints Discovered", "Unlocked Achievements"]

target = "Current Difficulty Multiplier"

# Sprawdzenie, czy dane są kompletne
if any(col not in df.columns for col in features + [target]):
    missing_cols = [col for col in features + [target] if col not in df.columns]
    raise ValueError(f"Brakujące kolumny w CSV: {missing_cols}")

# Przygotowanie danych
X = df[features]
y = df[target]

# Podział na zestawy treningowe i testowe (80/20)
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

# Definicja modelu XGBoost
xgb_model = XGBRegressor()

# Definicja hiperparametrów do tuningu
param_grid = {
    "n_estimators": [50, 100, 200, 300, 500],
    "learning_rate": [0.001, 0.01, 0.05, 0.1, 0.2],
    "max_depth": [3, 4, 5, 6, 7, 8, 10],
}

# Grid Search dla najlepszego modelu
grid_search = GridSearchCV(xgb_model, param_grid, cv=3, scoring="neg_mean_absolute_error", n_jobs=-1)
grid_search.fit(X_train, y_train)

# Znalezienie najlepszego modelu
best_model = grid_search.best_estimator_
y_pred = best_model.predict(X_test)
mae = mean_absolute_error(y_test, y_pred)

print(f"Najlepszy model: XGBoost - MAE: {mae:.4f}")
print(f"Najlepsze hiperparametry: {grid_search.best_params_}")

# Zapis modelu
joblib.dump(best_model, "ai_model.pkl")

print("Model zapisany jako `ai_model.pkl`!")
