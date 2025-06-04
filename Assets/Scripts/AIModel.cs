using UnityEngine;
using Unity.Sentis;

namespace AI_DDA.Assets.Scripts
{
    public class AIModel : MonoBehaviour
    {
        public static AIModel Instance;
        public ModelAsset modelAsset;  
        private Model runtimeModel;
        private Worker worker;
        private TensorShape inputShape;

        //  Mean & Std from model_training.py
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
                Debug.LogError("[AI-DDA] ONNX file of MLP model has not been found in inspector!"); //  DEBUG - Cannot find MLP ONNX in Unity inspector
                return;
            }

            runtimeModel = ModelLoader.Load(modelAsset);
            worker = new Worker(runtimeModel, BackendType.CPU);  // Changed to CPU for compatibility
            inputShape = new TensorShape(1, 4);  // Checking if model does not require [4] instead of [1,4]

            // Debug.Log("[AI-DDA] MLP Model has been loaded and is ready to predict.");   //  DEBUG - MLP has been loaded
        }

        public float PredictDifficulty(float playerDeaths, float enemiesDefeated, float totalCombatTime, float potionsUsed)
        {
            if (worker == null)
            {
                Debug.LogError("[AI-DDA] Worker has not been properly initialized!");   //  DEBUG - MLP model worker has not been properly inicialized
                return 5.0f;
            }

            // Normalization
            float normDeaths = (playerDeaths - mean[0]) / std[0];
            float normEnemies = (enemiesDefeated - mean[1]) / std[1];
            float normCombatTime = (totalCombatTime - mean[2]) / std[2];
            float normPotions = (potionsUsed - mean[3]) / std[3];

            float[] inputStats = new float[] { normDeaths, normEnemies, normCombatTime, normPotions };

            // Debug.Log($"[AI-DDA] Normalized Input -> Deaths: {normDeaths}, Enemies: {normEnemies}, Combat Time: {normCombatTime}, Potions: {normPotions}"); //  DEBUG - Normalization

            using var inputTensor = new Tensor<float>(inputShape, inputStats);
            
            worker.Schedule(inputTensor);  
            
            using var outputTensor = worker.PeekOutput().ReadbackAndClone() as Tensor<float>;

            if (outputTensor == null)
            {
                Debug.LogError("[AI-DDA] Missing proper model output!");    //  DEBUG - Missing MLP output
                return 5.0f;
            }

            float rawPrediction = outputTensor[0];  

            float adjustedPrediction = Mathf.Clamp(rawPrediction * 5 + 5, 3.0f, 10.0f);

            // Debug.Log($"[AI-DDA] MLP raw prediction: {rawPrediction}, adjusted: {adjustedPrediction}"); //  DEBUG - MLP raw & adjusted prediction values

            return adjustedPrediction;
        }

        private void OnDestroy()
        {
            worker?.Dispose();
        }
    }
}