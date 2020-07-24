using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;
using HMUI;

using BeatSaberDiary;
using TMPro;
using BeatSaberMarkupLanguage.FloatingScreen;
using IPA.Config.Data;

using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberDiary.UI.ViewController

{
    public class DiaryViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberDiary.UI.View.DiaryView.bsml";

        public FloatingScreen parent;
        public BeatSaberDiaryController diaryController;

        List<PlayData> diaryPlayData;

        [UIComponent("scoreList")]
        private CustomListTableData scoreList;

        private void Start()
        {
            //List<string> scoreDataText = BeatSaberDiary.DiaryData.GetAllTextData();
            diaryPlayData = BeatSaberDiary.DiaryData.GetAllPlayData();

            foreach (var data in diaryPlayData)
            {
                string txt = "[Title: " + data.GetSongName() + "] [GoodRate: " + data.GetTotalGoodRate() + "]";
                CustomListTableData.CustomCellInfo cellInfo = new CustomListTableData.CustomCellInfo(txt, "2020/07/23"); //To be change playData date
                scoreList.data.Add(cellInfo);
            }

            scoreList.tableView.ReloadData();
        }

        [UIAction("scoreSelect")]
        private void ListItemSelected(TableView tableView, int index)
        {
            BeatSaberDiary.Log.Write("scoreSelect " + index);
            BeatSaberDiary.DiaryData.UpdateGoodRate(diaryPlayData[index]);
            diaryController.SetActiveGoodRateChart(true, transform);
        }

        [UIAction("press")]
        private void ButtonPress()
        {
            if (parent != null)
            {
                diaryController.SetActiveGoodRateChart(false, transform);
                Destroy(parent.gameObject);
            }
        }
    }
}
