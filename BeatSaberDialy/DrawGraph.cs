using IPA.Config.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberDialy
{
    class GraphContainer : MonoBehaviour
    {
        WindowGraph winGraph;
        private AssetBundle assetBundle;

        private void Awake()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("BeatSaberDaily.UI.linegraph"))
            assetBundle = AssetBundle.LoadFromStream(stream);

            var prefab = assetBundle.LoadAsset<GameObject>("LineGraph");
            var sprite = assetBundle.LoadAsset<Sprite>("Circle");
            var go = Instantiate(prefab, transform);

            winGraph = go.AddComponent<WindowGraph>();
            winGraph.circleSprite = sprite;
            winGraph.transform.localScale /= 10;

            List<Vector2> goodGraph = DialyData.GetLastGoodRateGraphPoint();
            Log.Write("GraphContainer goodGraph Count = " + goodGraph.Count.ToString());
            if ( goodGraph.Count > 0)
            {
                Draw(goodGraph);
            }
            else
            {
                //Draw(new List<Vector2> { new Vector2(0f, 1f), new Vector2(1f, 2f), new Vector2(3f, 0f), new Vector2(4f, 5f) });
            }
            
        }

        public void Draw(List<Vector2> points)
        {
            Log.Write("GraphContainer Draw Start");

            winGraph.ShowGraph(points, makeOriginZero: true);
        }

        private void OnDestroy()
        {
            Log.Write("GraphContainer Destroy");
        }
    }

    /* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
    */

    public class WindowGraph : MonoBehaviour
    {
        public Sprite circleSprite;
        private RectTransform _labelTemplateX;
        private RectTransform _labelTemplateY;
        private RectTransform _dashTemplateX;
        private RectTransform _dashTemplateY;

        public RectTransform GraphContainer { get; private set; }
        public List<GameObject> DotObjects { get; private set; }
        public List<GameObject> LinkObjects { get; private set; }
        public List<GameObject> LabelXObjects { get; private set; }
        public List<GameObject> LabelYObjects { get; private set; }
        public List<GameObject> DashXObjects { get; private set; }
        public List<GameObject> DashYObjects { get; private set; }

        private void Awake()
        {
            GraphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
            _labelTemplateX = GraphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
            _labelTemplateY = GraphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
            _dashTemplateX = GraphContainer.Find("DashTemplateX").GetComponent<RectTransform>();
            _dashTemplateY = GraphContainer.Find("DashTemplateY").GetComponent<RectTransform>();

            DotObjects = new List<GameObject>();
            LinkObjects = new List<GameObject>();
            LabelXObjects = new List<GameObject>();
            LabelYObjects = new List<GameObject>();
            DashXObjects = new List<GameObject>();
            DashYObjects = new List<GameObject>();
        }

        public void ShowGraph(List<Vector2> valueList, bool makeDotsVisible = true, bool makeLinksVisible = true,
                              bool makeOriginZero = false, int maxVisibleValueAmount = -1,
                              Func<float, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
        {
            if (getAxisLabelX == null)
                getAxisLabelX = delegate (float _i) { return _i.ToString(); };
            if (getAxisLabelY == null)
                getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };

            if (maxVisibleValueAmount <= 0)
                maxVisibleValueAmount = valueList.Count;

            if (DotObjects != null)
            {
                foreach (var go in DotObjects)
                    Destroy(go);
                DotObjects.Clear();
            }
            if (LinkObjects != null)
            {
                foreach (var go in LinkObjects)
                    Destroy(go);
                LinkObjects.Clear();
            }
            if (LabelXObjects != null)
            {
                foreach (var go in LabelXObjects)
                    Destroy(go);
                LabelXObjects.Clear();
            }
            if (LabelYObjects != null)
            {
                foreach (var go in LabelYObjects)
                    Destroy(go);
                LabelYObjects.Clear();
            }
            if (DashXObjects != null)
            {
                foreach (var go in DashXObjects)
                    Destroy(go);
                DashXObjects.Clear();
            }
            if (DashYObjects != null)
            {
                foreach (var go in DashYObjects)
                    Destroy(go);
                DashYObjects.Clear();
            }

            var graphWidth = GraphContainer.sizeDelta.x;
            var graphHeight = GraphContainer.sizeDelta.y;

            var yMaximum = valueList[0].y;
            var yMinimum = valueList[0].y;

            var xMaximum = valueList[valueList.Count - 1].x;
            var xMinimum = valueList[0].x;

            for (var i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
            {
                var value = valueList[i];
                if (value.y > yMaximum)
                    yMaximum = value.y;
                if (value.y < yMinimum)
                    yMinimum = value.y;
            }

            var yDifference = yMaximum - yMinimum;
            if (yDifference <= 0)
                yDifference = 5f;
            yMaximum = yMaximum + (yDifference * 0.2f);
            yMinimum = yMinimum - (yDifference * 0.2f);

            if (makeOriginZero)
                yMinimum = 0f; // Start the graph at zero

            var xSize = graphWidth / (maxVisibleValueAmount + 1);
            var xIndex = 0;

            GameObject lastCircleGameObject = null;
            for (var i = Mathf.Max(valueList.Count - maxVisibleValueAmount, 0); i < valueList.Count; i++)
            {
                var xPosition = (valueList[i].x - xMinimum) / (xMaximum - xMinimum) * graphWidth;
                var yPosition = (valueList[i].y - yMinimum) / (yMaximum - yMinimum) * graphHeight;
                var circleGameObject = CreateCircle(new Vector2(xPosition, yPosition), makeDotsVisible);
                DotObjects.Add(circleGameObject);
                if (lastCircleGameObject != null)
                {
                    var dotConnectionGameObject = CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition,
                                                                      circleGameObject.GetComponent<RectTransform>().anchoredPosition,
                                                                      makeLinksVisible);
                    LinkObjects.Add(dotConnectionGameObject);
                }
                lastCircleGameObject = circleGameObject;

                var labelX = Instantiate(_labelTemplateX);
                labelX.SetParent(GraphContainer, false);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPosition, -7f);
                labelX.GetComponent<UnityEngine.UI.Text>().text = getAxisLabelX(i);
                LabelXObjects.Add(labelX.gameObject);

                var dashX = Instantiate(_dashTemplateX);
                dashX.SetParent(GraphContainer, false);
                dashX.gameObject.SetActive(true);
                dashX.anchoredPosition = new Vector2(yPosition, -3);
                DashXObjects.Add(dashX.gameObject);

                xIndex++;
            }

            var separatorCount = 10;
            for (var i = 0; i <= separatorCount; i++)
            {
                var labelY = Instantiate(_labelTemplateY);
                labelY.SetParent(GraphContainer, false);
                labelY.gameObject.SetActive(true);
                var normalizedValue = i * 1f / separatorCount;
                labelY.anchoredPosition = new Vector2(-7f, normalizedValue * graphHeight);
                labelY.GetComponent<UnityEngine.UI.Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
                LabelYObjects.Add(labelY.gameObject);

                var dashY = Instantiate(_dashTemplateY);
                dashY.SetParent(GraphContainer, false);
                dashY.gameObject.SetActive(true);
                dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
                DashYObjects.Add(dashY.gameObject);
            }
        }

        private GameObject CreateCircle(Vector2 anchoredPosition, bool makeDotsVisible)
        {
            var gameObject = new GameObject("Circle", typeof(Image));
            gameObject.transform.SetParent(GraphContainer, false);
            var image = gameObject.GetComponent<Image>();
            image.sprite = circleSprite;
            image.useSpriteMesh = true;
            image.enabled = makeDotsVisible;
            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(8, 8);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            return gameObject;
        }

        private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB, bool makeLinkVisible)
        {
            var gameObject = new GameObject("DotConnection", typeof(Image));
            gameObject.transform.SetParent(GraphContainer, false);
            var image = gameObject.GetComponent<Image>();
            image.color = new Color(1, 1, 1, .5f);
            image.enabled = makeLinkVisible;
            var rectTransform = gameObject.GetComponent<RectTransform>();
            var dir = (dotPositionB - dotPositionA).normalized;
            var distance = Vector2.Distance(dotPositionA, dotPositionB);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 2f);
            rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            return gameObject;
        }
    }
}
