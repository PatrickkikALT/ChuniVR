using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEngine;

public class ChuniIO : MonoBehaviour {

  public static ChuniIO Instance;
  private const string SHM_NAME = "ChuniIISharedMemory";
  private const int SHM_SIZE = 34;

  private MemoryMappedFile mmf;
  private MemoryMappedViewAccessor accessor;
  private byte[] dataBuffer = new byte[SHM_SIZE];
  private byte opbtn = 0;
  private byte beams = 0;
  private byte[] sliders = new byte[32];
  private bool running = true;

  void Start() {
    if (Instance == null) Instance = this;
    else Destroy(this);
    try {
      mmf = MemoryMappedFile.CreateOrOpen("ChuniIOSharedMemory", 34, MemoryMappedFileAccess.ReadWrite);
      accessor = mmf.CreateViewAccessor(0, 34, MemoryMappedFileAccess.Write);
      Debug.Log("ChuniIOWriter: Shared memory opened.");
      Thread newThread = new Thread(SendLoop);
      newThread.IsBackground = true;
      newThread.Start();
    }
    catch (Exception ex) {
      Debug.LogError("ChuniIOWriter: Failed to open shared memory: " + ex.Message);
    }
  }

  void OnDestroy() {
    running = false;
    accessor?.Dispose();
    mmf?.Dispose();
  }
  

  private void SendLoop() {
    byte[] buffer = new byte[34];
    while (running) {
      buffer[0] = opbtn;
      buffer[1] = beams;
      Array.Copy(sliders, 0, buffer, 2, 32);
      accessor.WriteArray(0, buffer, 0, SHM_SIZE);
      print($"Sent array {buffer} to IO");
      Thread.Sleep(0);
    }
  }

  public void SendButtonToIO(int btn) {
    sliders[btn - 1] = 128;
    Debug.Log($"Set Input for cell {btn} to pressed.");
  }

  public void ReleaseButtonFromIO(int btn) {
    sliders[btn - 1] = 0;
    Debug.Log($"Released input for cell {btn}");
  }
}