import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
import itertools

# Wczytanie danych
df = pd.read_csv("dataset_generated.csv")

features = ["Player Deaths", "Enemies Defeated", "Total Combat Time", "Potions Used"]
target = "Current Difficulty Multiplier"

X = df[features].values
y = df[target].values

# Normalizacja celu do [0,1]
y_min, y_max = y.min(), y.max()
y_scaled = (y - y_min) / (y_max - y_min)

# Podział na zbiór treningowy i testowy
X_train, X_test, y_train, y_test = train_test_split(X, y_scaled, test_size=0.2, random_state=42)

# Normalizacja wejść
scaler = StandardScaler()
X_train = scaler.fit_transform(X_train)
X_test = scaler.transform(X_test)

# Konwersja do tensorów PyTorch
X_train_tensor = torch.tensor(X_train, dtype=torch.float32)
y_train_tensor = torch.tensor(y_train, dtype=torch.float32).view(-1, 1)
X_test_tensor = torch.tensor(X_test, dtype=torch.float32)
y_test_tensor = torch.tensor(y_test, dtype=torch.float32).view(-1, 1)

# Parametry do testowania
hidden_sizes = [[64, 32], [128, 64], [256, 128]]
activations = [nn.ReLU, nn.LeakyReLU, nn.Tanh]
loss_functions = [nn.MSELoss(), nn.HuberLoss(delta=1.0)]
dropout_rates = [0.3, 0.5]
learning_rates = [0.001, 0.0005]

best_model = None
best_loss = float("inf")
best_config = None

# Testowanie różnych kombinacji
for h_sizes, activation, loss_func, dropout, lr in itertools.product(hidden_sizes, activations, loss_functions, dropout_rates, learning_rates):
    print(f"Testujemy konfigurację: {h_sizes}, {activation.__name__}, {loss_func}, Dropout: {dropout}, LR: {lr}")

    # Definicja modelu
    class TestModel(nn.Module):
        def __init__(self):
            super(TestModel, self).__init__()
            layers = []
            input_size = len(features)
            for h in h_sizes:
                layers.append(nn.Linear(input_size, h))
                layers.append(activation())
                layers.append(nn.Dropout(dropout))
                input_size = h
            layers.append(nn.Linear(input_size, 1))
            layers.append(nn.Sigmoid())  # Ograniczenie do [0,1]
            self.model = nn.Sequential(*layers)

        def forward(self, x):
            return self.model(x) * 9 + 1  # Skalowanie do [1,10]

    model = TestModel()
    optimizer = optim.Adam(model.parameters(), lr=lr, weight_decay=1e-5)

    # Trenowanie modelu
    for epoch in range(100):  # Zmniejszamy liczbę epok dla szybszego testowania
        optimizer.zero_grad()
        outputs = model(X_train_tensor)
        loss = loss_func(outputs, y_train_tensor)
        loss.backward()
        optimizer.step()

    # Testowanie modelu
    model.eval()
    with torch.no_grad():
        predictions = model(X_test_tensor)
        test_loss = loss_func(predictions, y_test_tensor)

    print(f"Test Loss: {test_loss.item():.4f}")

    # Zapis najlepszego modelu
    if test_loss.item() < best_loss:
        best_loss = test_loss.item()
        best_model = model
        best_config = (h_sizes, activation.__name__, loss_func, dropout, lr)

print(f"\n✅ Najlepsza konfiguracja: {best_config}, Test Loss: {best_loss:.4f}")
