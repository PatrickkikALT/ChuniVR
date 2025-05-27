using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor.Rendering;
using UnityEngine;

public class ChuniIO : MonoBehaviour {
  const string ShmemName = "Local\\Chu2to3Shmem";
  const int BufSize = 1024;

  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  struct SharedData
  {
    public ushort coin_counter;  // uint16_t
    public byte opbtn;           // uint8_t
    public byte beams;           // uint8_t
    public ushort version;       // uint16_t
  }

  private MemoryMappedFile mmf;
  private MemoryMappedViewAccessor accessor;

  public static ChuniIO Instance;

  private void Start() {
    if (!Instance) Instance = this; else Destroy(this);
  }
  
  public static void SendBtnToIO(int cell) {
    
  }

  public static void ReleaseBtnFromIO(int cell) {
    
  }

  public void OnDestroy() {
    
  }
  
  public bool OpenSharedMemory()
  {
    try
    {
      mmf = MemoryMappedFile.OpenExisting(ShmemName, MemoryMappedFileRights.ReadWrite);
      accessor = mmf.CreateViewAccessor(0, BufSize, MemoryMappedFileAccess.ReadWrite);
      return true;
    }
    catch (Exception e)
    {
      Debug.LogError("Failed to open shared memory: " + e.Message);
      return false;
    }
  }

  public void WriteSharedData(ushort coinCounter, byte opbtn, byte beams, ushort version)
  {
    if (accessor == null)
    {
      Debug.LogError("Shared memory not opened.");
      return;
    }

    SharedData data = new SharedData
    {
      coin_counter = coinCounter,
      opbtn = opbtn,
      beams = beams,
      version = version
    };
    
    int size = Marshal.SizeOf(typeof(SharedData));
    byte[] buffer = new byte[size];

    IntPtr ptr = Marshal.AllocHGlobal(size);
    try
    {
      Marshal.StructureToPtr(data, ptr, false);
      Marshal.Copy(ptr, buffer, 0, size);
      
      accessor.WriteArray(0, buffer, 0, size);
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
    }
  }

  public void Close()
  {
    accessor?.Dispose();
    mmf?.Dispose();
  }
}