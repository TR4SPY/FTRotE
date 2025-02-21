using UnityEngine;
using Unity.Sentis;

public class AIModel : MonoBehaviour
{
    public static AIModel Instance;
    public ModelAsset modelAsset;  
    private Model runtimeModel;
    private Worker worker;
    private TensorShape inputShape;

    // 📌 **NOWE wartości mean i std pobrane z train_model.py**
    private readonly float[] mean = { 6.97f, 99.54f, 2508.73f, 24.33f };
    private readonly float[] std = { 4.32f, 57.95f, 1438.51f, 14.40f };

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
        worker = new Worker(runtimeModel, BackendType.CPU);  // ✅ Zmiana na CPU dla kompatybilności
        inputShape = new TensorShape(1, 4);  // ✅ Sprawdź, czy model nie wymaga [4] zamiast [1,4]

        Debug.Log("[AI-DDA] Model załadowany i gotowy do predykcji.");
    }

    public float PredictDifficulty(float playerDeaths, float enemiesDefeated, float totalCombatTime, float potionsUsed)
    {
        if (worker == null)
        {
            Debug.LogError("[AI-DDA] Worker nie został poprawnie zainicjalizowany!");
            return 5.0f;
        }

        // 📌 **NOWA Normalizacja zgodna z train_model.py**
        float normDeaths = (playerDeaths - mean[0]) / std[0];
        float normEnemies = (enemiesDefeated - mean[1]) / std[1];
        float normCombatTime = (totalCombatTime - mean[2]) / std[2];
        float normPotions = (potionsUsed - mean[3]) / std[3];

        float[] inputStats = new float[] { normDeaths, normEnemies, normCombatTime, normPotions };

        Debug.Log($"[AI-DDA] Normalized Input -> Deaths: {normDeaths}, Enemies: {normEnemies}, Combat Time: {normCombatTime}, Potions: {normPotions}");

        using var inputTensor = new Tensor<float>(inputShape, inputStats);
        
        worker.Schedule(inputTensor);  
        
        using var outputTensor = worker.PeekOutput().ReadbackAndClone() as Tensor<float>;

        if (outputTensor == null)
        {
            Debug.LogError("[AI-DDA] Brak poprawnego wyjścia z modelu!");
            return 5.0f;
        }

        float rawPrediction = outputTensor[0];  

        // 📌 **Usunięcie dodatkowego skalowania, ponieważ model robi to sam**
        float adjustedPrediction = Mathf.Clamp(rawPrediction * 5 + 5, 1.0f, 10.0f);

        Debug.Log($"[AI-DDA] Raw Prediction: {rawPrediction}, Adjusted: {adjustedPrediction}");

        return adjustedPrediction;
    }

    private void OnDestroy()
    {
        worker?.Dispose();
    }
}
