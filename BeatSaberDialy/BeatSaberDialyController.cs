using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using System.IO;
using BS_Utils.Utilities;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberDialy.UI.ViewController;
using BeatSaberDialy.Util;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

namespace BeatSaberDialy
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    /// 

    public static class Log
    {
        public static void Write(String text)
        {
            Logger.log.Error(text);
        }
    }

    public class BeatSaberDialyController : MonoBehaviour
    {
        public static BeatSaberDialyController instance { get; private set; }

        static FloatingScreen floatingScreenForScore;

        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            if (instance != null)
            {
                Logger.log?.Warn($"Instance of {this.GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            instance = this;
            Logger.log?.Debug($"{name}: Awake()");

            DialyData.Init();
            BSEvents.levelCleared += LevelClearEvent;
            BSEvents.levelFailed += LevelClearEvent;
            BSEvents.noteWasCut += HandleScoreControllerNoteWasCut;
            BSEvents.noteWasMissed += HandleScoreControllerNoteWasMissed;

            BSEvents.lateMenuSceneLoadedFresh += SetGoodRateChart;

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            Log.Write("Stats OnLoad");
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Log.Write("Scene Loaded Name = " + arg0.name);
        }

        private void HandleScoreControllerNoteWasCut(NoteData noteData, NoteCutInfo noteCutInfo, int multiplier)
        {
            DialyData.NoteWasCut(noteData, noteCutInfo, multiplier);
        }

        private void HandleScoreControllerNoteWasMissed(NoteData noteData, int multiplier)
        {
            DialyData.NoteWasMissed(noteData, multiplier);
        }

        private void LevelClearEvent(StandardLevelScenesTransitionSetupDataSO data, LevelCompletionResults result)
        {

            var _mainGameSceneSetupData = BS_Utils.Plugin.LevelData;
            var _beatmapData = _mainGameSceneSetupData?.GameplayCoreSceneSetupData?.difficultyBeatmap?.beatmapData;

            if (_beatmapData != null)
            {
                var songDuration = _mainGameSceneSetupData?.GameplayCoreSceneSetupData?.difficultyBeatmap?.level?.beatmapLevelData?.audioClip?.length ?? -1f;
                var songName = _mainGameSceneSetupData.GameplayCoreSceneSetupData.difficultyBeatmap.level.songName;
                DialyData.LevelCleared(songName, songDuration);
            }

            String filepath = "D:/BeatSaberMod/record.csv";

            DialyData.WritePlayData(filepath);

            DialyData.Update();

            List<Vector2> goodGraph = DialyData.GetLastGoodRateGraphPoint();
            Log.Write("LevelClearEvent goodGraph Count = " + goodGraph.Count.ToString());

            floatingScreenForScore.rootViewController.gameObject.SetActive(true);
            floatingScreenForScore.GetComponent<GraphContainer>().Draw(DialyData.GetLastGoodRateGraphPoint());
        }

        private void SetGoodRateChart(ScenesTransitionSetupDataSO data)
        {
            new UnityTask(SetFloatingDisplay());
        }

        private static IEnumerator SetFloatingDisplay()
        {
            yield return new WaitForEndOfFrame();

            Log.Write("SetFloatingDisplay Start");

            Vector3 ChartStandardLevelPosition = new Vector3(0, 0.25f, 2.25f); /* Original: 0, -0.4, 2.25 */
            Vector3 ChartStandardLevelRotation = new Vector3(35, 0, 0);

            var pos = ChartStandardLevelPosition;
            var rot = Quaternion.Euler(ChartStandardLevelRotation);
            floatingScreenForScore = FloatingScreen.CreateFloatingScreen(new Vector2(105, 65), false, pos, rot);


            floatingScreenForScore.SetRootViewController(BeatSaberUI.CreateViewController<GoodRateViewController>(), true);

            Image image = floatingScreenForScore.GetComponent<Image>();
            image.enabled = false;

            floatingScreenForScore.gameObject.AddComponent<GraphContainer>();

            floatingScreenForScore.rootViewController.gameObject.SetActive(false);
        }

        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {

        }

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private void Update()
        {

        }

        /// <summary>
        /// Called every frame after every other enabled script's Update().
        /// </summary>
        private void LateUpdate()
        {

        }

        /// <summary>
        /// Called when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {

        }

        /// <summary>
        /// Called when the script becomes disabled or when it is being destroyed.
        /// </summary>
        private void OnDisable()
        {

        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Logger.log?.Debug($"{name}: OnDestroy()");
            instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.

        }
        #endregion
    }
}
