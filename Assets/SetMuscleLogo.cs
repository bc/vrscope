using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetMuscleLogo : MonoBehaviour
{
    public TextMeshPro tmp;
    public Renderer logoRenderer;
    public Muscle myMuscle;
    private SetColors _sc;
    private void Start()
    {
        _sc = GameObject.Find("ScriptManager").GetComponent<SetColors>();
        logoRenderer.material = _sc.Muscle2Material(myMuscle);
        tmp.text = myMuscle.ToString();

    }

}
