using System.Diagnostics;
using System.IO;
using UnityEngine;
using System.Linq;

namespace AI_DDA.Assets.Scripts
{
    public class AIModel : MonoBehaviour
    {
        public static AIModel Instance;

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

        public float PredictDifficulty(PlayerBehaviorLogger playerLogger)
        {
            string pythonPath = "/home/tr4spy/.pyenv/shims/python"; // Jeśli masz inną wersję, np. "python3.8"
            string scriptPath = Path.Combine(Application.streamingAssetsPath, "ai_model.py");
            string modelPath = Path.Combine(Application.streamingAssetsPath, "ai_model.pkl"); // Ścieżka do modelu

            // Sprawdzenie czy skrypt i model istnieją
            if (!File.Exists(scriptPath))
            {
                UnityEngine.Debug.LogError($"AI-DDA: Script file not found at {scriptPath}");
                return 5.0f; // Domyślna trudność
            }

            if (!File.Exists(modelPath))
            {
                UnityEngine.Debug.LogError($"AI-DDA: Model file not found at {modelPath}");
                return 5.0f; // Domyślna trudność
            }

            // Argumenty do Pythona (statystyki gracza + ścieżka modelu)
            string arguments = $"{playerLogger.playerDeaths} {playerLogger.enemiesDefeated} {playerLogger.totalCombatTime.ToString(System.Globalization.CultureInfo.InvariantCulture)} {playerLogger.npcInteractions} " +
                   $"{playerLogger.questsCompleted} {playerLogger.waypointsDiscovered} {playerLogger.zonesDiscovered} {playerLogger.unlockedAchievements.Count}";

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\" {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true, // Przechwytywanie błędów Pythona
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                string result = "";
                string errorOutput = "";

                using (StreamReader reader = process.StandardOutput)
                {
                    result = reader.ReadToEnd().Trim();
                }

                using (StreamReader errorReader = process.StandardError)
                {
                    errorOutput = errorReader.ReadToEnd().Trim();

                    if (!string.IsNullOrEmpty(errorOutput) && !errorOutput.Contains("Loading model from"))
                    {
                        UnityEngine.Debug.LogError($"[AI-DDA] Python Error: {errorOutput}");
                    }
                }

                // Debugowanie - sprawdzamy co zwraca Python
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    UnityEngine.Debug.LogError($"[AI-DDA] Python Error: {errorOutput}");
                    return 5.0f; // Jeśli Python zwrócił błąd, zwracamy domyślną trudność
                }

                UnityEngine.Debug.Log($"[AI-DDA] Raw Python Output: '{result}'");
                UnityEngine.Debug.Log($"[AI-DDA] Raw Python Output Length: {result.Length}");
                UnityEngine.Debug.Log($"[AI-DDA] Raw Python Output (ASCII): [{string.Join(",", result.Select(c => ((int)c).ToString()))}]");

                result = result.Replace("'", "").Trim();
                result = result.Split('\n')[0].Trim();
                result = result.Replace(',', '.');
                
                if (float.TryParse(result, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float difficulty))
                {
                    UnityEngine.Debug.Log($"[AI-DDA] Predicted Difficulty from Python: {difficulty}");
                    return difficulty;
                }
                else
                {
                    UnityEngine.Debug.LogError($"AI-DDA: Failed to parse difficulty prediction: '{result}'");
                    return 5.0f; // Domyślna wartość, jeśli coś poszło nie tak
                }
            }
        }
    }
}