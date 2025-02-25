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

        private float lastDecisionTime = 0f;  //    Last decision time of RL
        private float decisionInterval = 5f; // Interval for RL to make decisions (default: every 5 seconds)

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
            Debug.Log("[AI-DDA] RLModel successfully found PlayerBehaviorLogger.");
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

            Debug.Log($"[AI-DDA] MLP prediction: {currentDifficulty}, RL adjustment: {adjustedDifficulty}");

            float error = Mathf.Abs(currentDifficulty - adjustedDifficulty);
            float reward = Mathf.Clamp(1.0f - error / 10f, 0f, 1f);
            SetReward(reward);
        }

        public override void OnEpisodeBegin()
        {
            adjustedDifficulty = currentDifficulty;
        }

        public float AdjustDifficulty(float baseDifficulty)
        {
            // Reinforcement Learning time limiter to limit decision spamming
            if (Time.time - lastDecisionTime >= decisionInterval)
            {
                lastDecisionTime = Time.time;
                currentDifficulty = baseDifficulty;
                adjustedDifficulty = baseDifficulty;
                RequestDecision();

                        // Debug.Log($"[AI-DDA] Adjusting difficulty. Current: {currentDifficulty}, Adjusted: {adjustedDifficulty}, Character: {Game.instance.currentCharacter.name}"); //  DEBUG - In case of issues with currentCharacter ID
            }

            return adjustedDifficulty;
        }
    }
}