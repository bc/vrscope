using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Panel
{
    A,
    B
}
public class FlipSelector : MonoBehaviour
{
    public MusclePicker mp;
    public Panel panelIndex;
    public Renderer selectionLineRenderer;

    private void Start()
    {
        selectionLineRenderer.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SelectorBrush")) return;
        mp.selectedPanelToChange = (int)panelIndex;
        selectionLineRenderer.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        selectionLineRenderer.enabled = false;
    }
}
