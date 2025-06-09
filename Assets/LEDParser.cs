using System;
using System.Collections.Generic;
using UnityEngine;

public class LEDParser {

  private const byte SYNC = 0xE0;
  private const byte ESCAPE = 0xD0;

  public struct LedColor
  {
    public byte Blue;
    public byte Red;
    public byte Green;

    public LedColor(byte b, byte r, byte g)
    {
      Blue = b;
      Red = r;
      Green = g;
    }
  }

  public List<LedColor> ParseBoard2(byte[] rawData)
  {
    List<LedColor> ledColors = new List<LedColor>();

    for (int i = 0; i < rawData.Length; i++)
    {
      if (rawData[i] == SYNC && i + 1 < rawData.Length)
      {
        int ptr = i + 1;

        byte boardId = ReadByteWithEscape(rawData, ref ptr);
        if (boardId != 2) continue;

        ledColors.Clear(); // only interested in board 2 packet for now

        for (int led = 0; led < 31; led++)
        {
          if (ptr + 2 >= rawData.Length) break;

          byte b = ReadByteWithEscape(rawData, ref ptr);
          byte r = ReadByteWithEscape(rawData, ref ptr);
          byte g = ReadByteWithEscape(rawData, ref ptr);

          ledColors.Add(new LedColor(b, r, g));
        }

        i = ptr - 1;
      }
    }

    return ledColors;
  }

  private byte ReadByteWithEscape(byte[] data, ref int index)
  {
    if (index >= data.Length) return 0;

    byte value = data[index++];

    if (value == ESCAPE)
    {
      if (index >= data.Length) return 0;
      return (byte)(data[index++] + 1);
    }

    return value;
  }
}