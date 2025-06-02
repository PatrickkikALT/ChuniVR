using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEngine;

public class ChuniIO : MonoBehaviour {

  public static ChuniIO Instance;
  private const string SHM_NAME = "ChuniIOSharedMemory";
  private const int SHM_SIZE = 34;
  
  private MemoryMappedFile mmf;
  private MemoryMappedViewAccessor accessor;
  public byte opbtn = 0;
  public byte beams = 0;
  private byte[] sliders = new byte[32];
  public bool running = true;


  void Awake() {
    if (Instance == null) Instance = this;
    else Destroy(this);
  }
  void Start() {
    try {
      mmf = MemoryMappedFile.CreateOrOpen(SHM_NAME, SHM_SIZE, MemoryMappedFileAccess.ReadWrite);
      accessor = mmf.CreateViewAccessor(0, SHM_SIZE, MemoryMappedFileAccess.Write);
      Debug.Log("ChuniIOWriter: Shared memory opened.");
      Thread newThread = new Thread(SendLoop) {
        IsBackground = true
      };
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
      // print($"Sent array {buffer} to IO");
      Thread.Sleep(1);
    }
  }

  public void SendButtonToIO(int btn) {
    sliders[btn - 1] = 128;
    // Debug.Log($"Set Input for cell {btn} to pressed.");
  }

  public void ReleaseButtonFromIO(int btn) {
    sliders[btn - 1] = 0;
    // Debug.Log($"Released input for cell {btn}");
  }
}