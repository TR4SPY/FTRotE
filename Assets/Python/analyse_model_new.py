import torch
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score
import onnxruntime

# ===== 1. Wczytanie danych =====
df = pd.read_csv("dataset_generated.csv")

# Kolumny wejściowe (feature'y)
features = ["Player Deaths", "Enemies Defeated", "Total Combat Time", "Potions Used"]
target = "Current Difficulty Multiplier"

X = df[features].values
y = df[target].values

# ===== 2. Normalizacja wejść i wyjścia =====
scaler = StandardScaler()
X_scaled = scaler.fit_transform(X)

# Podział na zbiór treningowy i testowy
X_train, X_test, y_train, y_test = train_test_split(X_scaled, y, test_size=0.2, random_state=42)

# ===== 3. Ładowanie modelu ONNX i testowanie =====
session = onnxruntime.InferenceSession("difficulty_predictor.onnx")

# Upewniamy się, że wejście jest w poprawnym formacie (przetwarzamy próbki pojedynczo)
predictions = []
for i in range(X_test.shape[0]):
    onnx_inputs = {session.get_inputs()[0].name: X_test[i:i+1].astype(np.float32)}
    onnx_output = session.run(None, onnx_inputs)
    predictions.append(onnx_output[0].squeeze())  # Usuwamy dodatkowe wymiary

predictions = np.array(predictions)

# Przeskalowanie wyników do oryginalnego zakresu
predictions_original = predictions * (y.max() - y.min()) + y.min()

# ===== 4. Obliczanie metryk =====
mae = mean_absolute_error(y_test, predictions_original)
mse = mean_squared_error(y_test, predictions_original)
rmse = np.sqrt(mse)  # 🔹 Ręczne obliczenie RMSE
r2 = r2_score(y_test, predictions_original)

print(f"MAE: {mae:.4f}, RMSE: {rmse:.4f}, R²: {r2:.4f}")

# ===== 5. Wizualizacja wyników =====
plt.figure(figsize=(8, 5))
plt.scatter(y_test, predictions_original, label="Predykcje", alpha=0.5)
plt.plot([1, 10], [1, 10], linestyle="--", color="red", label="Idealna linia")
plt.xlabel("Rzeczywista trudność")
plt.ylabel("Przewidywana trudność")
plt.title("Porównanie rzeczywistej i przewidywanej trudności")
plt.legend()
plt.show()

# Histogram błędów
errors = y_test - predictions_original
plt.hist(errors, bins=50, alpha=0.75, label="Błędy predykcji")
plt.axvline(errors.mean(), color="red", linestyle="--", label="Średnia różnica")
plt.xlabel("Błąd predykcji (Rzeczywista - Przewidywana)")
plt.ylabel("Liczba przypadków")
plt.title("Histogram błędów predykcji")
plt.legend()
plt.show()
