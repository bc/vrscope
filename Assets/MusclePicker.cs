using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusclePicker : MonoBehaviour
{
    public int selectedPanelToChange = 0;
    public PlotFFTs pffts;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void ToggleSelectedPanelToChange()
    {
        selectedPanelToChange = selectedPanelToChange==0 ? 1 : 0;
    }

    internal void ApplyChangeToPanel(Muscle m)
    {
        if (selectedPanelToChange==0)
        {
            
        pffts.SetMuscleA(m);
        }
        else
        {
            pffts.SetMuscleB(m);
        }
    }
}
