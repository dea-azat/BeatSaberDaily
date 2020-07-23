using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;

using BeatSaberDiary;
using TMPro;
using BeatSaberMarkupLanguage.FloatingScreen;
using IPA.Config.Data;

using System.Collections.Generic;

namespace BeatSaberDiary.UI.ViewController

{
    public class DiaryViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberDiary.UI.View.DiaryView.bsml";

        public FloatingScreen parent;

        [UIComponent("scoreList")]
        private CustomListTableData scoreList;

        private void Start()
        {
            List<string> scoreDataText = BeatSaberDiary.DiaryData.GetAllTextData();

            foreach(var txt in scoreDataText)
            {
                CustomListTableData.CustomCellInfo cellInfo = new CustomListTableData.CustomCellInfo(txt, "2020/07/23");
                scoreList.data.Add(cellInfo);
            }

            scoreList.tableView.ReloadData();
        }

        [UIAction("press")]
        private void ButtonPress()
        {
            if (parent != null)
            {
                Destroy(parent.gameObject);
            }
        }
    }
}
