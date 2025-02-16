import joblib
import numpy as np
import onnx
import onnxmltools
import xgboost as xgb
from skl2onnx import update_registered_converter
from skl2onnx.common.data_types import FloatTensorType
from onnxmltools.convert.xgboost.operator_converters.XGBoost import convert_xgboost

# Wczytaj model XGBoost
model = joblib.load("ai_model.pkl")

# Rejestracja konwertera dla XGBoost
update_registered_converter(
    xgb.XGBRegressor,
    "XGBoostXGBRegressor",
    convert_xgboost,
    options={"nocl": [True, False]},
)

# Określenie wejścia modelu (10 cech wejściowych)
initial_type = [("float_input", FloatTensorType([None, 10]))]

# Konwersja modelu XGBoost do ONNX
onnx_model = onnxmltools.convert.convert_xgboost(model, initial_types=initial_type)

# Zapis modelu do pliku
onnx.save_model(onnx_model, "ai_model.onnx")

print("✅ Model XGBoost został przekonwertowany do ONNX!")
