using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;

using BeatSaberDiary;
using TMPro;
using BeatSaberMarkupLanguage.FloatingScreen;

namespace BeatSaberDiary.UI.ViewController

{
    public class DiaryViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberDiary.UI.View.DiaryView.bsml";

        public FloatingScreen parent;

        [UIComponent("today-scores")]
        private TextMeshProUGUI todayText;

        private void Start()
        {
            todayText.text = BeatSaberDiary.DiaryData.GetAllTextData();
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
