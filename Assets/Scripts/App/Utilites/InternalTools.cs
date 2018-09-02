// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace LoomNetwork.CZB.Helpers
{
    public class InternalTools
    {
        private static readonly string LINE_BREAK = "%n%";

        public static void FixVerticalLayoutGroupFitting(Object value)
        {
            VerticalLayoutGroup group = null;

            if (value is VerticalLayoutGroup)
            {
                group = value as VerticalLayoutGroup;
            } else if (value is GameObject)
            {
                group = (value as GameObject).GetComponent<VerticalLayoutGroup>();
            } else if (value is Transform)
            {
                group = (value as Transform).GetComponent<VerticalLayoutGroup>();
            }

            if (group == null)
            
return;

            group.enabled = false;
            Canvas.ForceUpdateCanvases();
            group.SetLayoutVertical();
            group.CalculateLayoutInputVertical();
            group.enabled = true;
        }

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        // InternalTools.CallPhoneNumber("+############");
        public static void CallPhoneNumber(string phone)
        {
            Application.OpenURL("tel://" + phone);
        }

        public static string ReplaceLineBreaks(string data)
        {
            if (data == null)
            {
                return "";
            }

            return data.Replace(LINE_BREAK, "\n");
        }

        public static void SetLayerRecursively(GameObject parent, int layer, List<string> ignoreNames = null, bool parentIgnored = false)
        {
            if (!parentIgnored)
            {
                parent.layer = layer;
            }

            bool ignored = false;
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                if (ignoreNames != null)
                {
                    ignored = ignoreNames.Contains(parent.transform.GetChild(i).gameObject.name);
                }

                if ((!ignored && !parentIgnored) || (ignoreNames == null))
                {
                    parent.transform.GetChild(i).gameObject.layer = layer;
                }

                if (parent.transform.GetChild(i).childCount > 0)
                {
                    SetLayerRecursively(parent.transform.GetChild(i).gameObject, layer, ignoreNames, ignored);
                }
            }
        }

        public static void ShakeList<T>(ref List<T> list)
        {
            Random rnd = new Random();
            list = list.OrderBy(item => rnd.Next()).ToList();
        }

        public static List<T> ShakeList<T>(List<T> list)
        {
            Random rnd = new Random();
            return list.OrderBy(item => rnd.Next()).ToList();
        }

        public static void GroupHorizontalObjects(Transform root, float offset, float spacing)
        {
            int count = root.childCount;

            float width = (spacing * count) - 1;

            Vector3 pivot = new Vector3(offset, 0, 0);

            for (int i = 0; i < count; i++)
            {
                root.GetChild(i).localPosition = new Vector3(pivot.x - (width / 2f), 0, 0);
                pivot.x += width / count;
            }
        }

        public static void GroupVerticalObjects(Transform root, float spacing, float centerOffset = -7f, float height = 7.2f)
        {
            int count = root.childCount;
            float halfHeightOffset = height + spacing;

            float startPos = centerOffset + (((count - 1) * halfHeightOffset) / 2f);

            for (int i = 0; i < count; i++)
            {
                root.GetChild(i).localPosition = new Vector3(root.GetChild(i).localPosition.x, startPos - (halfHeightOffset * i), root.GetChild(i).localPosition.z);
            }
        }

        public static List<object> GetRandomElementsFromList(List<object> root, int count)
        {
            List<object> list = new List<object>();

            if (root.Count < count)
            {
                list.AddRange(root);
            } else
            {
                object element = null;
                for (int i = 0; i < count; i++)
                {
                    element = ShakeList(root).First(x => !list.Contains(x));

                    if ((element != null) && (element != default(List<object>)))
                    {
                        list.Add(element);
                    }
                }
            }

            return list;
        }

        public static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        public static bool IsTabletScreen()
        {
#if FORCE_TABLET_UI
            return true;
#elif FORCE_PHONE_UI
            return false;
#else
            return DeviceDiagonalSizeInInches() > 6.5f;
#endif
        }
    }
}
