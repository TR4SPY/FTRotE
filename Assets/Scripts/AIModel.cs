using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Diagnostics;
using System;
using System.IO;
using PLAYERTWO.ARPGProject;

namespace AI_DDA.Assets.Scripts
{
    public class AIModel : MonoBehaviour
    {
        public static AIModel Instance;
        private string rlServerUrl = "http://localhost:5000"; // RL Serwer

        private void Awake()
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

        /// <summary>
        /// Pobiera `totalPlayTime` z `Game.instance.currentCharacter.totalPlayTime`
        /// </summary>
        private float GetTotalPlayTime()
        {
            if (Game.instance != null && Game.instance.currentCharacter != null)
            {
                return Game.instance.currentCharacter.totalPlayTime;
            }
            else
            {
                UnityEngine.Debug.LogError("[AI-DDA] Error: Game instance or character is null!");
                return 0.0f; // Jeśli coś nie działa, zwracamy 0
            }
        }

        /// <summary>
        /// Predykcja początkowej trudności (XGBoost)
        /// </summary>
        public float PredictDifficulty(PlayerBehaviorLogger playerLogger)
        {
            string pythonPath = "/home/tr4spy/.pyenv/shims/python";
            string scriptPath = Path.Combine(Application.streamingAssetsPath, "ai_model.py");

            float totalPlayTime = GetTotalPlayTime(); // Pobieramy totalPlayTime z `Game.instance`

            // Argumenty dla XGBoost
            string arguments = $"{totalPlayTime.ToString(System.Globalization.CultureInfo.InvariantCulture)} " +
                               $"{playerLogger.playerDeaths} {playerLogger.enemiesDefeated} {playerLogger.totalCombatTime.ToString(System.Globalization.CultureInfo.InvariantCulture)} " +
                               $"{playerLogger.npcInteractions} {playerLogger.potionsUsed} {playerLogger.questsCompleted} " +
                               $"{playerLogger.waypointsDiscovered} {playerLogger.zonesDiscovered} {playerLogger.unlockedAchievements.Count}";

            UnityEngine.Debug.Log($"[AI-DDA] Sending XGBoost Prediction Request: {arguments}");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\" {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                string result = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (float.TryParse(result, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float difficulty))
                {
                    UnityEngine.Debug.Log($"[AI-DDA] Initial XGBoost Difficulty: {difficulty}");
                    return difficulty;
                }
                else
                {
                    UnityEngine.Debug.LogError($"AI-DDA: Failed to parse difficulty prediction: '{result}'");
                    return 5.0f; // Domyślna wartość
                }
            }
        }

        /// <summary>
        /// Dynamiczne dostosowanie trudności przez RL
        /// </summary>
        public void SendStatsToRL(PlayerBehaviorLogger logger)
        {
            StartCoroutine(SendStatsCoroutine(logger));
        }

        private IEnumerator SendStatsCoroutine(PlayerBehaviorLogger logger)
        {
            float totalPlayTime = GetTotalPlayTime();

            string jsonData = JsonUtility.ToJson(new RLData(logger, totalPlayTime));

            using (UnityWebRequest request = new UnityWebRequest(rlServerUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    RLResponse response = JsonUtility.FromJson<RLResponse>(responseText);
                    DifficultyManager.Instance.SetDifficultyFromAI(response.difficulty);
                    UnityEngine.Debug.Log($"[AI-DDA] RL Adjusted Difficulty to: {response.difficulty}");
                }
                else
                {
                    UnityEngine.Debug.LogError($"[AI-DDA] RL Communication Failed: {request.error}");
                }
            }
        }
    }

    [Serializable]
    public class RLData
    {
        public float totalPlayTime;
        public int playerDeaths;
        public int enemiesDefeated;
        public float totalCombatTime;
        public int npcInteractions;
        public int potionsUsed;
        public int questsCompleted;
        public int waypointsDiscovered;
        public int zonesDiscovered;
        public int unlockedAchievements;

        public RLData(PlayerBehaviorLogger logger, float playTime)
        {
            totalPlayTime = playTime;
            playerDeaths = logger.playerDeaths;
            enemiesDefeated = logger.enemiesDefeated;
            totalCombatTime = logger.totalCombatTime;
            npcInteractions = logger.npcInteractions;
            potionsUsed = logger.potionsUsed;
            questsCompleted = logger.questsCompleted;
            waypointsDiscovered = logger.waypointsDiscovered;
            zonesDiscovered = logger.zonesDiscovered;
            unlockedAchievements = logger.unlockedAchievements.Count;
        }
    }

    [Serializable]
    public class RLResponse
    {
        public float difficulty;
    }
}
