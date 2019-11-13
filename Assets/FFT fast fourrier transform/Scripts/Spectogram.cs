using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Spectogram : MonoBehaviour
{
    private static PlotFFTs fftOutput;

    private List<Vector3> _vertices;
    private List<int> _triangles;
    private Mesh _currentMesh;
    private float myPlotWidth = 2f;

    private AudioSource _audioSource;
    public float dbMulti = 50;      // first value is a "sample columns" and each of column have sample data     // it's used for making nice geometric stuff     public static float[][] _bandVolumes;      // defines how many signal observations will be used for the FFT window     public static int SAMPLE_COUNT = 128;     // defines how many FFT results we will hold onto in the _bandVolumes data structure.      public static int SAMPLE_COLUMNS = 500;     private List<float> _bands; 

    // Start is called before the first frame update
    void Start()
    {
    
        _currentMesh = GetComponent<MeshFilter>().mesh;
        _audioSource = GetComponent<AudioSource>();
        _bands = new List<float>()         {             20,40,60,80,100,120,140,160,180,200,220         };           _bandVolumes = new float[SAMPLE_COLUMNS][];          for (int i = 0; i < SAMPLE_COLUMNS; i++)         {             _bandVolumes[i] = new float[_bands.Count - 1]; // -1 because bands are "from,to" like from 20 to 50
        }
    }

    private void Awake()     {         if ((fftOutput == null) && (GameObject.Find("Time-Frequency Script").GetComponent<PlotFFTs>() != null))
            fftOutput = GameObject.Find("Time-Frequency Script").GetComponent<PlotFFTs>();         else             Debug.LogWarning("Missing PlotFFTs component. Please add one");       }


    // Update is called once per frame
    void Update()
    {
        double[] samples = new double[SAMPLE_COUNT];         samples = fftOutput._fftValuesArray;         //Debug.Log("samples length: " + samples.Length);         if (samples.Length > 0)
            ApplyNewMesh(_currentMesh, samples, myPlotWidth);

    }

    private float[][] calculateSignalHistory(double[] samples)     {
        // copying last values level "up"
        for (int i = SAMPLE_COLUMNS - 1; i > 0; i--)         {             Array.Copy(_bandVolumes[i - 1], _bandVolumes[i], _bands.Count - 1);         }


        float[] bandVolumes = new float[_bands.Count - 1];         for (int i = 1; i < _bands.Count; i++)         {             double db = BandVol(_bands[i - 1], _bands[i], samples) * dbMulti;             bandVolumes[i - 1] = (float)db;
            // Debug.Log(i.ToString() + " " + db);
        }          _bandVolumes[0] = bandVolumes;          return _bandVolumes;     }

    public static float BandVol(float fLow, float fHigh, double[] samples)
    {
        float hzStep = 20000f / SAMPLE_COUNT;

        int samples_count = Mathf.RoundToInt((fHigh - fLow) / hzStep);

        int firtSample = Mathf.RoundToInt(fLow / hzStep);
        int lastSample = Mathf.Min(firtSample + samples_count, SAMPLE_COUNT - 1);


        float sum = 0;
        // average the volumes of frequencies fLow to fHigh
        for (int i = firtSample; i <= lastSample; i++)
        {
            sum += (float) samples[i];
        }
        return sum;
    }

    private void ApplyNewMesh(Mesh myMesh, double[] samples, float plotWidth = 10f)
    {
        // reading current samples

        //_audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        //pushing all my fft values to the right
        var signalHistory = calculateSignalHistory(samples);
        //var signalHistory = AudioManager._bandVolumes;
        ComputeDataMesh(out _triangles, out _vertices, signalHistory, plotWidth);
        RefreshMesh(myMesh, _vertices, _triangles);
    }

    private static void RefreshMesh(Mesh myMesh, List<Vector3> vertices, List<int> triangles)
    {
        myMesh.Clear();
        myMesh.vertices = vertices.ToArray();
        myMesh.triangles = triangles.ToArray();
        myMesh.RecalculateNormals();
    }

    private static void ComputeDataMesh(out List<int> triangles, out List<Vector3> vertices, float[][] signalHistory, float plotWidth)
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        for (int m = 0; m < signalHistory.Length - 1; m++)
        {
            float[] currentVolumes = signalHistory[m];
            float[] previousVolumes = signalHistory[m + 1];

            // the signal history.length is the number of observations over time.
            float zBandValue = m / (signalHistory.Length - 1f);
            float zBandNextValue = (m + 1) / (signalHistory.Length - 1f);

            var numberOfXBins = currentVolumes.Length;
            for (int i = 0; i < numberOfXBins - 1; i++)
            {
                // calculating x position
                float x = ((float)i / (numberOfXBins - 1)) * plotWidth;
                float xNext = ((float)(i + 1) / (numberOfXBins - 1)) * plotWidth;
                float volume = currentVolumes[i];
                float volumeNext = currentVolumes[i + 1];

                // two volumes that was previous
                float volumePrevious = previousVolumes[i];
                float volumeNextPrevious = previousVolumes[i + 1];

                if (m == 0)
                    GenerateFrontFace(x, xNext, volume, volumeNext, vertices, triangles, zBandValue);


                // connection with previous band

                // adding vertices connecting this band with the next one
                vertices.Add(new Vector3(x, volume, zBandValue));
                vertices.Add(new Vector3(xNext, volumeNext, zBandValue));
                vertices.Add(new Vector3(x, volumePrevious, zBandNextValue));
                vertices.Add(new Vector3(xNext, volumeNextPrevious, zBandNextValue));

                int start_point = vertices.Count - 4;
                // adding 2 triangles using this vertex
                triangles.Add(start_point + 0);
                triangles.Add(start_point + 2);
                triangles.Add(start_point + 1);

                triangles.Add(start_point + 2);
                triangles.Add(start_point + 3);
                triangles.Add(start_point + 1);
            }
        }
    }


    private static void GenerateFrontFace(float x, float x_next, float volume, float volume_next, List<Vector3> verts, List<int> triangles, float zBandValue)
    {
        // this algoritm can be better, I don't need adding vertex of "next band"

        // adding verst connecting this band with the next one
        verts.Add(new Vector3(x, 0, zBandValue));
        verts.Add(new Vector3(x, volume, zBandValue));
        verts.Add(new Vector3(x_next, 0, zBandValue));
        verts.Add(new Vector3(x_next, volume_next, zBandValue));

        int start_point = verts.Count - 4;
        // adding 2 triangles using this vertex
        triangles.Add(start_point + 0);
        triangles.Add(start_point + 1);
        triangles.Add(start_point + 2);

        triangles.Add(start_point + 1);
        triangles.Add(start_point + 3);
        triangles.Add(start_point + 2);


    }   
}
