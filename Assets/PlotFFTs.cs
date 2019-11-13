using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Diagnostics;

public class PlotFFTs : MonoBehaviour
{

    private static CheckInput muscleEmgs;
    public List<List<double>> mySampleSignal = new List<List<double>>();
    //double[][] mySampleSignal = new double[7][];
    double[] myTimeSignal;

    public double _fftVals;
    public float maxFreq = 100;
    [Range(0.01f, 10.0f)]
    public float stepSize = 1;
    private int muscleRange = 2;
    private String[] muscleNames = new string[7] { "A", "B", "C", "D", "E", "F", "G" };
    private int noSegments = 1;
    //public double[] _fftValuesArray = { 1, 3, 5, 7, 9 };
    public double[] _fftValuesArray;
    private int segment_length;
    public string firstMuscle;
    public string secondMuscle;
    private int firstMuscleIndex;
    private int secondMuscleIndex;
    private int target_segment = 0;
    private int[] muscleIndices = new int[2];
    public DrawQ1Plot dqpCombined, signalA,frequencyA,signalB,frequencyB,signalXY;

    public List<Transform> timeFreqTransforms = new List<Transform>();

    public Transform tfFreqm0;
    public Transform tfTimem0;
    public Transform tfFreqm1;
    public Transform tfTimem1;


    private struct DFT
    {
        public string muscle;
        public int segment_index;
        public double[] frequency_of_interest, dftMagnitudes, dftPhases;
        public Complex[] dft; // each value corresponds to a foi frequency value (same index).

        public DFT(string muscleName, double[] foi, int segm_index, Complex[] dftval)
        {
            muscle = muscleName;
            frequency_of_interest = foi;
            segment_index = segm_index;
            dft = dftval;
            dftMagnitudes = dft.Select(x => x.Magnitude).ToArray();
            dftPhases = dft.Select(FourierPhase).ToArray();
        }
    }

    private List<DFT> _dftCards = new List<DFT>();
    private double[] _frequencies;
    public Transform coherencePlotTransform;

    private void Awake()
    {
        muscleEmgs = GetComponent<CheckInput>();

    }

    void Start()
    {
        //selecting frequency at 1 gives an unusually high answer
        _frequencies = RangeIEnumerable(2, 64, 2).ToArray();
        segment_length = muscleEmgs.windowSize / noSegments;
        timeFreqTransforms.Add(tfTimem0);
        timeFreqTransforms.Add(tfFreqm0);
        timeFreqTransforms.Add(tfTimem1);
        timeFreqTransforms.Add(tfFreqm1);



    }

    private void Update()
    {
        if (muscleEmgs.timeList.ToArray().Length == muscleEmgs.windowSize)
        {
            firstMuscleIndex = Array.FindIndex(muscleNames, x => x == firstMuscle);
            secondMuscleIndex = Array.FindIndex(muscleNames, x => x == secondMuscle);

            muscleIndices[0] = firstMuscleIndex;
            muscleIndices[1] = secondMuscleIndex;
            myTimeSignal = normalizeTimeSig(muscleEmgs.timeList.ToArray());
            mySampleSignal = muscleEmgs.muscles;

            _dftCards = Enumerable.Range(0, noSegments).SelectMany(segment_index => ComputeSegmentDFTCards(segment_index, muscleNames, _frequencies, myTimeSignal, mySampleSignal, segment_length, noSegments, muscleRange, muscleIndices)).ToList();

            PlotSignalCard(GetMuscleString(muscleIndices[0]),frequencyA,signalA);
            PlotSignalCard(GetMuscleString(muscleIndices[1]),frequencyB,signalB);
            PlotXYComplex(GetMuscleString(muscleIndices[0]), GetMuscleString(muscleIndices[1]), signalXY);

            var msCoh = CalculateFrequencyPhaseDifferences(firstMuscle, secondMuscle);

            PlotFreqSubtraction(GetMuscleString(muscleIndices[0]), GetMuscleString(muscleIndices[1]), dqpCombined);
        }

    }

    public void SetMuscleA(Muscle m)
    {
        firstMuscle = GetStringMuscle(m);
    }
    
    public void SetMuscleB(Muscle m)
    {
        secondMuscle = GetStringMuscle(m);
    }

