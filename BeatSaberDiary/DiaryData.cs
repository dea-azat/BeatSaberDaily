﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using System.IO;
using UnityEngine.PlayerLoop;

namespace BeatSaberDiary
{
    public static class DiaryData
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

        public static void LevelCleared(String songName, float songDuration)
        {
            nowPlayData.LevelCleared(songName, songDuration);
        }

        public static List<Vector2> GetLastGoodRateGraphPoint()
        {
            return lastPlayData.GetGoodRateGraphPoint();
        }

        public static String GetAllTextData()
        {
            //string date = DateTime.Now.ToString("yyyy_MM_dd_");
            string date = "2020_07_21_";
            string filepath = "D:/BeatSaberMod/" + date + "record.csv";
            string text = "";

            StreamReader file = new StreamReader(filepath, Encoding.UTF8);

            string line = "";
            line = file.ReadLine();
            // To Be Section Data
            int goodCnt = 0;
            int notGoodCnt = 0;

            while (line != null)
            {
                Debug.Log(line);
                string[] word = line.Split(","[0]);

                if (word.Length == 1)
                {
                    if (goodCnt + notGoodCnt > 0)
                    {
                        text += " [GoodRate: " + ((float)goodCnt / (float)(goodCnt + notGoodCnt)) + "]\n";
                    }
                    text += "[Title: " + word[0] + "]";
                    goodCnt = notGoodCnt = 0;
                }
                else
                {
                    goodCnt += int.Parse(word[2]);
                    notGoodCnt += int.Parse(word[3]);
                    notGoodCnt += int.Parse(word[4]);
                }

                line = file.ReadLine();
            }

            text += " [GoodRate: " + ((float)goodCnt / (float)(goodCnt + notGoodCnt)) + "]";

            return text;
        }
    }

    public class SectionData
    {
        int goodcut;
        int badcut;
        int missed;

        float startTime;
        float endTime;

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

        public void SetEndTime(float _endTime)
        {
            endTime = _endTime;
        }

        public String Data2Text()
        {
            Log.Write("[     time ] = " + startTime.ToString());
            Log.Write("[ good cut ] = " + goodcut.ToString());
            Log.Write("[  bad cut ] = " + badcut.ToString());
            Log.Write("[   missed ] = " + missed.ToString());
            String txt = string.Concat(new string[]
            {
                startTime.ToString(),
                ",",
                endTime.ToString(),
                ",",
                goodcut.ToString(),
                ",",
                badcut.ToString(),
                ",",
                missed.ToString(),
            });

            return txt;
        }

        public Vector2 Data2PointForGoodRate()
        {
            Vector2 vec = new Vector2();

            vec.x = endTime;
            vec.y = (float)goodcut / (float)(goodcut + badcut + missed);

            return vec;
        }
    }

    public class PlayData
    {
        String songName;

        List<SectionData> sectionData = new List<SectionData>();
        
        int cutCount = 0;

        const int COUNT_MAX = 20; //Must to be DI

        // NoteCut
        private void UpdateSectionData(float time)
        {
            int index = cutCount / COUNT_MAX;
            if (cutCount % COUNT_MAX == 0)
            {
                if (cutCount == 0)
                {
                    sectionData.Add(new SectionData(0));
                }
                else
                {
                    sectionData[index - 1].SetEndTime(time);
                    sectionData.Add(new SectionData(time));
                }
            }
        }

        public void NoteWasCut(NoteData noteData, NoteCutInfo noteCutInfo, int multiplier)
        {
            bool isNotNote = noteData.noteType != NoteType.NoteA && noteData.noteType != NoteType.NoteB;
            if (isNotNote) return;

            UpdateSectionData(noteData.time);

            int index = cutCount / COUNT_MAX;
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

            UpdateSectionData(noteData.time);

            int index = cutCount / COUNT_MAX;
            sectionData[index].InclementMissed();
            cutCount++;
        }

        public void WritePlayData(String filepath)
        {
            StreamWriter file = new StreamWriter(filepath, true, Encoding.UTF8);
            Logger.log.Error("LevelClearEvent");
            file.WriteLine(songName);
            foreach (SectionData sd in sectionData)
            {
                String txt = sd.Data2Text();
                file.WriteLine(txt);
            }
            file.Close();
        }

        public void LevelCleared(String _songName, float songDuration)
        {
            songName = _songName;
            sectionData[cutCount / COUNT_MAX].SetEndTime(songDuration);
        }

        public List<Vector2> GetGoodRateGraphPoint()
        {
            List<Vector2> vec = new List<Vector2>();

            vec.Add(new Vector2(0f, 1f));

            foreach (SectionData sd in sectionData)
            {
                vec.Add(sd.Data2PointForGoodRate());
            }

            return vec;
        }
    }
}