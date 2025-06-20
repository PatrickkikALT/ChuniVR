using System;
using System.IO.MemoryMappedFiles;
using UnityEngine;

public class EAmusementReader : MonoBehaviour {
  private const string SHM_NAME = "ChuniVRAime";
  private const int SHM_SIZE = 10;
  private byte[] empty = new byte[10];

  private MemoryMappedFile mmf;
  private MemoryMappedViewAccessor accessor;

  private void Start() {
    try {
      mmf = MemoryMappedFile.CreateOrOpen(SHM_NAME, SHM_SIZE, MemoryMappedFileAccess.ReadWrite);
      accessor = mmf.CreateViewAccessor(0, SHM_SIZE, MemoryMappedFileAccess.Write);
      accessor.WriteArray(0, empty, 0, 10);
      accessor.Flush();
      Debug.Log("ChuniAimeWriter: Shared memory opened.");
    }
    catch (Exception ex) {
      Debug.LogError("ChuniAimeWriter: Failed to open shared memory: " + ex.Message);
    }
  }

  /* convert every num in the string to their corresponding bcd format. allowing us to put 2 numbers in one byte.
  this is just 8421, so a 5 would be 0101.
  game expects this format (and 10 bytes) and otherwise doesnt read it at all. */
  public static byte[] StringToBCD(string decimalString, int byteCount) {
    byte[] bcd = new byte[byteCount];
    int strLen = decimalString.Length;

    int bcdIndex = byteCount - 1;
    int strIndex = strLen - 1;

    while (bcdIndex >= 0) {
      byte lowNibble = 0;
      byte highNibble = 0;

      if (strIndex >= 0) {
        lowNibble = (byte)(decimalString[strIndex] - '0');
        strIndex--;
      }

      if (strIndex >= 0) {
        highNibble = (byte)(decimalString[strIndex] - '0');
        strIndex--;
      }

      bcd[bcdIndex] = (byte)((highNibble << 4) | lowNibble);
      bcdIndex--;
    }

    return bcd;
  }


  private void OnTriggerEnter(Collider other) {
    if (other.gameObject.TryGetComponent(out EAmusementCard card)) {
      ReadCard(StringToBCD(card.id, 10));
    }
  }

  private void OnTriggerExit(Collider other) {
    if (other.gameObject.TryGetComponent(out EAmusementCard card)) {
      accessor.WriteArray(0, empty, 0, 10);
      accessor.Flush();
    }
  }

  public void ReadCard(byte[] id) {
    accessor.WriteArray(0, id, 0, 10);
    accessor.Flush();
  }
}