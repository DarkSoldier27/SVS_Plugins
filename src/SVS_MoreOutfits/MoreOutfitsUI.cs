using BepInEx;
using CharacterCreation;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SV;
using SV.CoordeSelectScene;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SVS_MoreOutfits
{
    internal class MoreOutfitsUI
    {
        //private static Texture2D OutfitSprite { get; set; }
        private static readonly List<Toggle> tglGroup = new();

        public static void CreateNewOutfitIcons(List<string> outfitsList, int maxOutfits)
        {
            GameObject coordbg;
            GameObject coordSelect;
            GameObject outfitCasual;

            var humanCusCoordSelect = HumanCustom.Instance;
            var simCoordSelect = CoordeSelect.Instance;
            if (humanCusCoordSelect != null && simCoordSelect != null)
            {
                var coordbgTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateBG");
                coordbg = coordbgTransform.gameObject;

                var coordSelectTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateType");
                coordSelect = coordSelectTransform.gameObject;

                var coordCasualTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateType/01_Plain");
                outfitCasual = coordCasualTransform.gameObject;
            }
            else if (simCoordSelect != null)
            {
                tglGroup.Clear();
                var coordbgTransform = simCoordSelect.transform.Find("Canvas/MainPanel/Selection/CoordinateBG");
                coordbg = coordbgTransform.gameObject;

                var coordSelectTransform = simCoordSelect.transform.FindChild("Canvas/MainPanel/Selection/CoordinateType");
                coordSelect = coordSelectTransform.gameObject;

                var coordCasualTransform = simCoordSelect.transform.FindChild("Canvas/MainPanel/Selection/CoordinateType/01_Plain");
                outfitCasual = coordCasualTransform.gameObject;
            }
            else if (humanCusCoordSelect != null)
            {
                var coordbgTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateBG");
                coordbg = coordbgTransform.gameObject;

                var coordSelectTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateType");
                coordSelect = coordSelectTransform.gameObject;

                var coordCasualTransform = humanCusCoordSelect.transform.Find("UI/Root/Cvs_Coorde/BG/CoordinateType/01_Plain");
                outfitCasual = coordCasualTransform.gameObject;
            }
            else return;

            //Extend the UI so it can fit more icons
            float offset = -50 * (maxOutfits - 3);
            if (coordbg != null)
            {
                var coordCanvas = coordbg.GetComponent<RectTransform>();
                if (coordCanvas != null)
                {
                    var vec = coordCanvas.offsetMin;
                    vec.x += offset;
                    coordCanvas.offsetMin = vec;
                }
            }
            else return;

            var coordinateGroup = coordSelect.GetComponent<HorizontalLayoutGroup>();//Layout group

            int max = maxOutfits - 3;//Check max outfits, ignoring default ones.
            int number = 4;//Use for naming nothing else.
            for (int i = 0; i < max; i++)//<- Create GameObjects and their components for every new outfit.
            {
                //Create a new GameObject and add Components to it
                var customOutfit = new GameObject("0" + number + "_" + outfitsList[i]); //New GameObject to be use for new Outfits icons.
                if (number >= 10) customOutfit = new GameObject(number + "_" + outfitsList[i]);
                number++;
                customOutfit.layer = 5;
                customOutfit.transform.SetParent(coordSelect.transform);//<- Set parent
                customOutfit.AddComponent<RectTransform>();
                customOutfit.AddComponent<CanvasRenderer>();
                customOutfit.AddComponent<Image>();
                customOutfit.AddComponent<Toggle>();

                //Set GameObjects Components
                var rectOutfit = customOutfit.GetComponent<RectTransform>();
                rectOutfit.anchoredPosition = new Vector2(0, 0);
                rectOutfit.sizeDelta = new Vector2(48, 56);
                rectOutfit.pivot = new Vector2(0, 1);
                rectOutfit.anchoredPosition3D = new Vector3(0, 0, 0);

                coordinateGroup.rectChildren.Add(rectOutfit);//<- Add to the outfit select window
                rectOutfit.localScale = new Vector3(1, 1, 1);//Reset Local Scale

                //Create cb_check GameObject. Use for when you click the icon
                GameObject cbOutfit = new GameObject("cb_check");
                cbOutfit.layer = 5;
                cbOutfit.transform.SetParent(customOutfit.transform);//<- Set parent
                cbOutfit.AddComponent<RectTransform>();
                cbOutfit.AddComponent<CanvasRenderer>();
                cbOutfit.AddComponent<Image>();

                //Init Component
                var rectcb = cbOutfit.GetComponent<RectTransform>();
                rectcb.anchorMin = new Vector2(0, 0);
                rectcb.anchorMax = new Vector2(1, 1);
                rectcb.anchoredPosition = new Vector2(0, 0);
                rectcb.sizeDelta = new Vector2(0, 0);
                rectcb.anchoredPosition3D = new Vector3(0, 0, 0);
                rectcb.offsetMin = new Vector2(0, 0);
                rectcb.offsetMax = new Vector2(0, 0);
                rectcb.localScale = new Vector3(1, 1, 1);//Reset Local scale for cb_check

                //Get Component from Outfit GameObject
                var imageOutfits = customOutfit.GetComponent<Image>();
                var imageOutfitsHover = customOutfit.GetComponent<Toggle>();

                imageOutfitsHover.transition = Selectable.Transition.SpriteSwap;

                //Get Components from cb_check GameObject
                var imageOutfitsSelected = cbOutfit.GetComponent<Image>();
                imageOutfitsSelected.raycastTarget = false;
                imageOutfitsHover.graphic = imageOutfitsSelected;

                var defSprite = outfitCasual.GetComponent<Image>();//Default outfit component
                var OutfitSprite = ResourcesLoader.LoadSprite(i, 0);
                if (OutfitSprite != null)
                {
                    Sprite newSprite = Sprite.Create(OutfitSprite, new Rect(0.0f, 0.0f, OutfitSprite.width, OutfitSprite.height), new Vector2(0f, 1f), 100.0f);
                    imageOutfits.sprite = newSprite;
                }
                else
                {
                    MoreOutfitsPlugin.Log.LogInfo($"Missing Sprite for Custom Outfit Normal: Case Null");
                    imageOutfits.sprite = defSprite.sprite;
                }

                OutfitSprite = ResourcesLoader.LoadSprite(i, 1);
                if (OutfitSprite != null)
                {
                    Sprite newSprite = Sprite.Create(OutfitSprite, new Rect(0.0f, 0.0f, OutfitSprite.width, OutfitSprite.height), new Vector2(0f, 1f), 100.0f);
                    imageOutfitsHover.targetGraphic = imageOutfits;
                    SpriteState state = new SpriteState();
                    state.highlightedSprite = newSprite;
                    imageOutfitsHover.spriteState = state;
                }
                else
                {
                    MoreOutfitsPlugin.Log.LogInfo($"Missing Sprite for Custom Outfit Hover: Case Null");
                    var defSpriteHover = outfitCasual.GetComponent<Toggle>();
                    imageOutfitsHover.targetGraphic = defSprite;
                    imageOutfitsHover.spriteState.highlightedSprite = defSpriteHover.spriteState.highlightedSprite;
                }

                OutfitSprite = ResourcesLoader.LoadSprite(i, 2);
                if (OutfitSprite != null)
                {
                    Sprite newSprite = Sprite.Create(OutfitSprite, new Rect(0.0f, 0.0f, OutfitSprite.width, OutfitSprite.height), new Vector2(0f, 1f), 100.0f);
                    imageOutfitsSelected.sprite = newSprite;

                }
                else
                {
                    MoreOutfitsPlugin.Log.LogInfo($"Missing Sprite for Custom Outfit Selected: Case Null");
                    var defSpriteHover = outfitCasual.GetComponent<Toggle>();
                    imageOutfitsHover.spriteState.highlightedSprite = defSpriteHover.spriteState.highlightedSprite;
                }

                OutfitSprite = ResourcesLoader.LoadSprite(i, 3);
                if (OutfitSprite != null)
                {
                    Sprite newSprite = Sprite.Create(OutfitSprite, new Rect(0.0f, 0.0f, OutfitSprite.width, OutfitSprite.height), new Vector2(0f, 1f), 100.0f);
                    SpriteState state = new SpriteState(imageOutfitsHover.spriteState.Pointer);
                    state.disabledSprite = newSprite;
                    imageOutfitsHover.spriteState = state;
                }
                else
                {
                    MoreOutfitsPlugin.Log.LogInfo($"Missing Sprite for Custom Outfit Normal: Case Null");
                    imageOutfitsHover.spriteState.disabledSprite = defSprite.sprite;
                }

                if (simCoordSelect != null && humanCusCoordSelect == null) tglGroup.Add(imageOutfitsHover);
            }

            if (simCoordSelect != null && humanCusCoordSelect == null && tglGroup.Count > 0)
            {
                int index = 0;
                Il2CppReferenceArray<Toggle> newtglGroup = new Il2CppReferenceArray<Toggle>(maxOutfits);
                for (int i = 0; i < maxOutfits; i++)
                {
                    if (i < 3) newtglGroup[i] = simCoordSelect._tglCoordes[i];
                    else
                    {
                        newtglGroup[i] = tglGroup[index];
                        index++;
                    }
                }
                simCoordSelect._tglCoordes = newtglGroup;
            }
        }

        public static DirectoryInfo[] GetImagesPath()
        {
            string customOutfitPath = Path.Combine(Paths.PluginPath, "SVS_MoreOutfits\\images");
            if (Directory.Exists(customOutfitPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(customOutfitPath);
                var imagePacks = dirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly);

                if (imagePacks.Length == 0) return null;
                return imagePacks;
            }
            return null;
        }

        public static void DisplayOutfits(CoordinateTypeChange coordType)
        {
            if (coordType._human.data.Coordinates.Count > coordType._coordinateTypeNames.Count)
            {
                var outfitsList = MoreOutfitsPlugin.GetCustomOufitList();
                if (coordType._coordinateTypeNames.Count < 3)
                {
                    var names = new Il2CppStringArray(coordType._human.data.Coordinates.Count)
                    {
                        //Create array for JP languaje.
                        [0] = "私服",
                        [1] = "役職服",
                        [2] = "水着"
                    };

                    int number = 0;
                    for (int i = 3; i < coordType._human.data.Coordinates.Count; i++)
                    {
                        names[i] = outfitsList[number];
                        number++;
                    }
                    coordType._coordinateTypeNames = names;
                }
                else
                {
                    var names = new Il2CppStringArray(coordType._human.data.Coordinates.Count);

                    int number = 0;
                    for (int i = 0; i < coordType._human.data.Coordinates.Count; i++)
                    {
                        if (i >= coordType._coordinateTypeNames.Count)
                        {
                            names[i] = outfitsList[number];
                            number++;
                        }
                        else names[i] = coordType._coordinateTypeNames[i];
                    }
                    coordType._coordinateTypeNames = names;
                }
            }
        }
        public static void AddOutfitsToDropdown(CustomDropdown customDropdown)
        {
            if (customDropdown.OptionCount() != (MoreOutfitsPlugin.GetMaxOutfits()))
            {
                if (customDropdown._dropdownTMP != null && HumanCustom.Instance.Human != null)
                {
                    var outfitsList = MoreOutfitsPlugin.GetCustomOufitList();
                    int number = 0;
                    for (int i = 3; i < HumanCustom.Instance.Human.data.Coordinates.Count; i++)
                    {
                        TMP_Dropdown.OptionData newOption = new();
                        newOption.m_Text = outfitsList[number];
                        newOption.text = outfitsList[number];
                        customDropdown._dropdownTMP.options.Add(newOption);
                        customDropdown._dropdownTMP.RefreshShownValue();
                        number++;
                    }
                }
            }
        }

        public static void DisplayCharaCoordinates(CoordeSelect charaCoordSelect)
        {
            if (charaCoordSelect == null) return;
            if (GameChara.Player is null) return;

            int currentCoords = GameChara.Player.charFile.Coordinates.Count;
            if (currentCoords < MoreOutfitsPlugin.GetMaxOutfits())
            {
                for (int i = 0; i < charaCoordSelect._tglCoordes.Count; i++)
                {
                    if (i >= currentCoords)
                    {
                        charaCoordSelect._tglCoordes[i].interactable = false;
                        //charaCoordSelect._tglCoordes[i].image.color = new Color(0.5f, 0.5f, 0.5f, 1);
                    }
                    else if (!charaCoordSelect._tglCoordes[i].interactable)
                    {
                        charaCoordSelect._tglCoordes[i].interactable = true;
                        //charaCoordSelect._tglCoordes[i].image.color = Color.white;
                    }
                }
            }
            else
            {
                for (int i = 0; i < charaCoordSelect._tglCoordes.Count; i++)
                {
                    if (!charaCoordSelect._tglCoordes[i].interactable)
                    {
                        charaCoordSelect._tglCoordes[i].interactable = true;
                        //charaCoordSelect._tglCoordes[i].image.color = Color.white;
                    }
                }
            }

        }
    }
}
