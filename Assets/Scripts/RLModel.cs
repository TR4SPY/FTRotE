using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class RLModel : Agent
    {
        public static RLModel Instance;
        private float currentDifficulty;
        private float adjustedDifficulty;
        private PlayerBehaviorLogger playerLogger;

        private float lastDecisionTime = 0f;
        private float decisionInterval = 5f;
        private float lastLogTime = 0f;
        private float logCooldown = 10f; 

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }   

        private void Start()
        {
            StartCoroutine(InitializeLogger());
        }

        private IEnumerator InitializeLogger()
        {
            while (PlayerBehaviorLogger.Instance == null)
            {
                Debug.LogWarning("[AI-DDA] Waiting for PlayerBehaviorLogger...");
                yield return new WaitForSeconds(0.5f);
            }

            playerLogger = PlayerBehaviorLogger.Instance;
            // Debug.Log("[AI-DDA] RLModel successfully found PlayerBehaviorLogger.");
        }

        public void SetPlayerLogger(PlayerBehaviorLogger logger)
        {
            playerLogger = logger;
            Debug.Log("[AI-DDA] PlayerBehaviorLogger assigned to RLModel.");
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            if (playerLogger == null)
            {
                Debug.LogError("[AI-DDA] RL model cannot find PlayerBehaviorLogger!");
                return;
            }

            sensor.AddObservation(currentDifficulty);
            sensor.AddObservation(playerLogger.playerDeaths);
            sensor.AddObservation(playerLogger.enemiesDefeated);
            sensor.AddObservation(playerLogger.potionsUsed);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (playerLogger == null) return;

            float correction = actions.ContinuousActions[0];
            adjustedDifficulty = Mathf.Clamp(currentDifficulty + correction, 1f, 10f);

            DifficultyManager.Instance.SetDifficultyFromAI(adjustedDifficulty);

            if (Time.time - lastLogTime >= logCooldown)
            {
                Debug.Log($"[AI-DDA] MLP prediction: {currentDifficulty}, RL adjustment: {adjustedDifficulty}");
                lastLogTime = Time.time;
            }

            float error = Mathf.Abs(currentDifficulty - adjustedDifficulty);
            float reward = Mathf.Clamp(1.0f - error / 10f, 0f, 1f);
            SetReward(reward);
        }

        public override void OnEpisodeBegin()
        {
            adjustedDifficulty = currentDifficulty;
        }

        public float GetCurrentDifficulty()
        {
            Debug.Log($"[AI-DDA] GetCurrentDifficulty() called, returning: {adjustedDifficulty}");
            return adjustedDifficulty;
        }

        public void SetCurrentDifficulty(float value)
        {
            currentDifficulty = Mathf.Clamp01(value);
        }

        public void AdjustDifficulty(float baseDifficulty)
        {
            lastDecisionTime = Time.time;
            currentDifficulty = baseDifficulty;
            adjustedDifficulty = baseDifficulty;

            RequestDecision();  

            // return adjustedDifficulty;
        }
    }
}