using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
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

    //get every active cell
    foreach (Transform t in transform) {
      sortedChildren.Add(t.gameObject);
    }

    //sort them by their respective button ids.
    sortedChildren.Sort((a, b) => {
      var btnA = a.GetComponent<TouchCell>().btn;
      var btnB = b.GetComponent<TouchCell>().btn;
      return btnA.CompareTo(btnB);
    });

    //cache renders to prevent getting the renderer component every frame
    foreach (var go in sortedChildren) {
      var rend = go.GetComponent<Renderer>();
      cachedRenderers.Add(rend);
    }
  }

  //stop pipe to prevent garbage data
  void OnApplicationPause(bool pauseStatus) {
    if (pauseStatus) {
      keepReading = false;
      readThread?.Join();
      pipeClient?.Dispose();
    }
    else {
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

      //clear all leftover data to prepare for reading
      receivedData.Clear();
      packetQueue.Clear();

      keepReading = true;
      //again start a new thread to prevent hogging the main thread
      readThread = new Thread(ReadFromPipe) {
        IsBackground = true
      };
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

            //keep other threads from accessing this data while we are busy with it.
            lock (receivedData) {
              //if an escape byte is found, we need to skip it and use the next one
              //segatools.ini:
              //0xD0 is used as an escape character -- if you receive D0 in the output, ignore
              //it and use the next sent byte plus one instead.
              if (escapeNext) {
                receivedData.Add((byte)(b + 1));
                escapeNext = false;
              }
              else if (b == 0xD0) {
                escapeNext = true;
              }
              else {
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
    //make sure no other threads can access the data while we are busy with it
    lock (receivedData) {
      leds = parser.ParseBoard(receivedData.ToArray());
      //clear data after parsing to prevent duplicate data
      receivedData.Clear();
    }

    if (leds.Count == 0) return;

    //every index is switched between an LED and divider, so we have to only grab the LEDs.
    int noteLedIndex = 0;
    for (int i = 0; i < leds.Count; i++) {
      if (i % 2 == 0 && noteLedIndex < 16) {
        var color = leds[i];

        //index is halved, so we have to * 2 to get the correct id then use +1 for the bottom one
        //(ids go 31 top, 32 bottom etc.)
        int unityTop = noteLedIndex * 2;
        int unityBottom = unityTop + 1;

        SetLed(unityTop, color);
        SetLed(unityBottom, color);

        noteLedIndex++;
      }
    }
  }

  //create new color using LEDColor struct (brg) and switch the cached renderer's color to the correct one
  void SetLed(int btn, LEDParser.LedColor ledColor) {
    var color = new Color(ledColor.Red, ledColor.Green, ledColor.Blue);
    if (btn < 0 || btn >= cachedRenderers.Count) {
      return;
    }

    cachedRenderers[btn].material.color = color;
  }

  //stop threads and disconnect pipe
  void OnDestroy() {
    keepReading = false;
    readThread?.Join();
    pipeClient?.Dispose();
  }
}