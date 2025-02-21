import numpy as np
import onnxruntime
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score

# ===== 1. Wczytanie modelu ONNX =====
onnx_path = "difficulty_predictor.onnx"
session = onnxruntime.InferenceSession(onnx_path)

# ===== 2. Wczytanie danych testowych =====
df = pd.read_csv("dataset_generated.csv")

# Kolumny wejściowe i wyjściowe
features = ["Player Deaths", "Enemies Defeated", "Total Combat Time", "Potions Used"]
target = "Current Difficulty Multiplier"

X = df[features].values.astype(np.float32)  # ✅ Zmieniamy typ na float32
y_true = df[target].values.astype(np.float32)  # ✅ Zmieniamy typ na float32

# ===== 3. Normalizacja wartości wejściowych (TAK JAK W AIModel.cs) =====
mean = np.array([5.96, 79.21, 2873.45, 17.85], dtype=np.float32)  # ✅ Upewniamy się, że to float32
std = np.array([6.32, 59.88, 1423.89, 9.75], dtype=np.float32)  # ✅ Upewniamy się, że to float32

X_normalized = (X - mean) / std

# ===== 4. Predykcje modelu ONNX =====
predicted_difficulties = []

for i in range(len(X_normalized)):
    onnx_inputs = {session.get_inputs()[0].name: X_normalized[i:i+1].astype(np.float32)}  # ✅ Konwersja na float32
    onnx_outputs = session.run(None, onnx_inputs)
    raw_prediction = onnx_outputs[0][0][0]

    # Skalowanie do przedziału [1,10]
    adjusted_prediction = raw_prediction * 4 + 5
    predicted_difficulties.append(adjusted_prediction)

predicted_difficulties = np.array(predicted_difficulties, dtype=np.float32)  # ✅ Konwersja na float32

# ===== 5. Obliczenie metryk jakości modelu =====
mae = mean_absolute_error(y_true, predicted_difficulties)
rmse = np.sqrt(mean_squared_error(y_true, predicted_difficulties))
r2 = r2_score(y_true, predicted_difficulties)

print(f"Mean Absolute Error (MAE): {mae:.4f}")
print(f"Root Mean Squared Error (RMSE): {rmse:.4f}")
print(f"R² Score: {r2:.4f}")

# ===== 6. Generowanie wykresów =====

# 🔹 Wykres rzeczywistych vs przewidywanych wartości
plt.figure(figsize=(8, 6))
plt.scatter(y_true, predicted_difficulties, alpha=0.6, label="Predykcje")
plt.plot([min(y_true), max(y_true)], [min(y_true), max(y_true)], color="red", linestyle="--", label="Idealna linia")
plt.xlabel("Rzeczywista trudność")
plt.ylabel("Przewidywana trudność")
plt.title("Porównanie rzeczywistej i przewidywanej trudności")
plt.legend()
plt.grid(True)
plt.show()
