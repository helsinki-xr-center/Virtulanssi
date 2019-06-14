using System.Reflection;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public static class ObjectTagger
{
    public enum IconType
    {
        Label = 0,
        Small = 1,
        Pix16 = 2
    };

    [SerializeField]
    static readonly int[] iconCount = { 8, 16, 16 };

    [SerializeField]
    static readonly GUIContent[] labelIconArray =
    {
        EditorGUIUtility.IconContent("sv_label_0"),
        EditorGUIUtility.IconContent("sv_label_1"),
        EditorGUIUtility.IconContent("sv_label_2"),
        EditorGUIUtility.IconContent("sv_label_3"),
        EditorGUIUtility.IconContent("sv_label_4"),
        EditorGUIUtility.IconContent("sv_label_5"),
        EditorGUIUtility.IconContent("sv_label_6"),
        EditorGUIUtility.IconContent("sv_label_7")
    };
    [SerializeField]
    static readonly GUIContent[] smallIconArray =
    {
        EditorGUIUtility.IconContent("sv_icon_dot0_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot1_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot2_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot3_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot4_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot5_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot6_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot7_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot8_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot9_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot10_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot11_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot12_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot13_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot14_sml"),
        EditorGUIUtility.IconContent("sv_icon_dot15_sml")
    };
    [SerializeField]
    static readonly GUIContent[] pix16IconArray =
    {
        EditorGUIUtility.IconContent("sv_icon_dot0_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot1_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot2_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot3_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot4_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot5_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot6_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot7_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot8_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot9_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot10_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot11_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot12_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot13_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot14_pix16_gizmo"),
        EditorGUIUtility.IconContent("sv_icon_dot15_pix16_gizmo")
    };

    public static void SetIcon(GameObject g, IconType i, int index)
    {
        int div = iconCount[(int) i];
        int ind = index % div;
        switch(i)
        {
            case IconType.Label:
                var iconL = labelIconArray[ind];
                var eguL = typeof(EditorGUIUtility);
                var flagsL = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                var argsL = new object[] { g, iconL.image };
                var setIconL = eguL.GetMethod("SetIconForObject", flagsL, null, new Type[]
                    {
                        typeof(UnityEngine.Object), typeof(Texture2D)
                    }, null);
                setIconL.Invoke(null, argsL);
                break;
            case IconType.Small:
                var iconS = smallIconArray[ind];
                var eguS = typeof(EditorGUIUtility);
                var flagsS = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                var argsS = new object[] { g, iconS.image };
                var setIconS = eguS.GetMethod("SetIconForObject", flagsS, null, new Type[]
                    {
                        typeof(UnityEngine.Object), typeof(Texture2D)
                    }, null);
                setIconS.Invoke(null, argsS);
                break;
            case IconType.Pix16:
                var iconP = pix16IconArray[ind];
                var eguP = typeof(EditorGUIUtility);
                var flagsP = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
                var argsP = new object[] { g, iconP.image };
                var setIconP = eguP.GetMethod("SetIconForObject", flagsP, null, new Type[]
                    {
                        typeof(UnityEngine.Object), typeof(Texture2D)
                    }, null);
                setIconP.Invoke(null, argsP);
                break;
        }
    }

    public static void SetIcon(GameObject g, IconType i, IconColor c, bool round)
    {
        switch (c)
        {
            case IconColor.Gray:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 0);
                }
                else
                {
                    SetIcon(g, i, 8);
                }
                break;
            case IconColor.Blue:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 1);
                }
                else
                {
                    SetIcon(g, i, 9);
                }
                break;
            case IconColor.Jade:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 2);
                }
                else
                {
                    SetIcon(g, i, 10);
                }
                break;
            case IconColor.Green:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 3);
                }
                else
                {
                    SetIcon(g, i, 11);
                }
                break;
            case IconColor.Yellow:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 4);
                }
                else
                {
                    SetIcon(g, i, 12);
                }
                break;
            case IconColor.Orange:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 5);
                }
                else
                {
                    SetIcon(g, i, 13);
                }
                break;
            case IconColor.Red:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 6);
                }
                else
                {
                    SetIcon(g, i, 14);
                }
                break;
            case IconColor.Purple:
                if (round || i == IconType.Label)
                {
                    SetIcon(g, i, 7);
                }
                else
                {
                    SetIcon(g, i, 15);
                }
                break;
        }
    }

    public static void SetAsUnconnectedConnectNode(GameObject g)
    {
        SetIcon(g, IconType.Pix16, 8);
    }

    public static void SetAsUnconnectedNormalNode(GameObject g)
    {
        SetIcon(g, IconType.Pix16, 0);
    }

    public static void SetAsBusConnectNode(GameObject g)
    {
        SetIcon(g, IconType.Pix16, 12);
    }

    public static void SetAsBusNormalNode(GameObject g)
    {
        SetIcon(g, IconType.Small, 4);
    }

    public static void SetLaneIcons(TagColorScheme scheme, int laneIndex, ref List<GameObject> nodeList)
    {
        switch(scheme)
        {
            case TagColorScheme.ByLaneNumber:
                foreach (GameObject g in nodeList)
                {
                    ByLaneNumber(laneIndex, g);
                }
                break;
        }
    }

    public static void SetLaneIcons(TagColorScheme scheme, int laneIndex, ref Nodes[] nodeList)
    {
        switch (scheme)
        {
            case TagColorScheme.ByLaneNumber:
                foreach (Nodes n in nodeList)
                {
                    ByLaneNumber(laneIndex, n.gameObject);
                }
                break;
        }
    }

    public static void ByLaneNumber(int laneIndex, GameObject g)
    {
        IconColor c = IconColor.Gray;
        switch(laneIndex)
        {
            case 0:
                c = IconColor.Blue;
                break;
            case 1:
                c = IconColor.Jade;
                break;
            case 2:
                c = IconColor.Green;
                break;
            case 3:
                c = IconColor.Orange;
                break;
            case 4:
                c = IconColor.Red;
                break;
            case 5:
                c = IconColor.Purple;
                break;
        }

        Nodes n = g.GetComponent<Nodes>();
        n.CheckNullNodes();
        bool busLane = n.BusLane;
        bool connected = true;
        bool connectNode = n.ConnectNode;
        if (n.InNodesLength == 0)
        {
            connected = false;
        }
        if (n.OutNodesLength==0)
        {
            connected = false;
        }

        if (connectNode)
        {
            if (!connected)
            {
                SetAsUnconnectedConnectNode(g);
            }
            else if (busLane)
            {
                SetAsBusConnectNode(g);
            }
            else
            {
                SetIcon(g, IconType.Pix16, c, false);
            }
        }
        else
        {
            if (!connected)
            {
                SetAsUnconnectedNormalNode(g);
            }
            else if (busLane)
            {
                SetAsBusNormalNode(g);
            }
            else
            {
                SetIcon(g, IconType.Small, c, true);
            }
        }

    }

}
