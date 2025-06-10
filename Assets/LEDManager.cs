using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public class LEDManager : MonoBehaviour {
  private NamedPipeClientStream pipeClient;
  private Thread readThread;
  private bool keepReading = false;
  public List<GameObject> sortedChildren = new();
  private List<byte> receivedData = new();
  public List<Renderer> cachedRenderers = new();
  private readonly ConcurrentQueue<byte[]> packetQueue = new();
  private LEDParser parser;

  void Start() {
    parser = new LEDParser();
    ConnectToPipe();
    foreach (Transform t in transform) {
      sortedChildren.Add(t.gameObject);
    }
    sortedChildren.Sort((a, b) => {
      var btnA = a.GetComponent<TouchCell>().btn;
      var btnB = b.GetComponent<TouchCell>().btn;
      return btnA.CompareTo(btnB);
    });
    
    
    foreach (var go in sortedChildren) {
      var rend = go.GetComponent<Renderer>();
      cachedRenderers.Add(rend);
    }
  }

  void OnApplicationPause(bool pauseStatus) {
    if (pauseStatus) {
        keepReading = false;
        readThread?.Join();
        pipeClient?.Dispose();
    } else {
        ConnectToPipe();
    }
}

void ConnectToPipe() {
    try {
        if (pipeClient != null && pipeClient.IsConnected) {
            pipeClient.Dispose();
        }
        
        pipeClient = new NamedPipeClientStream(".", "chuni_led", PipeDirection.In);
        pipeClient.Connect(5000);
        Debug.Log("Connected to LED pipe");

        receivedData.Clear();
        packetQueue.Clear();
        
        keepReading = true;
        readThread = new Thread(ReadFromPipe);
        readThread.IsBackground = true;
        readThread.Start();
    }
    catch (Exception e) {
        Debug.LogError("Failed to connect to pipe: " + e.Message);
    }
}

  void ReadFromPipe() {
    try {
      var rawBuffer = new byte[2048];
      bool escapeNext = false;

      while (keepReading && pipeClient.IsConnected) {
        int bytesRead = pipeClient.Read(rawBuffer, 0, rawBuffer.Length);
        if (bytesRead > 0) {
          for (int i = 0; i < bytesRead; i++) {
            byte b = rawBuffer[i];

            lock (receivedData) {
              if (escapeNext) {
                receivedData.Add((byte)(b + 1));
                escapeNext = false;
              } else if (b == 0xD0) {
                escapeNext = true;
              } else {
                receivedData.Add(b);
              }
            }
          }
        }
        else {
          Thread.Sleep(1);
        }
      }
    }
    catch (Exception e) {
      Debug.LogError("Error reading from pipe: " + e.Message);
    }
  }

  private void Update() {
    if (receivedData.Count == 0) return;
    List<LEDParser.LedColor> leds;
    lock (receivedData) {
       leds = parser.ParseBoard(receivedData.ToArray());
       receivedData.Clear();
    }
    if (leds.Count == 0) return;
    for (int i = 0; i < leds.Count; i++) {
      var led = leds[i];
      SetLed(i, led);
    }
    SetLed(31, leds[^1]);
  }


  void SetLed(int btn, LEDParser.LedColor ledColor) {
    var color = new Color(ledColor.Red, ledColor.Green, ledColor.Blue);
    if (btn < 0 || btn >= cachedRenderers.Count) {
        return;
    }
    cachedRenderers[btn].material.color = color;
  }

  void OnDestroy() {
    keepReading = false;
    readThread?.Join();
    pipeClient?.Dispose();
  }
}