    public void SetMuscles(Muscle mA, Muscle mB)
    {
        SetMuscleA(mA);
        SetMuscleB(mB);
    }

    public string GetStringMuscle(Muscle m)
    {
        switch (m)
        {
            case Muscle.BiL:
                return "A";
            case Muscle.BiR:
                return "B";
            case Muscle.Tri:
                return "C";
            case Muscle.ADelt:
                return "D";
            case Muscle.MDelt:
                return "E";
            case Muscle.PDelt:
                return "F";
            case Muscle.UTrap:
                return "G";
            default:
                throw new ArgumentOutOfRangeException(nameof(m), m, null);
        }
    }
    public int GetMuscleIndex(string m)
    {
        if (m=="A"){return 0;}
        if (m=="B"){return 1;}
        if (m=="C"){return 2;}
        if (m=="D"){return 3;}
        if (m=="E"){return 4;}
        if (m=="F"){return 5;}
        if (m=="G"){return 6;}
        else
        {
            return 0;
        }
    }

    public string GetMuscleString(int m)
    {
        if (m==0){return "A";}
        if (m==1){return "B";}
        if (m==2){return "C";}
        if (m==3){return "D";}
        if (m==4){return "E";}
        if (m==5){return "F";}
        if (m==6){return "G";}

        return "A";
    }
    private void PlotSignalCard(string myTargetMuscle, DrawQ1Plot frequencyA, DrawQ1Plot signalA)
    {
        ExtractComplexResult(out double[] frequency_array, out double[] complex_array, _dftCards, target_segment,
            myTargetMuscle);
        var signalX = RangeIEnumerable(0.0078125, 1.0, (1.0 / complex_array.Length)).ToArray();
        frequencyA.ac = ListDoubleToCurve(new List<double[]>() {_frequencies.ToArray(), complex_array});
        signalA.ac = ListDoubleToCurve(new List<double[]>() {myTimeSignal, mySampleSignal[GetMuscleIndex(myTargetMuscle)].ToArray()});
    }
    
    private void PlotFreqSubtraction(string mA,string mB, DrawQ1Plot freqPlot)
    {
        ExtractComplexResult(out double[] frequency_arrayA, out double[] complex_arrayA, _dftCards, target_segment,
            mA);
        ExtractComplexResult(out double[] frequency_arrayB, out double[] complex_arrayB, _dftCards, target_segment,
            mB);
        
        var signalX = RangeIEnumerable(0.0078125, 1.0, (1.0 / complex_arrayA.Length)).ToArray();
        freqPlot.ac = ListDoubleToCurve(new List<double[]>() {_frequencies.ToArray(), ElementwiseMult(complex_arrayA,complex_arrayB)});
    }
    
    
    private void PlotXYComplex(string mA, string mB, DrawQ1Plot xyPlot)
    {
        ExtractComplexResult(out double[] frequency_arrayA, out double[] complex_arrayA, _dftCards, target_segment,
            mA);
        ExtractComplexResult(out double[] frequency_arrayB, out double[] complex_arrayB, _dftCards, target_segment,
            mB);
        var signalX = RangeIEnumerable(0.0078125, 1.0, (1.0 / complex_arrayA.Length)).ToArray();
        xyPlot.ac = ListDoubleToCurve(new List<double[]>() {complex_arrayA, complex_arrayB});
    }

    /// <summary>
/// normalizes to 01 for y and x
/// </summary>
/// <param name="msCoh"></param>
/// <returns></returns>
    private static AnimationCurve ListDoubleToCurve(List<double[]> msCoh)
    {
        var msCohAc = new AnimationCurve();
        for (int i = 0; i < msCoh[0].Length; i++)
        {
            msCohAc.AddKey(new Keyframe((float) msCoh[0][i], (float) msCoh[1][i]));
        }

        return msCohAc;
    }

    //Calling the ShowFFTAmplitudes for given segment for the number of muscles we specify
    private void PlotFftAmpltidues(int targeted_segment, string target_muscle, Transform timeTransform, Transform freqTransform, int muscleIndex)
    {
        ExtractComplexResult(out double[] freqs, out double[] complexes, _dftCards, targeted_segment, target_muscle);

        _fftValuesArray = complexes;
        ShowFFTAmplitudes(timeTransform, freqTransform, freqs, complexes, muscleIndex);
    }
 
    
    
    

