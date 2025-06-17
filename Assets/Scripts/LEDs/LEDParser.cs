using System.Collections.Generic;

public class LEDParser {

  //constants for special operator buttons
  private const byte Sync = 0xE0;
  private const byte Escape = 0xD0;

  public struct LedColor {
    public byte Blue;
    public byte Red;
    public byte Green;

    public LedColor(byte b, byte r, byte g) {
      Blue = b;
      Red = r;
      Green = g;
    }
  }
  //providing this with a byte array returns a list of all the colors found in that data. 
  //it will be up to the manager to use that to actually get the correct slider colors
  public List<LedColor> ParseBoard(byte[] rawData) {
    List<LedColor> ledColors = new List<LedColor>();
    
    for (int i = 0; i < rawData.Length; i++) {
      if (rawData[i] == Sync && i + 1 < rawData.Length) {
        int ptr = i + 1;
        byte boardId = ReadByteWithEscape(rawData, ref ptr);
        if (boardId != 2) continue; 
        List<byte> payload = UnescapePayload(rawData, ref ptr);
        
        ledColors.Clear();
        for (int j = 0; j + 2 < payload.Count; j += 3) {
          byte b = payload[j];
          byte r = payload[j + 1];
          byte g = payload[j + 2];
          ledColors.Add(new LedColor(b, r, g));
        }

        i = ptr - 1; 
      }
    }


    return ledColors;
  }

  private List<byte> UnescapePayload(byte[] data, ref int ptr) {
    List<byte> result = new List<byte>();

    while (ptr < data.Length && data[ptr] != Sync) {
      byte b = data[ptr++];

      if (b == Escape) {
        if (ptr >= data.Length) break;
        result.Add((byte)(data[ptr++] + 1));
      }
      else {
        result.Add(b);
      }
    }

    return result;
  }
  
  private byte ReadByteWithEscape(byte[] data, ref int index)
  {
    if (index >= data.Length) return 0;

    byte value = data[index++];

    if (value == Escape) {
      if (index >= data.Length) return 0;
      return (byte)(data[index++] + 1);
    }

    return value;
  }
}