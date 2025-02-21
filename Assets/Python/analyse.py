import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

# Wczytanie danych
df = pd.read_csv("dataset_generated.csv")

# Sprawdzenie kolumn i typów danych
print(df.dtypes)

# Usunięcie kolumn zawierających wartości nienumeryczne
df_numeric = df.select_dtypes(include=["number"])

# Sprawdzenie podstawowych statystyk
print("Podstawowe statystyki danych:")
print(df_numeric.describe())

# Histogramy dla każdej cechy
features = ["Player Deaths", "Enemies Defeated", "Total Combat Time", "Potions Used", "Current Difficulty Multiplier"]

plt.figure(figsize=(12, 8))
for i, feature in enumerate(features, 1):
    plt.subplot(2, 3, i)
    sns.histplot(df_numeric[feature], bins=30, kde=True)
    plt.title(f'Dystrybucja: {feature}')
plt.tight_layout()
plt.show()

# Wykresy pudełkowe dla wykrycia wartości odstających
plt.figure(figsize=(12, 8))
for i, feature in enumerate(features, 1):
    plt.subplot(2, 3, i)
    sns.boxplot(x=df_numeric[feature])
    plt.title(f'Boxplot: {feature}')
plt.tight_layout()
plt.show()

# Macierz korelacji
plt.figure(figsize=(10, 6))
sns.heatmap(df_numeric.corr(), annot=True, cmap='coolwarm', fmt='.2f')
plt.title("Macierz korelacji")
plt.show()

print("Analiza dystrybucji zakończona.")