    private double[] CalculateCoherence(double[] eulersIdentity, string muscleA, string muscleB)
    {

        double[] muscleAAmplitudes = GetMeanAmplitdues(muscleA);
        double[] muscleBAmplitudes = GetMeanAmplitdues(muscleB);

        double[] coherenceForTwoMuscles = new double[muscleAAmplitudes.Length];

        for (int i=0; i < muscleAAmplitudes.Length; i++)
        {
            coherenceForTwoMuscles[i] = muscleAAmplitudes[i] * muscleBAmplitudes[i] * eulersIdentity[i];
        }
        return coherenceForTwoMuscles;
    }

    private double[] GetMeanAmplitdues(string muscleName)
    {
        double[] meanAmplitudes = new double[_frequencies.Length];

        var sumResults = _dftCards.Where(x => x.muscle == muscleName).Select(x => x.dftMagnitudes).Aggregate((arg1, arg2) => ElementwiseSum(arg1, arg2));
        meanAmplitudes = sumResults.Select(x => x / (float)noSegments).ToArray();

        return meanAmplitudes;
    }

    private static double[] GetEulersIdentityfromPhaseDiff(double[] phasevals)
    {
        return phasevals.Select(x => (Math.Cos(x) + Math.Sin(x))).ToArray();
    }

    /// <summary>
    /// first element is the frequencies of interest, and the second element are the corresponding mean phasedifferences across the n segments recorded.
    /// </summary>
    /// <returns>The coherence.</returns>
    private List<double[]> CalculateFrequencyPhaseDifferences(string muscleA, string muscleB)
    { 

        double[] mscohValues = MeanSquaredCoherence(muscleA, muscleB);
        double[] frequenciesUsed = _dftCards[0].frequency_of_interest;
        return new List<double[]> { frequenciesUsed, mscohValues };
    }

    private double[] MeanSquaredCoherence(string muscleA, string muscleB)
    {
        var phaseDifferencesPerSegment = new List<double[]>();
        var ampAmpPhaseVecPerSegment = new List<double[]>();
        var aMagnitudesAcrossSegments = new List<double[]>();
        var bMagnitudesAcrossSegments = new List<double[]>();
        for (int i = 0; i < noSegments; i++)
        {
            var mA = _dftCards.Where(x => x.muscle == muscleA).Where(x => x.segment_index == i).ToList()[0];
            var mB = _dftCards.Where(x => x.muscle == muscleB).Where(x => x.segment_index == i).ToList()[0];

            // a foi is a list of all the frequencies of interest. each one has a corresponding phase value.
            // for each frequency of interest we get the difference between muscle A and muscle B
            // phaseDifferences has the same number of elements as there are frequencies we're evaluating
            GetCoherencePrimitives(
                out double[] eulerNormalizedPhaseDifferences,
                out double[] aMagnitudes,
                out double[] bMagnitudes,
                out double[] ampAmpPhaseVec, mA, mB);

            ampAmpPhaseVecPerSegment.Add(ampAmpPhaseVec);
            aMagnitudesAcrossSegments.Add(aMagnitudes);
            bMagnitudesAcrossSegments.Add(bMagnitudes);

        }


        //double[] sumResult = phaseDifferencesPerSegment.Aggregate((arg1, arg2) => ElementwiseSum(arg1, arg2));
        //double[] meanResult = sumResult.Select(x => x / (float)noSegments).ToArray();

        var summed_freq_numerators = ampAmpPhaseVecPerSegment.Aggregate((arg1, arg2) => ElementwiseSum(arg1, arg2).ToArray());
        var numerator = summed_freq_numerators.Select(x => Math.Pow(Math.Abs(x), 2)).ToArray();
        //this aggregates down from multiple segments down to one combined result. it will be a double[] as long as the number of frequencies specified
        var poweredASums = PowerElementWiseAndSumSegmentWise(aMagnitudesAcrossSegments);
        var poweredBSums = PowerElementWiseAndSumSegmentWise(bMagnitudesAcrossSegments);

        var denominator = ElementwiseMult(poweredASums, poweredBSums);
        double[] msCoh = ElementwiseDivision(numerator, denominator);
        return msCoh;

    }

