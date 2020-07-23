using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Attributes;
using UnityEngine.UI;

using BeatSaberDiary;
using BeatSaberDiary.Util;
using TMPro;

using UnityEngine;

using HMUI;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using System.Collections;

namespace BeatSaberDiary.UI.ViewController

{
    public class DiaryButtonController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberDiary.UI.View.DiaryButton.bsml";

        [UIAction("press")]
        private void ButtonPress()
        {
            BeatSaberDiary.Log.Write("Hey look, the text changed");
            SetDiaryView();
        }

        private void SetDiaryView()
        {
            new UnityTask(SetDiaryView_Process());
        }

        private static IEnumerator SetDiaryView_Process()
        {
            yield return new WaitForEndOfFrame();

            BeatSaberDiary.Log.Write("SetDiaryView Start");

            var screenSystem = Resources.FindObjectsOfTypeAll<ScreenSystem>()[0];
            //Vector3 pos = new Vector3(8.5f, 1.6f, 0.5f);
            //Vector3 rot = new Vector3(0, 90, 0);
            Vector3 pos = screenSystem.rightScreen.gameObject.transform.position;
            Quaternion rot = screenSystem.rightScreen.gameObject.transform.rotation;

            BeatSaberDiary.Log.Write("pos = " + pos);
            BeatSaberDiary.Log.Write("rot = " + rot);

            //[ERROR @ 16:36:11 | BeatSaberDialy] pos = (4.5, 1.6, 0.5)
            //[ERROR @ 16:36:11 | BeatSaberDialy] rot = (0.0, 120.0, 0.0)

            var floatingScreenForDiary = FloatingScreen.CreateFloatingScreen(new Vector2(150, 100), false, pos + new Vector3(0, 0, -0.05f), rot);

            floatingScreenForDiary.SetRootViewController(BeatSaberUI.CreateViewController<DiaryViewController>(), true);

            var createdController = Resources.FindObjectsOfTypeAll<DiaryViewController>()[0];
            createdController.parent = floatingScreenForDiary;
        }

    }
}
