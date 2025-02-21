using UnityEngine;
using Unity.Sentis;

public class AIModel : MonoBehaviour
{
    public static AIModel Instance;
    public ModelAsset modelAsset;  // Przypisujemy model w Unity Inspectorze
    private Model runtimeModel;
    private Worker worker;
    private TensorShape inputShape;

    // ✅ Wartości mean i std wpisane na stałe (zamiast wczytywać plik)
    private readonly float[] mean = { 5.96f, 79.21f, 2873.45f, 17.85f };
    private readonly float[] std = { 6.32f, 59.88f, 1423.89f, 9.75f };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadModel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadModel()
    {
        if (modelAsset == null)
        {
            Debug.LogError("[AI-DDA] Model ONNX nie został przypisany w Inspectorze!");
            return;
        }

        runtimeModel = ModelLoader.Load(modelAsset);
        worker = new Worker(runtimeModel, BackendType.GPUCompute); // Możesz zmienić na BackendType.CPU
        inputShape = new TensorShape(1, 4);  // 🔹 Model przyjmuje 4 wartości wejściowe

        Debug.Log("[AI-DDA] Model załadowany i gotowy do predykcji.");
    }

    public float PredictDifficulty(float playerDeaths, float enemiesDefeated, float totalCombatTime, float potionsUsed)
    {
        if (worker == null)
        {
            Debug.LogError("[AI-DDA] Worker nie został poprawnie zainicjalizowany!");
            return 5.0f;  // Domyślny poziom trudności
        }

        // ✅ **Normalizacja danych wejściowych**
        //float normDeaths = (playerDeaths * 2 - mean[0]) / std[0];
        //float normEnemies = (enemiesDefeated * 1.5f - mean[1]) / std[1];
        //float normCombatTime = (totalCombatTime - mean[2]) / std[2];
        //float normPotions = (potionsUsed * 3 - mean[3]) / std[3];

        float normDeaths = (playerDeaths * 3 - mean[0]) / std[0];
        float normEnemies = (enemiesDefeated * 2.5f - mean[1]) / std[1]; // ⚡ Większy wpływ
        float normCombatTime = (totalCombatTime - mean[2]) / std[2];
        float normPotions = (potionsUsed * 3 - mean[3]) / std[3];

        float[] inputStats = new float[] { normDeaths, normEnemies, normCombatTime, normPotions };

        Debug.Log($"[AI-DDA] Normalized Input Stats -> Deaths: {normDeaths}, Enemies: {normEnemies}, Combat Time: {normCombatTime}, Potions: {normPotions}");

        using var inputTensor = new Tensor<float>(inputShape, inputStats);
        
        worker.Schedule(inputTensor);  // **✅ Uruchamiamy obliczenia**
        
        using var outputTensor = worker.PeekOutput().ReadbackAndClone() as Tensor<float>;  // **✅ Używamy ReadbackAndClone()**
        
        if (outputTensor == null)
        {
            Debug.LogError("[AI-DDA] Brak poprawnego wyjścia z modelu!");
            return 5.0f;
        }

        float rawPrediction = outputTensor[0];  // **✅ Pobieramy pierwszą wartość z tensora**

        // **🔥 Korekta skalowania**
        //float adjustedPrediction = rawPrediction * 9 + 1; // Skalujemy do [1-10]
        float adjustedPrediction = rawPrediction * 4 + 5;

        Debug.Log($"[AI-DDA] Raw Prediction: {rawPrediction}, Adjusted: {adjustedPrediction}");

        return Mathf.Clamp(adjustedPrediction, 1.0f, 10.0f);
    }

    private void OnDestroy()
    {
        worker?.Dispose();
    }
}