    private double[] PowerElementWiseAndSumSegmentWise(List<double[]> aMagnitudesAcrossSegments)
    {
        return aMagnitudesAcrossSegments.Select(x => x.Select(y => Math.Pow(y, 2)).ToArray()).Aggregate((arg1, arg2) => ElementwiseSum(arg1, arg2)).ToArray();
    }

    /// <summary>
    /// Gets the coherence primitives.
    /// </summary>
    /// <param name="mA">M a.</param>
    /// <param name="mB">M b.</param>
    private void GetCoherencePrimitives(
        out double[] eulerNormalizedPhaseDifferences,
        out double[] aMagnitudes,
        out double[] bMagnitudes,
        out double[] ampAmpPhaseVec,
        DFT mA,
        DFT mB)
    {
        var phaseDifferences = SignedPhaseDifferences(mA, mB);
        eulerNormalizedPhaseDifferences = GetEulersIdentityfromPhaseDiff(phaseDifferences);
        aMagnitudes = mA.dftMagnitudes;
        bMagnitudes = mB.dftMagnitudes;
        ampAmpPhaseVec = ElementwiseMult(ElementwiseMult(aMagnitudes, bMagnitudes), eulerNormalizedPhaseDifferences);
    }

    private static double[] ElementwiseAbsSumPow(double[] arrayA, double[] arrayB)
    {
        var output = new double[arrayA.Length];
        for (int i = 0; i < arrayA.Length; i++)
        {
            output[i] = Math.Pow(Math.Abs(arrayA[i] + arrayB[i]),2);
        }
        return output;
    }

    private static double[] ElementwiseSum(double[] arrayA, double[] arrayB)
    {
        var output = new double[arrayA.Length];
        for (int i = 0; i < arrayA.Length; i++)
        {
            output[i] = arrayA[i] + arrayB[i];
        }
        return output;
    }
    
    private static double[] ElementWiseSubtract(double[] first, double[] minusThisOne)
    {
        var output = new double[first.Length];
        for (int i = 0; i < first.Length; i++)
        {
            output[i] = first[i] - minusThisOne[i];
        }
        return output;
    }

    private static double[] ElementwiseMult(double[] arrayA, double[] arrayB)
    {
        var output = new double[arrayA.Length];
        for (int i = 0; i < arrayA.Length; i++)
        {
            output[i] = arrayA[i] * arrayB[i];
        }
        return output;
    }

    private static double[] ElementwiseDivision(double[] numerator, double[] denominator)
    {
        var output = new double[numerator.Length];
        for (int i = 0; i < numerator.Length; i++)
        {
            output[i] = numerator[i] / denominator[i];
        }
        return output;
    }

    private double[] SignedPhaseDifferences(DFT a, DFT b)
    {
        if(a.frequency_of_interest != b.frequency_of_interest)
        {
            throw new Exception("frequencies of interest were not aligned");
        }
        if (a.segment_index != b.segment_index)
        {
            throw new Exception("segment indices didn't line up");
        }

        var freqLen = a.dftPhases.Length;
        double[] phaseDifferences = new double[freqLen];

        for (int i = 0; i < freqLen; i++)
        {
            phaseDifferences[i] = b.dftPhases[i] - a.dftPhases[i];
        }
        return phaseDifferences;
    }


    /// <summary>
    /// Extracts the complex result for all of the frequencies that show up. 
    /// will be an array of frequencies and an array of complex values that can be passed to plotting/coherence functions
    /// </summary>
    /// <param name="frequency_array">Frequency array.</param>
    /// <param name="complex_array">Complex array.</param>
    /// <param name="DFTCards">DFT Cards.</param>
    /// <param name="target_segment">Target segment.</param>
    /// <param name="target_muscle">Target muscle.</param>
    private static void ExtractComplexResult(out double[] frequency_array, out double[] complex_array, List<DFT> DFTCards, int target_segment, string target_muscle)
    {

        var freq_and_dft = DFTCards.Where(x => x.segment_index == target_segment & x.muscle == target_muscle).Select(x => new Tuple<double[], double[]>(x.frequency_of_interest, x.dft.Select(y => y.Magnitude).ToArray()));

        List < Tuple<double[], double[]> > freq_dft_list = freq_and_dft.ToList();
        frequency_array = freq_dft_list[0].Item1; //frequency for particular target_muscle and target_segment
        complex_array = freq_dft_list[0].Item2; //dft amplitude for particular target_muscle and target_segment

    }

