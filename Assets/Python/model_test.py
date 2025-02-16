import pandas as pd
import numpy as np
import joblib
from sklearn.model_selection import train_test_split, GridSearchCV
from sklearn.ensemble import RandomForestRegressor, GradientBoostingRegressor
from xgboost import XGBRegressor
from sklearn.metrics import mean_absolute_error

# Wczytanie danych
df = pd.read_csv("generated_dataset.csv")

# Przygotowanie danych
features = ["Total Play Time", "Player Deaths", "Enemies Defeated", "Total Combat Time",
            "NPC Interactions", "Potions Used", "Zones Discovered", "Quests Completed",
            "Waypoints Discovered", "Unlocked Achievements"]

target = "Current Difficulty Multiplier"

X = df[features]
y = df[target]

# Podział na zestawy treningowe i testowe (80/20)
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

# Definicja modeli ML
models = {
    "RandomForest": RandomForestRegressor(),
    "XGBoost": XGBRegressor(),
    "GradientBoosting": GradientBoostingRegressor()
}

# Definicja hiperparametrów do tuningu
param_grid = {
    "RandomForest": {
        "n_estimators": [50, 100, 200],
        "max_depth": [None, 5, 10],
        "min_samples_split": [2, 5, 10],
    },
    "XGBoost": {
        "n_estimators": [50, 100, 200],
        "learning_rate": [0.01, 0.1, 0.2],
        "max_depth": [3, 6, 10],
    },
    "GradientBoosting": {
        "n_estimators": [50, 100, 200],
        "learning_rate": [0.01, 0.1, 0.2],
        "max_depth": [3, 6, 10],
    }
}

# Trenowanie i porównanie modeli
best_models = {}
results = {}

for name, model in models.items():
    print(f"Trenowanie modelu: {name}")

    grid_search = GridSearchCV(model, param_grid[name], cv=3, scoring="neg_mean_absolute_error", n_jobs=-1)
    grid_search.fit(X_train, y_train)

    best_model = grid_search.best_estimator_
    y_pred = best_model.predict(X_test)
    mae = mean_absolute_error(y_test, y_pred)

    best_models[name] = best_model
    results[name] = {"MAE": mae, "Best Params": grid_search.best_params_}

    print(f"{name} - MAE: {mae:.4f} - Best Params: {grid_search.best_params_}")

# Zapis najlepszego modelu
best_model_name = min(results, key=lambda k: results[k]["MAE"])
joblib.dump(best_models[best_model_name], "ai_model.pkl")

print(f"Najlepszy model: {best_model_name} (MAE: {results[best_model_name]['MAE']:.4f}) zapisany jako `ai_model.pkl`!")

