using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LEDManager : MonoBehaviour {
  private NamedPipeClientStream pipeClient;
  private Thread readThread;
  private bool keepReading = false;
  private List<GameObject> sortedChildren;
  private List<byte> receivedData = new();
  private List<Renderer> cachedRenderers = new();
  private readonly ConcurrentQueue<byte[]> packetQueue = new();

  void Start() {
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

  void ConnectToPipe() {
    try {
      pipeClient = new NamedPipeClientStream(".", "chuni_led", PipeDirection.In);
      pipeClient.Connect(5000);
      Debug.Log("Connected to LED pipe");

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
      var rawBuffer = new byte[1024];
      bool escapeNext = false;

      while (keepReading && pipeClient.IsConnected) {
        int bytesRead = pipeClient.Read(rawBuffer, 0, rawBuffer.Length);
        if (bytesRead > 0) {
          for (int i = 0; i < bytesRead; i++) {
            byte b = rawBuffer[i];

            if (escapeNext) {
              b = (byte)(b + 1);
              escapeNext = false;
            }
            else if (b == 0xD0) {
              escapeNext = true;
              continue;
            }

            receivedData.Add(b);

            TryParsePackets();
          }
        }
        else {
          Thread.Sleep(10);
        }
      }
    }
    catch (Exception e) {
      Debug.LogError("Error reading from pipe: " + e.Message);
    }
  }

  void TryParsePackets() {
    int packetLength = 1 + 1 + (31 * 3);

    while (true) {
      int startIndex = receivedData.IndexOf(0xE0);
      if (startIndex == -1) {
        if (receivedData.Count > 1000) receivedData.Clear();
        return;
      }

      if (receivedData.Count - startIndex < packetLength) {
        return;
      }

      var candidate = receivedData.GetRange(startIndex, packetLength).ToArray();

      byte boardNum = candidate[1];
      if (boardNum == 2) {
        packetQueue.Enqueue(candidate);
        receivedData.RemoveRange(0, startIndex + packetLength);
      }
      else {
        receivedData.RemoveRange(0, startIndex + 1);
      }
    }
  }

  void Update() {
    while (packetQueue.TryDequeue(out byte[] packet)) {
      ProcessSliderPacket(packet);
    }
  }

  void ProcessSliderPacket(byte[] packet) {
    const int ledCount = 31;

    for (int i = 0; i < ledCount; i++) {
      int baseIndex = 2 + i * 3;
      byte b = packet[baseIndex];
      byte r = packet[baseIndex + 1];
      byte g = packet[baseIndex + 2];

      Color col = new Color(r / 255f, g / 255f, b / 255f);
      SetLed(i, col);
    }
  }

  void SetLed(int btn, Color color) {
    cachedRenderers[btn].material.color = color;
  }

  void OnDestroy() {
    keepReading = false;
    readThread?.Join();
    pipeClient?.Dispose();
  }
}