    /// <summary>
    /// Computes the DFT Cards for a single segment. includes the FFT for each
    ///  muscle, for each desired input frequency
    /// </summary>
    /// <returns>The segment DFTC ards.</returns>
    /// <param name="segmentIndex">Segment index as an integer</param>
    /// <param name="muscleNames">Muscle names as strings i.e. "A"</param>
    /// <param name="_frequencies">Frequencies in Hz as integers</param>
    /// <param name="_myTimeSignal">My time signal</param>
    /// <param name="_mySampleSignal">My raw sample signal from the muscle</param>
    private static List<DFT> ComputeSegmentDFTCards(int segmentIndex, string[] muscleNames, double[] _frequencies, double[] _myTimeSignal, List<List<double>> _mySampleSignal, int segmentLength, int noSegments, int muscleRange, int[] muscleIndices)
    {
        List<DFT> DFTCards = new List<DFT>();
        List<double> slicedSampleSignal;
        for (int i = 0; i < muscleRange; i++)
        {
            //muscleIndices[i]
            if (segmentIndex == noSegments - 2 || noSegments - 2 < 0)
                slicedSampleSignal = _mySampleSignal[muscleIndices[i]].Skip(segmentIndex * segmentLength).Take(_mySampleSignal[i].Count).ToList();
            else
                slicedSampleSignal = _mySampleSignal[muscleIndices[i]].Skip(segmentIndex * segmentLength).Take((segmentIndex + 1) * segmentLength).ToList();

            Stopwatch timer = new Stopwatch();
            timer.Start();

            Complex[] _fftVals = FourierFrequencies(_frequencies, _myTimeSignal, slicedSampleSignal);

            string pA = timer.ElapsedMilliseconds.ToString("F4");
//            UnityEngine.Debug.Log($"fourier: {pA}");


            DFTCards.Add(new DFT(muscleNames[muscleIndices[i]], _frequencies, segmentIndex, _fftVals));

        }
        return DFTCards;
    }

    private static void DebugLogger(int segmentIndex, string[] muscleNames, double[] _frequencies, double[] _myTimeSignal, List<List<double>> _mySampleSignal, List<DFT> DFTCards, int i)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();

        string pA = timer.ElapsedMilliseconds.ToString("F4");
        timer.Restart();

        string pB = timer.ElapsedMilliseconds.ToString("F4");

