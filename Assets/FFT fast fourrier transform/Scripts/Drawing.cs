using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Drawing
{
    //transform parameters to draw the chart;
    public static double a, b, x0, y0;
    public static LineRenderer linR;
    public static double[] tempYLim = new double[] { 0, 0 };

    public static void DrawChart(Transform tf, double[] X_inputValues, double[] Y_inputValues, double[] ylim)
    {
        linR = tf.GetComponent<LineRenderer>();


        //transform parameters to draw the heart rate
        a = tf.localScale.x;
        b = tf.localScale.y;
        x0 = tf.position.x;
        y0 = tf.position.y;


        linR.positionCount = X_inputValues.Length;
        double max_X = FastFourierTransform.MaxD(X_inputValues);
        double min_X = FastFourierTransform.MinD(X_inputValues);
        double min_Y, max_Y;
         if(System.Math.Abs(ylim[0] - -100) < 0.01)
        {

       
        max_Y = FastFourierTransform.MaxD(Y_inputValues);
        min_Y = FastFourierTransform.MinD(Y_inputValues);
        }
        else
        {
            min_Y = ylim[0];
            max_Y = ylim[1];
        }
        //drawing factors
        double factorA_X = (x0 + a / 2 - (x0 - a / 2)) / (max_X - min_X + 0.01f);
        double factorB_X = factorA_X * (-min_X) + (x0 - a / 2);
        double factorA_Y = (y0 + b / 2 - (y0 - b / 2)) / (max_Y - min_Y + 0.01f);
        double factorB_Y = factorA_Y * (-min_Y) + (y0 - b / 2);


        //draw using the lineRender
        for (int ii = 0; ii < linR.positionCount; ii++)
        {
            // Debug.Log(ii);
            double xt = X_inputValues[ii] * factorA_X + factorB_X;
            double yt = Y_inputValues[ii] * factorA_Y + factorB_Y;

            linR.SetPosition(ii, new UnityEngine.Vector3((float)xt, (float)yt, tf.position.z));
        }
    }

}