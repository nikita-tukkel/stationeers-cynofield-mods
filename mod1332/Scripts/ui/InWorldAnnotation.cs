using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class InWorldAnnotation : MonoBehaviour
    {
        GameObject obj;
        Canvas canvas;
        RawImage bkgd;
        TextMeshProUGUI text;
        Thing anchor;

        ThingsUi thingsUi;

        public string id;

        public void Inject(ThingsUi thingsUi)
        {
            this.thingsUi = thingsUi;
        }

        void Start()
        {
            //ConsoleWindow.Print($"InWorldAnnotation Start");
            obj = new GameObject("0");
            obj.SetActive(false);
            obj.transform.parent = gameObject.transform;
            canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            //canvas.pixelPerfect = true; // for RenderMode.ScreenSpaceOverlay

            var canvasTransform = canvas.transform;
            canvasTransform.localScale = Vector3.one * 0.5f; // less than 0.5 looks bad

            bkgd = new GameObject("1").AddComponent<RawImage>();
            bkgd.rectTransform.SetParent(canvas.transform, false);
            bkgd.rectTransform.sizeDelta = new Vector2(1f, 1f);

            // https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html
            text = new GameObject("2").AddComponent<TextMeshProUGUI>();
            text.rectTransform.SetParent(canvas.transform, false);
            text.rectTransform.sizeDelta = bkgd.rectTransform.sizeDelta;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.richText = true;
            text.margin = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = false;

            // Resources.Load<TMP_FontAsset>(string.Format("UI/{0}", this.FontName))
            // Font font = new Font("StreamingAssets/Fonts/3270-Regular.ttf");
            // var tfont = TMP_FontAsset.CreateFontAsset(font);
            // text.font = tfont;
            //ConsoleWindow.Print($"{text.font.name}");
            text.font = Localization.CurrentFont;
            //0.08f for default font used by TextMeshProUGUI without font specified;
            //0.06f when using Localization.CurrentFont
            text.fontSize = 0.06f;

            ApplyNextColorScheme(bkgd, text);
        }

        private static int ColorSchemeId = 0;
        private static void ApplyNextColorScheme(RawImage bkgd, TextMeshProUGUI text)
        {
            ApplyColorScheme(ColorSchemeId++, bkgd, text);
        }

        private static void ApplyColorScheme(int i, RawImage bkgd, TextMeshProUGUI text)
        {
            switch (i)
            {
                case 1:
                    {
                        // bkgd.color = new Color(0xad / 255f, 0xd8 / 255f, 0xe6 / 255f, 0.2f);
                        bkgd.color = new Color(0xad / 255f, 0xad / 255f, 0xe6 / 255f, 0.2f);
                        text.alpha = 0.8f;
                        text.color = new Color(0f, 0f, 0f, 0.8f); // low alpha is used to hide font antialiasing artifacts.
                        text.fontStyle = FontStyles.Bold;
                        break;
                    }
                // case 2:
                //     {
                //         break;
                //     }
                default:
                    {
                        ColorSchemeId = 1; // reset to the first, so the sequence after restart is 0, 1, 2...., 0.

                        bkgd.color = new Color(0.2f, 0.3f, 0.7f, 0.4f);
                        text.alpha = 0.2f;
                        text.color = new Color(1f, 1f, 1f, 0.1f); // low alpha is used to hide font antialiasing artifacts.
                        text.fontStyle = FontStyles.Bold;
                        break;
                    }
            }
        }

        public void ShowNear(Thing thing, string id, RaycastHit hit)
        {
            if (thing == null)
            {
                Hide();
                return;
            }
            if (thing == anchor)
            {
                return;
            }
            this.anchor = thing;
            this.id = id;

            // relink to new parent, thus appear in the parent scene.
            transform.SetParent(thing.transform, false);
            transform.SetPositionAndRotation(
                // need to reset position on this component reuse
                thing.transform.position + Vector3.zero,

                // rotate vertically to the camera:
                Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0)
                );

            Render();

            // Fine tune coordinates only after content is rendered.
            // Expect that `bkgd` is here and covers whole annotation.
            var posHit = hit.point;
            //var posHead = InventoryManager.ParentHuman.HeadBone.transform.position;
            var posHead = InventoryManager.ParentHuman.GlassesSlot.Occupant.transform.position;
            var posLegs = InventoryManager.ParentHuman.transform.position;
            var humanHeight = (posHead.y - posLegs.y) * 1.2f;
            var limit1 = posLegs.y;
            var limit2 = limit1 + humanHeight;
            transform.position = new Vector3(posHit.x, posHit.y, posHit.z);
            Vector3[] corners = new Vector3[4];
            bkgd.rectTransform.GetWorldCorners(corners);
            var y1 = corners[0].y; // bottom left
            var y2 = corners[2].y; // top right
            var height = y2 - y1;
            var pos = transform.position;
            if (y2 > limit2)
            {
                // ConsoleWindow.Print($"ShowNear {y2} > {posHead.y} ; pos.y={pos.y}, y1={y1}, y2={y2}; head={posHead.y}, legs={posLegs.y}");
                transform.position = new Vector3(pos.x, limit2, pos.z);
            }
            else if (y1 < limit1)
            {
                // ConsoleWindow.Print($"ShowNear {y1} < {posLegs.y} ; pos.y={pos.y}, y1={y1}, y2={y2}; head={posHead.y}, legs={posLegs.y}");
                transform.position = new Vector3(posHit.x, limit1 + height, posHit.z);
            }
            transform.Translate(Camera.main.transform.forward * -0.5f, Space.World);

            obj.SetActive(true);
            gameObject.SetActive(true);
        }

        public void Render()
        {
            thingsUi.RenderArAnnotation(anchor, canvas, text);
        }

        public bool IsActive()
        {
            if (GameManager.GameState != Assets.Scripts.GridSystem.GameState.Running)
                return false;
            if (InventoryManager.ParentHuman == null)
                return false;

            return this.isActiveAndEnabled && anchor != null;
        }

        public void Hide()
        {
            anchor = null;
            gameObject.SetActive(false);
        }

        public void Destroy()
        {
            Destroy(gameObject);
            Destroy(obj);
        }
    }
}
