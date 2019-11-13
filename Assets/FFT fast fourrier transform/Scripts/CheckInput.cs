using System;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Debug = UnityEngine.Debug;
using TMPro;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;


//Receiving Data from Client
public class NetMqListener
{
    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    public delegate void MessageDelegate(string message);

    private readonly MessageDelegate _messageDelegate;

    private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

    private void ListenerWork(string ip, string port)
    {
        AsyncIO.ForceDotNet.Force();

        using (var subSocket = new SubscriberSocket())
        {
            subSocket.Options.ReceiveHighWatermark = 1;
            subSocket.Connect($"tcp://{ip}:{port}");
            subSocket.Subscribe("");
            Debug.Log($"Proctor has successfully connected to client2 via tcp://{ip}:{port}");
            while (!_listenerCancelled)
            {
                if (!subSocket.TryReceiveFrameString(out string frameString)) continue;
                //Debug.Log(frameString);
                _messageQueue.Enqueue(frameString);

            }
            subSocket.Close();
        }
        NetMQConfig.Cleanup();
    }

    public void Update()
    {
        while (!_messageQueue.IsEmpty)
        {
            string message;
            if (_messageQueue.TryDequeue(out message))
            {
                _messageDelegate(message);
            }
            else
            {
                break;
            }
        }
    }

    public NetMqListener(MessageDelegate messageDelegate, string ip, string port)
    {
        _messageDelegate = messageDelegate;
        _listenerWorker = new Thread(() => ListenerWork(ip, port));
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}

public class CheckInput : MonoBehaviour
{

    private NetMqListener _netMqListener;
    private readonly string ip = "127.0.0.1";
    private readonly string port = "556";
    public GameObject showWhileAwaitingData;
    [System.NonSerialized]
    public List<List<double>> muscles = new List<List<double>>();
    [System.NonSerialized]
    public List<double> timeList = new List<double>();
    [System.NonSerialized]
    public String time;
    private double emgMin = -3.0;
    private double emgMax = 3.0;
    private bool isSuccessful;
    private double item;
    private int noMuscles = 7;

    [System.NonSerialized]
    public int windowSize = 128;
    private int prevWindowSize;



    private void HandleMessage(string msg)
    {

        //Debug.Log("msg: " + msg);
        if (msg == null) return;
        String message = msg.Split('#')[0];
        time = msg.Split('#')[1];

        if (muscles[0].Count != windowSize)
        {
            for (int i = 0; i < noMuscles; i++)
            {
                muscles[i].Add(double.Parse(message.Split('%')[i]));
            }
            timeList.Add(double.Parse(time));
        }
        else
        {
            DestroyWaitingGameObject();
            for (int i = 0; i < noMuscles; i++)
            {
                muscles[i].RemoveAt(0);
                muscles[i].Add(double.Parse(message.Split('%')[i]));
            }
            timeList.RemoveAt(0);
            timeList.Add(double.Parse(time));
        }
    }

    private double NormalizeEmg(double val)
    {

        double normalizedVal = (val - emgMin) / (emgMax - emgMin);
        return normalizedVal;
    }

    private void DestroyWaitingGameObject()
    {
        if (showWhileAwaitingData != null) {
            Destroy(showWhileAwaitingData);
        }
    }

    private void Start()
    {
        _netMqListener = new NetMqListener(HandleMessage, ip, port);
        _netMqListener.Start();
        prevWindowSize = windowSize;

        for (int i = 0; i < noMuscles; i++)
        {
            muscles.Add(new List<double>());

        }

    }

    private void Update()
    {
        _netMqListener.Update();

    }

    private void OnDestroy()
    {
        _netMqListener.Stop();
    }

}
