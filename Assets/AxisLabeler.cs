using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AxisLabeler : MonoBehaviour
{
    public string axisValue;

        public TextMeshPro tmp;
   
    
        void LateUpdate()
        {
            tmp.text = axisValue;
        }
}
