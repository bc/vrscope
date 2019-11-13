using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePlotOnContact : MonoBehaviour
{
    private MusclePicker _musclePicker;
    private SetMuscleLogo _setMuscleLogo;
    

    private void Start()
    {
        _setMuscleLogo = GetComponent<SetMuscleLogo>();
        _musclePicker = GameObject.Find("ScriptManager").GetComponent<MusclePicker>();
    }
    

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SelectorBrush")) return;
        _musclePicker.ApplyChangeToPanel(_setMuscleLogo.myMuscle);
//        Debug.Log($"HIT on {_setMuscleLogo.myMuscle}: {(int)_setMuscleLogo.myMuscle}");
    }
}
