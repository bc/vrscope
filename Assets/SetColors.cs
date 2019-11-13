using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum StyleColor
{
    Blue,
    Green,
    Grey,
    Brown,
    Pink,
    Purple,
    Yellow
}

public enum Muscle
{
    [Description("Biceps (Short Head)")]
    BiL,
    [Description("Biceps (Long Head)")]
    BiR,
    [Description("Triceps Brachii")]
    Tri,
    [Description("Anterior Deltoid")]
    ADelt,
    [Description("Medial Deltoid")]
    MDelt,
    [Description("Posterior Deltoid")]
    PDelt,
    [Description("Upper Trapezius")]
    UTrap
}

public enum BlendedStyleColor
{
    Yellow_Purple,
    Yellow_Grey,
    Yellow_Green,
    Yellow_Brown,
    Pink_Yellow,
    Yellow_Blue,
    Pink_Purple,
    Pink_Grey,
    Pink_Green,
    Pink_Brown,
    Grey_Green,
    Grey_Purple,
    Grey_Brown,
    Green_Purple,
    Blue_Pink,
    Blue_Purple,
    Brown_Green,
    Brown_Purple,
    Blue_Grey,
    Blue_Green,
    Blue_Brown
}
public class SetColors : MonoBehaviour
{
    public Material[] materials;
    public Material[] combinedMaterials;
    public Renderer[] targetA;
    public Renderer[] targetB;
    public Renderer[] targetCombination;
    public PlotFFTs plotFFTs;
    public StyleColor a,b;
    private void LateUpdate()
    {

        if (Time.frameCount % 5 != 0) return;
        a = (StyleColor) plotFFTs.GetMuscleIndex(plotFFTs.firstMuscle);
        b = (StyleColor) plotFFTs.GetMuscleIndex(plotFFTs.secondMuscle);
        ApplyNewColors();
    }

    private void ApplyNewColors()
    {
        ApplyColor(targetA, a);
        ApplyColor(targetB, b);
        ApplyMixColor(targetCombination, a, b);
    }

    internal void SetMuscles(int inputA, int inputB)
    {
        a = (StyleColor) inputA;
        b = (StyleColor) inputB;
    }

    private void ApplyMixColor(Renderer[] rendererList, StyleColor a, StyleColor b)
    {
        foreach (var x in rendererList)
        {
            x.material = MixColors(a, b);
        }
    }
    private void ApplyColor(Renderer[] rendererList, StyleColor a)
    {
        foreach (var x in rendererList)
        {
            x.material = GetStyle(a);
        }
    }

    private static BlendedStyleColor Mix(StyleColor a, StyleColor b)
    {
        var aStr = a.ToString();
        var bStr = b.ToString();
        return (BlendedStyleColor)GetEnumMatchIndex(aStr,bStr);;
    }

    private Material MixColors(StyleColor a, StyleColor b)
    {
        if (a==b)
        {
            return GetStyle(a);
        }
        else
        {
            var mixedVal = (int) Mix(a, b);
            return combinedMaterials[mixedVal];
        }
    }

    public Material Muscle2Material(Muscle m)
    {
        return GetStyle((StyleColor)m);
    }
    private Material GetStyle(StyleColor a)
    {
        return materials[(int)a];
    }

    private static int GetEnumMatchIndex(string aStr, string bStr)
    {
                var blendedStrings = Enum.GetNames(typeof(BlendedStyleColor));
                var splits = blendedStrings.Select(x => x.Split('_')).ToList();
                for (int i = 0; i < splits.Count; i++)
                {
                    var elementStr = splits[i];
                    if (MatchEitherWay(aStr, bStr, elementStr))
                    {
                        return i;
                    }
                }

                return -1;
    }

    private static bool MatchEitherWay(string aStr, string bStr, string[] elementStr)
    {
        bool forward = elementStr[0] == aStr && elementStr[1] == bStr;
        bool reverse = elementStr[0] == bStr && elementStr[1] == aStr;
        return forward | reverse;
    }
}
