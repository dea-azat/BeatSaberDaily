using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using System.IO;

namespace BeatSaberDaily
{
    public static class DailyData
    {
        static List<PlayData> playData = new List<PlayData>();

        static PlayData nowPlayData;
        static PlayData lastPlayData;

        // Add/Delete/Read/Write
        public static void Init()
        {
            nowPlayData = new PlayData();
            lastPlayData = new PlayData();
        }
        static public void Update()
        {
            playData.Add(nowPlayData);
            lastPlayData = nowPlayData;
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

        public static List<Vector2> GetLastGoodRateGraphPoint()
        {
            return lastPlayData.GetGoodRateGraphPoint();
        }
    }

    public class SectionData
    {
        int goodcut;
        int badcut;
        int missed;

        float startTime;

        public SectionData(float time)
        {
            startTime = time;

            goodcut = badcut = missed = 0;
        }

        public void InclementGoodCut()
        {
            goodcut++;
        }

        public void InclementBadCut()
        {
            badcut++;
        }

        public void InclementMissed()
        {
            missed++;
        }

        public String Data2Text()
        {
            Logger.log.Error("[     time ] = " + startTime.ToString());
            Logger.log.Error("[ good cut ] = " + goodcut.ToString());
            Logger.log.Error("[  bad cut ] = " + badcut.ToString());
            Logger.log.Error("[   missed ] = " + missed.ToString());
            String txt = string.Concat(new string[]
            {
                goodcut.ToString(),
                ",",
                badcut.ToString(),
                ",",
                missed.ToString()
            });

            return txt;
        }

        public Vector2 Data2PointForGoodRate()
        {
            Vector2 vec = new Vector2();

            vec.x = startTime;
            vec.y = (float)goodcut / (float)(goodcut + badcut + missed);

            return vec;
        }
    }

    public class PlayData
    {
        List<SectionData> sectionData = new List<SectionData>();
        int cutCount = 0;

        const int COUNT_MAX = 20; //Must to be DI

        // NoteCut

        public void NoteWasCut(NoteData noteData, NoteCutInfo noteCutInfo, int multiplier)
        {
            bool isNotNote = noteData.noteType != NoteType.NoteA && noteData.noteType != NoteType.NoteB;
            if (isNotNote) return;

            int index = cutCount / COUNT_MAX;
            if (cutCount % COUNT_MAX == 0)
            {
                sectionData.Add(new SectionData(noteData.time));
            }

            bool allIsOK = noteCutInfo.allIsOK;
            if (allIsOK)
            {
                sectionData[index].InclementGoodCut();
            }
            else
            {
                sectionData[index].InclementBadCut();
            }
            cutCount++;
        }

        public void NoteWasMissed(NoteData noteData, int multiplier)
        {
            bool isNotNote = noteData.noteType != NoteType.NoteA && noteData.noteType != NoteType.NoteB;
            if (isNotNote) return;

            int index = cutCount / COUNT_MAX;
            if (cutCount % COUNT_MAX == 0)
            {
                sectionData.Add(new SectionData(noteData.time));
            }

            sectionData[index].InclementMissed();
            cutCount++;
        }

        public void WritePlayData(String filepath)
        {
            StreamWriter file = new StreamWriter(filepath, false, Encoding.UTF8);
            Logger.log.Error("LevelClearEvent");
            foreach (SectionData sd in sectionData)
            {
                String txt = sd.Data2Text();
                file.WriteLine(txt);
            }
            file.Close();
        }

        public List<Vector2> GetGoodRateGraphPoint()
        {
            List<Vector2> vec = new List<Vector2>();

            foreach (SectionData sd in sectionData)
            {
                vec.Add(sd.Data2PointForGoodRate());
            }

            return vec;
        }
    }
}
