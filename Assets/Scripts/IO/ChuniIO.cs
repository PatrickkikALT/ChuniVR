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

  //every bit is their own beam, so for the first beam you would have to set bit 0 to 1.
  public byte beams = 0;

  //0 = cell off
  //128 = cell on
  private byte[] sliders = new byte[32];
  public bool running = true;
  private byte SERVICE = 0x02;
  private byte TEST = 0x01;


  void Awake() {
    if (Instance == null) Instance = this;
    else Destroy(this);
  }

  void Start() {
    try {
      mmf = MemoryMappedFile.CreateOrOpen(SHM_NAME, SHM_SIZE, MemoryMappedFileAccess.ReadWrite);
      accessor = mmf.CreateViewAccessor(0, SHM_SIZE, MemoryMappedFileAccess.Write);
      Debug.Log("ChuniIOWriter: Shared memory opened.");
      //thread to prevent the loop from hogging the main thread
      Thread newThread = new Thread(SendLoop) {
        IsBackground = true
      };
      newThread.Start();
    }
    catch (Exception ex) {
      Debug.LogError("ChuniIOWriter: Failed to open shared memory: " + ex.Message);
    }
  }

  //make sure all threads stop correctly to prevent resource leaks
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
      //make sure to reset the operator button after sending it
      opbtn = 0;
      Thread.Sleep(1);
    }
  }

  public void ServiceKey() {
    opbtn = SERVICE;
  }

  public void TestKey() {
    opbtn = TEST;
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