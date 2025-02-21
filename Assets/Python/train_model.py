import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
import onnx
import onnxruntime
import torch.onnx

# ===== 1. Wczytanie danych =====
df = pd.read_csv("dataset_generated.csv")  # Używamy nowego datasetu!

# Kolumny wejściowe (feature'y)
features = ["Player Deaths", "Enemies Defeated", "Total Combat Time", "Potions Used"]
target = "Current Difficulty Multiplier"

X = df[features].values
y = df[target].values

# ===== 2. Wzmocnienie wpływu niektórych zmiennych =====
df["Player Deaths"] *= 2  # Każdy zgon bardziej wpływa na trudność
df["Enemies Defeated"] *= 1.5  # Wzmocnienie wpływu pokonanych przeciwników
df["Potions Used"] *= 3  # Nadużywanie mikstur bardziej wpływa na obniżenie trudności

# ===== 3. Normalizacja wejść i wyjścia =====
y_min, y_max = y.min(), y.max()
y_scaled = (y - y_min) / (y_max - y_min)  # Normalizacja wyjścia do zakresu [0,1]

# Podział na zbiór treningowy i testowy
X_train, X_test, y_train, y_test = train_test_split(X, y_scaled, test_size=0.2, random_state=42)

# Normalizacja wejść (feature'ów)
scaler = StandardScaler()
X_train = scaler.fit_transform(X_train)
X_test = scaler.transform(X_test)

# Konwersja do tensorów PyTorch
X_train_tensor = torch.tensor(X_train, dtype=torch.float32)
y_train_tensor = torch.tensor(y_train, dtype=torch.float32).view(-1, 1)
X_test_tensor = torch.tensor(X_test, dtype=torch.float32)
y_test_tensor = torch.tensor(y_test, dtype=torch.float32).view(-1, 1)

# ===== 4. Definicja modelu sieci neuronowej (MLP) =====
class DifficultyPredictor(nn.Module):
    def __init__(self, input_size):
        super(DifficultyPredictor, self).__init__()
        self.model = nn.Sequential(
            nn.Linear(input_size, 64),
            nn.ReLU(),
            nn.Dropout(0.3),
            nn.Linear(64, 32),
            nn.ReLU(),
            nn.Dropout(0.3),
            nn.Linear(32, 1),
            nn.Tanh()  # ✅ Nowa warstwa, aby wynik mieścił się w przedziale [-1,1]
        )

    def forward(self, x):
        return self.model(x) * 5 + 5  # ✅ Skaluje wynik do zakresu [1,10]

# Inicjalizacja modelu
model = DifficultyPredictor(input_size=len(features))
criterion = nn.HuberLoss(delta=1.0)  # Bardziej odporne na outliery niż MSELoss
optimizer = optim.Adam(model.parameters(), lr=0.001, weight_decay=1e-5)  # ✅ Dodany weight decay dla stabilności

# ===== 5. Trenowanie modelu =====
epochs = 500
for epoch in range(epochs):
    optimizer.zero_grad()
    outputs = model(X_train_tensor)
    loss = criterion(outputs, y_train_tensor)
    loss.backward()
    optimizer.step()

    if (epoch + 1) % 50 == 0:
        print(f"Epoch [{epoch+1}/{epochs}], Loss: {loss.item():.4f}")

# ===== 6. Testowanie modelu =====
model.eval()
with torch.no_grad():
    predictions = model(X_test_tensor)
    test_loss = criterion(predictions, y_test_tensor)
    print(f"Test Loss: {test_loss.item():.4f}")

# ===== 7. Zapisywanie modelu jako ONNX =====
onnx_path = "difficulty_predictor.onnx"
dummy_input = torch.randn(1, len(features))  # Przykładowe wejście dla ONNX
torch.onnx.export(model, dummy_input, onnx_path, input_names=["input"], output_names=["output"])

print(f"Model zapisany jako {onnx_path}")

# ===== 8. Testowanie modelu w ONNX + Wyświetlanie statystyk każdej próbki =====
session = onnxruntime.InferenceSession(onnx_path)

print("\n🔹 **TESTUJEMY 5 PRÓBEK W ONNX** 🔹")
true_difficulties = []
predicted_difficulties = []

for i in range(5):  # Testujemy 5 różnych próbek
    onnx_inputs = {session.get_inputs()[0].name: X_test[i:i+1].astype(np.float32)}
    onnx_outputs = session.run(None, onnx_inputs)

    # Przeskalowanie trudności z powrotem do zakresu [1,10]
    predicted_difficulty = (onnx_outputs[0] * (y_max - y_min)) + y_min

    # 🔹 Odtwarzanie rzeczywistych wartości wejściowych (przed normalizacją)
    original_values = scaler.inverse_transform(X_test[i:i+1])  # Przywrócenie rzeczywistych wartości

    print(f"\n🔹 **Próbka {i+1}:**")
    print(f"   🔸 Player Deaths (raw): {original_values[0][0]:.0f}")  # Liczba całkowita
    print(f"   🔸 Enemies Defeated (raw): {original_values[0][1]:.0f}")  # Liczba całkowita
    print(f"   🔸 Total Combat Time (raw): {original_values[0][2]:.2f} sec")  # Sekundy
    print(f"   🔸 Potions Used (raw): {original_values[0][3]:.0f}")  # Liczba całkowita
    print(f"   ✅ **Predykcja trudności:** {predicted_difficulty.squeeze():.2f}")
    print(f"   🎯 **Prawdziwa trudność:** {y_test[i] * (y_max - y_min) + y_min:.2f}")

    true_difficulties.append(y_test[i] * (y_max - y_min) + y_min)
    predicted_difficulties.append(predicted_difficulty.squeeze())  # 🔹 Naprawa wymiarów

# ===== 9. Wykres porównujący prawdziwą i przewidywaną trudność =====
plt.figure(figsize=(8, 5))
plt.plot(true_difficulties, label="Prawdziwa trudność", marker="o")
plt.plot(predicted_difficulties, label="Przewidywana trudność", marker="x")
plt.xlabel("Próbki")
plt.ylabel("Poziom trudności")
plt.title("Porównanie prawdziwej i przewidywanej trudności")
plt.legend()
plt.show()
