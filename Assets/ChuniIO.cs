using System;
using System.IO.Pipes;
using System.Threading;
using UnityEngine;

public class ChuniIO : MonoBehaviour {

  public static ChuniIO Instance;
  private NamedPipeClientStream pipeClient;
  private Thread sendThread;
  private bool running;
  
  private byte opbtn = 0;
  private byte beams = 0;
  private byte[] sliders = new byte[32];

  void Start() {
    if (Instance == null) Instance = this;
    else Destroy(this);
    
    pipeClient = new NamedPipeClientStream(".", "ChuniIIPipe", PipeDirection.Out);
    pipeClient.Connect(3000);  // wait max 3 seconds
    running = true;
    sendThread = new Thread(SendLoop);
    sendThread.Start();
  }

  void OnDestroy() {
    running = false;
    sendThread?.Join();
    pipeClient?.Dispose();
  }
  

  private void SendLoop()
  {
    byte[] buffer = new byte[34];
    while (running) {
      buffer[0] = opbtn;
      buffer[1] = beams;
      Array.Copy(sliders, 0, buffer, 2, 32);

      try {
        pipeClient.Write(buffer, 0, buffer.Length);
        pipeClient.Flush();
      }
      catch (Exception e) {
        Debug.LogError("Pipe write error: " + e.Message);
        running = false;
        break;
      }

      Thread.Sleep(1);
    }
  }

  public void SendButtonToIO(int btn) {
    sliders[btn] = 128;
  }

  public void ReleaseButtonFromIO(int btn) {
    sliders[btn] = 0;
  }
}