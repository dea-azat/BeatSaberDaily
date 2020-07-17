using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using System.IO;
using BS_Utils.Utilities;

namespace BeatSaberDaily
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    /// 

    public static class DailyData
    {
        static List<PlayData> playData = new List<PlayData>();

        static PlayData nowPlayData;

        // Add/Delete/Read/Write
        public static void Init()
        {
            nowPlayData = new PlayData();
        }
        static public void Update()
        {
            playData.Add(nowPlayData);
            nowPlayData = new PlayData();
        }

        // PlayData Interface
        public static void NoteWasCut(NoteData noteData, NoteCutInfo noteCutInfo, int multiplier)
        {
            nowPlayData.NoteWasCut(noteData, noteCutInfo, multiplier);
        }

        public static void NoteWasMissed(NoteData noteData, int multiplier)
        {
            nowPlayData.NoteWasMissed(noteData, multiplier);
        }

        public static void WritePlayData(String filepath)
        {
            nowPlayData.WritePlayData(filepath);
        }
    }

    public class PlayData
    {
        List<int> goodcut = new List<int>();
        List<int> badcut = new List<int>();
        List<int> missed = new List<int>();
        int cutCount = 0;

        const int COUNT_MAX = 20; //Must to be DI

        // NoteCut

        public void NoteWasCut(NoteData noteData, NoteCutInfo noteCutInfo, int multiplier)
        {
            bool isNotNote = noteData.noteType != NoteType.NoteA && noteData.noteType != NoteType.NoteB;
            if (isNotNote) return;

            int index = cutCount / COUNT_MAX;
            bool flag2 = cutCount % COUNT_MAX == 0;
            if (flag2)
            {
                goodcut.Add(0);
                badcut.Add(0);
                missed.Add(0);
            }

            bool allIsOK = noteCutInfo.allIsOK;
            if (allIsOK)
            {
                int count = goodcut[index];
                goodcut[index] = count + 1;
            }
            else
            {
                int count2 = badcut[index];
                badcut[index] = count2 + 1;
            }
            cutCount++;
        }

        public void NoteWasMissed(NoteData noteData, int multiplier)
        {
        bool isNotNote = noteData.noteType != NoteType.NoteA && noteData.noteType != NoteType.NoteB;
        if (isNotNote) return;

            int index = cutCount / COUNT_MAX;
            bool flag2 = cutCount % COUNT_MAX == 0;
            if (flag2)
            {
                goodcut.Add(0);
                badcut.Add(0);
                missed.Add(0);
            }
            int count = missed[index];
            missed[index] = count + 1;
            cutCount++;
        }
        
        public void WritePlayData(String filepath)
        {
            StreamWriter file = new StreamWriter(filepath, false, Encoding.UTF8);
            Logger.log.Error("LevelClearEvent");
            for (int i = 0; i < goodcut.Count; i++)
            {
                Logger.log.Error("[ good cut ] = " + goodcut[i].ToString());
                Logger.log.Error("[  bad cut ] = " + badcut[i].ToString());
                Logger.log.Error("[   missed ] = " + missed[i].ToString());
                file.WriteLine(string.Concat(new string[]
                {
                goodcut[i].ToString(),
                ",",
                badcut[i].ToString(),
                ",",
                missed[i].ToString()
                }));
            }
            file.Close();
        }
    }


    public class BeatSaberDailyController : MonoBehaviour
    {
        public static BeatSaberDailyController instance { get; private set; }

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

            DailyData.Init();
            BSEvents.levelCleared += LevelClearEvent;
            BSEvents.levelFailed += LevelClearEvent;
            BSEvents.noteWasCut += HandleScoreControllerNoteWasCut;
            BSEvents.noteWasMissed += HandleScoreControllerNoteWasMissed;
            Logger.log.Error("Stats OnLoad");
        }

        private static void HandleScoreControllerNoteWasCut(NoteData noteData, NoteCutInfo noteCutInfo, int multiplier)
        {
            DailyData.NoteWasCut(noteData, noteCutInfo, multiplier);
        }

        public void HandleScoreControllerNoteWasMissed(NoteData noteData, int multiplier)
        {
            DailyData.NoteWasMissed(noteData, multiplier);
        }

        private static void LevelClearEvent(StandardLevelScenesTransitionSetupDataSO data, LevelCompletionResults result)
        {
            String filepath = "D:/BeatSaberMod/record.csv";

            DailyData.WritePlayData(filepath);

            DailyData.Update();
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
