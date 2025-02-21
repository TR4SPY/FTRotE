import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score

# ===== 1. Wczytanie danych =====
df = pd.read_csv("dataset_generated.csv")

# Kolumny wejściowe (feature'y)
features = ["Player Deaths", "Enemies Defeated", "Total Combat Time", "Potions Used"]
target = "Current Difficulty Multiplier"

X = df[features].values
y = df[target].values

# ===== 2. Normalizacja =====
scaler_X = StandardScaler()
X_scaled = scaler_X.fit_transform(X)

y_min, y_max = y.min(), y.max()
y_scaled = (y - y_min) / (y_max - y_min)  # Normalizacja do [0,1]

# Podział na zbiór treningowy i testowy
X_train, X_test, y_train, y_test = train_test_split(X_scaled, y_scaled, test_size=0.2, random_state=42)

# ===== 3. Definicja modeli =====
class MLPModel(nn.Module):
    def __init__(self, input_size, hidden_layers, activation):
        super(MLPModel, self).__init__()
        layers = []
        prev_size = input_size
        
        for size in hidden_layers:
            layers.append(nn.Linear(prev_size, size))
            if activation == "ReLU":
                layers.append(nn.ReLU())
            elif activation == "LeakyReLU":
                layers.append(nn.LeakyReLU())
            layers.append(nn.Dropout(0.3))
            prev_size = size

        layers.append(nn.Linear(prev_size, 1))
        layers.append(nn.Tanh())  # Skalowanie do [-1,1]
        self.model = nn.Sequential(*layers)

    def forward(self, x):
        return self.model(x) * (y_max - y_min) + y_min  # Skalowanie do oryginalnego zakresu

# Konfiguracja modeli:
configurations = [
    ([64, 32], "ReLU"),
    ([256, 128], "LeakyReLU")  # Nowa konfiguracja
]

results = {}
model_predictions = {}

# ===== 4. Trening i testowanie modeli =====
for hidden_layers, activation in configurations:
    print(f"\n🔹 Trening modelu: {hidden_layers} + {activation}")

    # Inicjalizacja modelu
    model = MLPModel(input_size=X_train.shape[1], hidden_layers=hidden_layers, activation=activation)
    criterion = nn.HuberLoss()
    optimizer = optim.Adam(model.parameters(), lr=0.001, weight_decay=1e-5)

    # Trening
    epochs = 500
    loss_values = []
    X_train_tensor = torch.tensor(X_train, dtype=torch.float32)
    y_train_tensor = torch.tensor(y_train, dtype=torch.float32).view(-1, 1)

    for epoch in range(epochs):
        optimizer.zero_grad()
        outputs = model(X_train_tensor)
        loss = criterion(outputs, y_train_tensor)
        loss.backward()
        optimizer.step()
        loss_values.append(loss.item())

    # Wykres funkcji straty
    plt.plot(loss_values, label=f"{hidden_layers} + {activation}")

    # Testowanie
    model.eval()
    with torch.no_grad():
        X_test_tensor = torch.tensor(X_test, dtype=torch.float32)
        predictions = model(X_test_tensor).detach().cpu().numpy()

    # Odtworzenie skali
    predictions = predictions.squeeze()
    y_test_original = y_test * (y_max - y_min) + y_min  # Odtwarzamy oryginalne wartości

    mae = mean_absolute_error(y_test_original, predictions)
    rmse = np.sqrt(mean_squared_error(y_test_original, predictions))  # Poprawka RMSE!
    r2 = r2_score(y_test_original, predictions)

    results[f"{hidden_layers} + {activation}"] = (mae, rmse, r2)
    model_predictions[f"{hidden_layers} + {activation}"] = (y_test_original, predictions)

    print(f"MAE: {mae:.4f}, RMSE: {rmse:.4f}, R²: {r2:.4f}")

# ===== 5. Wizualizacja wyników =====
plt.xlabel("Epoch")
plt.ylabel("Loss")
plt.title("Przebieg funkcji straty")
plt.legend()
plt.show()

# ===== 6. Porównanie wyników =====
df_results = pd.DataFrame(results, index=["MAE", "RMSE", "R²"]).T
print("\n📊 **Podsumowanie wyników:**")
print(df_results)

# ===== 7. Wykres predykcji vs rzeczywista trudność =====
plt.figure(figsize=(8, 5))
for model_name, (y_true, y_pred) in model_predictions.items():
    plt.scatter(y_true, y_pred, label=f"{model_name}", alpha=0.5)

plt.plot([1, 10], [1, 10], linestyle="--", color="red", label="Idealna linia")
plt.xlabel("Rzeczywista trudność")
plt.ylabel("Przewidywana trudność")
plt.title("Porównanie rzeczywistej i przewidywanej trudności")
plt.legend()
plt.show()

# ===== 8. Histogram błędów predykcji =====
plt.figure(figsize=(8, 5))
for model_name, (y_true, y_pred) in model_predictions.items():
    errors = y_true - y_pred
    plt.hist(errors, bins=50, alpha=0.5, label=f"{model_name}")

plt.axvline(0, color="red", linestyle="--", label="Brak błędu")
plt.xlabel("Błąd predykcji (Rzeczywista - Przewidywana)")
plt.ylabel("Liczba przypadków")
plt.title("Histogram błędów predykcji")
plt.legend()
plt.show()
