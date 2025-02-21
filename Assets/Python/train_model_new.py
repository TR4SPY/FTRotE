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
df = pd.read_csv("dataset_generated.csv")

# Kolumny wejściowe (feature'y)
features = ["Player Deaths", "Enemies Defeated", "Total Combat Time", "Potions Used"]
target = "Current Difficulty Multiplier"

X = df[features].values
y = df[target].values

# ===== 2. Normalizacja wejść i wyjścia =====
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

# ===== 3. Definicja modelu sieci neuronowej (MLP) =====
class DifficultyPredictor(nn.Module):
    def __init__(self, input_size):
        super(DifficultyPredictor, self).__init__()
        self.model = nn.Sequential(
            nn.Linear(input_size, 256),
            nn.LeakyReLU(),
            nn.Dropout(0.3),
            nn.Linear(256, 128),
            nn.LeakyReLU(),
            nn.Dropout(0.3),
            nn.Linear(128, 1),
            nn.Tanh()  # ✅ Skaluje wynik do [-1,1]
        )

    def forward(self, x):
        return self.model(x) * 5 + 5  # ✅ Skaluje wynik do zakresu [1,10]

# Inicjalizacja modelu
model = DifficultyPredictor(input_size=len(features))
criterion = nn.HuberLoss()
optimizer = optim.Adam(model.parameters(), lr=0.001, weight_decay=1e-5)

# ===== 4. Trenowanie modelu =====
epochs = 500
loss_values = []
for epoch in range(epochs):
    optimizer.zero_grad()
    outputs = model(X_train_tensor)
    loss = criterion(outputs, y_train_tensor)
    loss.backward()
    optimizer.step()
    loss_values.append(loss.item())
    if (epoch + 1) % 50 == 0:
        print(f"Epoch [{epoch+1}/{epochs}], Loss: {loss.item():.4f}")

# ===== 5. Testowanie modelu =====
model.eval()
with torch.no_grad():
    predictions = model(X_test_tensor)
    predictions = predictions.numpy().squeeze()
    predictions_original = predictions * (y_max - y_min) + y_min  # ✅ Odwrócenie skalowania
    
    test_loss = criterion(torch.tensor(predictions_original), torch.tensor(y_test))
    print(f"Test Loss: {test_loss.item():.4f}")

# ===== 6. Zapisywanie modelu jako ONNX =====
onnx_path = "difficulty_predictor.onnx"
dummy_input = torch.randn(1, len(features))
torch.onnx.export(model, dummy_input, onnx_path, input_names=["input"], output_names=["output"])

print(f"Model zapisany jako {onnx_path}")