        UnityEngine.Debug.Log($"fourier {pA}, makedft: {pB}");
    }

    /// <summary>
    /// Normalizes the time sig.
    /// </summary>  
    /// <returns>The time sig.</returns>
    /// <param name="time">Time.</param>
    private double[] normalizeTimeSig(double[] time)
    {
        double maxTime = time.Max();
        double minTime = time.Min();

        for (int i=0; i<time.Length; i++)
        {
            time[i] = (time[i] - minTime) / (maxTime - minTime);
        }
        return time;
    }


    //https://stackoverflow.com/questions/7552839/generate-sequence-with-step-value
    public static IEnumerable<double> RangeIEnumerable(double min, double max, double step)
    {
        double i;
        for (i = min; i <= max; i += step)
            yield return i;
    }

    public static double[] Range(double min, double max, double step)
    {
        var enumerableList = RangeIEnumerable(min, max, step);
        return enumerableList as double[] ?? enumerableList.ToArray();
    }


    public void ShowFFTAmplitudes(Transform timeTransform, Transform freqTtransform, double[] _frequencies, double[] inputSpectrum, int muscleIndex)
    {

        //goes from zero to one with the step size defined by the length of how many frequencies were computed in DFT
        //big assumption: all frequencies are equidistant from one another and there are no missing frequencies.
        double[] X_inputValues = RangeIEnumerable(0.0078125, 1.0, (1.0/inputSpectrum.Length)).ToArray();
        Drawing.DrawChart(timeTransform, myTimeSignal, mySampleSignal[muscleIndex].ToArray(), new double[] { -100, -100 });
        Drawing.DrawChart(freqTtransform, _frequencies, inputSpectrum, new double[] { -100, -100 });
    }

    public void BluePlotFrequency(Transform targetPlot, double[] _frequencies, double[] _yValues)
    {
        Drawing.DrawChart(targetPlot, _frequencies, _yValues, new double[] { -100, -100 });
    }

   
    public static Complex[] FourierFrequencies(double[] frequencies, double[] myTimeSignal, List<double> mySampleSignal)
    {
        var window = GenHanningWindow(myTimeSignal.Count());
        return frequencies.Select(frequency => FourierComplexResult(myTimeSignal, mySampleSignal.ToArray(), frequency, window)).ToArray();
    }

    // this is the amplitude of the sine wave. when we power2 it and get sqrt of it that's the power.
    private static Complex FourierComplexResult(double[] timeSignal, double[] sampleSignal, double targetFrequencyHz, List<double> myOutputHanningWindow)
    {
        int signalLen = timeSignal.Count();
        List<(double, double)> listOfComplexValues = FrequencySpecificGeneratedWaves(targetFrequencyHz, timeSignal);
        Complex complexResult = DiscreteFourierTransform(myOutputHanningWindow, listOfComplexValues, sampleSignal);
        return complexResult;
    }





    // angle between the imaginary and the real components of a DFT's complex number.
    // the closer the result is to zero, the more the phase looks like a cosine.
    // its the right/left shift of the peaks and values with respect to the window.
    // on average, does that signal look more like a cosine or sine wrt the 0 moment.
    // output should be from - pi to pi
    public static double FourierPhase(Complex dftResult)
    {

        // real part refers to the sinusoidal part of the complex number
        //TODO pick the right one
        var radian_output = Math.Atan(dftResult.Imaginary/ dftResult.Real);
        var radian_output2 = Math.Atan2(dftResult.Imaginary , dftResult.Real);
        //TODO verify
//        UnityEngine.Debug.Log($"out1: {radian_output}, out2: {radian_output2}");
        return radian_output;
    }

    // hann window is normalized. result: complex result (the complex-value fourier transform output for the frequency of interest)
    private static Complex DiscreteFourierTransform(List<double> myOutputHanningWindow, List<(double, double)> listOfComplexValues, double[] sampleSignal)
    {
        int signal_len = sampleSignal.Length;
        double realSum = 0;
        double imaginarySum = 0;
        for (int i = 0; i < signal_len; i++)
        {
            realSum += sampleSignal[i] * myOutputHanningWindow[i] * listOfComplexValues[i].Item1;
            imaginarySum += sampleSignal[i] * myOutputHanningWindow[i] * listOfComplexValues[i].Item2;
        }
        //TODO ask chris if this makes sense
        return new Complex(realSum*2,imaginarySum*2);
    }

    private static List<(double, double)> FrequencySpecificGeneratedWaves(double targetFrequencyHz, double[] inputTimeStamps)
    {
        var numPoints = inputTimeStamps.Count();
        double[] outputWave = new double[numPoints];
        var complexVector = new List<(double, double)>();
        for (int i = 0; i < numPoints; i++)
        {
            var a = Math.Cos(2.0 * Math.PI * inputTimeStamps[i] * targetFrequencyHz);
            var b = Math.Sin(2.0 * Math.PI * inputTimeStamps[i] * targetFrequencyHz);
            (double, double) complexValue = (a, b);
            complexVector.Add(complexValue);
        }

        return (complexVector);
    }

    private static List<double> GenHanningWindow(int signal_length)
    {
        var interval = (double)(signal_length) / (double)(signal_length - 1);
        var littleNs = Range(0.0, (double)(signal_length), interval);
        List<double> myOutputWindow = new List<double>();
        for (int i = 0; i < littleNs.Count(); i++)
        {
            var myRes = HanningHelper(signal_length, littleNs.ToArray()[i]);
            myOutputWindow.Add(myRes);
        }
        var cumSum = myOutputWindow.Sum();

        List<double> myNormalizedOutputWindow = myOutputWindow.Select(t => t / cumSum).ToList();

        return myNormalizedOutputWindow;
    }

    private static double HanningHelper(int numSamples, double littleN)
    {
        double sampleFraction = (double)littleN / ((double)numSamples);
        var formattedVal = 0.5f * (1.0f - Mathf.Cos(2.0f * Mathf.PI * (float)sampleFraction));
        return (float)formattedVal;
    }


}
