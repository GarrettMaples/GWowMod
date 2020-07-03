namespace GWowMod
{
    // Decompiled with JetBrains decompiler
// Type: Curse.Hashing.MurmurHash2
// Assembly: Curse.Hashing, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F7BCE7E-2DAB-48A9-B3D7-52866694E264
// Assembly location: C:\Users\garre\AppData\Roaming\Twitch\Bin\Curse.Hashing.dll

using System.IO;
using System.Text;

namespace Curse.Hashing
{
  public class MurmurHash2
  {
    public const int Seed = 1;
    public const int BufferSize = 65536;

    public static long ComputeNormalizedFileHash(string path)
    {
      return MurmurHash2.ComputeFileHash(path, true);
    }

    public static long ComputeFileHash(string path, bool normalizeWhitespace = false)
    {
      using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
        return (long) MurmurHash2.ComputeHash((Stream) fileStream, 0L, normalizeWhitespace);
    }

    public static uint ComputeHash(string input, bool normalizeWhitespace = false)
    {
      return MurmurHash2.ComputeHash(Encoding.UTF8.GetBytes(input), normalizeWhitespace);
    }

    public static uint ComputeHash(byte[] input, bool normalizeWhitespace = false)
    {
      return MurmurHash2.ComputeHash((Stream) new MemoryStream(input), 0L, normalizeWhitespace);
    }

    public static uint ComputeHash(Stream input, long precomputedLength = 0, bool normalizeWhitespace = false)
    {
      long num1 = precomputedLength != 0L ? precomputedLength : input.Length;
      byte[] buffer = new byte[65536];
      if (precomputedLength == 0L & normalizeWhitespace)
      {
        long position = input.Position;
        num1 = MurmurHash2.ComputeNormalizedLength(input, buffer);
        input.Seek(position, SeekOrigin.Begin);
      }
      uint num2 = (uint) (1UL ^ (ulong) num1);
      uint num3 = 0;
      int num4 = 0;
label_3:
      int num5 = input.Read(buffer, 0, buffer.Length);
      if (num5 != 0)
      {
        for (int index = 0; index < num5; ++index)
        {
          byte b = buffer[index];
          if (!normalizeWhitespace || !MurmurHash2.IsWhitespaceCharacter(b))
          {
            num3 |= (uint) b << num4;
            num4 += 8;
            if (num4 == 32)
            {
              uint num6 = num3 * 1540483477U;
              uint num7 = (num6 ^ num6 >> 24) * 1540483477U;
              num2 = num2 * 1540483477U ^ num7;
              num3 = 0U;
              num4 = 0;
            }
          }
        }
        goto label_3;
      }
      else
      {
        if (num4 > 0)
          num2 = (num2 ^ num3) * 1540483477U;
        uint num6 = (num2 ^ num2 >> 13) * 1540483477U;
        return num6 ^ num6 >> 15;
      }
    }

    public static uint ComputeNormalizedHash(string input)
    {
      return MurmurHash2.ComputeHash(input, true);
    }

    public static uint ComputeNormalizedHash(byte[] input)
    {
      return MurmurHash2.ComputeHash(input, true);
    }

    public static uint ComputeNormalizedHash(Stream input, long precomputedLength = 0)
    {
      return MurmurHash2.ComputeHash(input, precomputedLength, true);
    }

    public static long ComputeNormalizedLength(Stream input, byte[] buffer = null)
    {
      long num1 = 0;
      if (buffer == null)
        buffer = new byte[65536];
label_2:
      int num2 = input.Read(buffer, 0, buffer.Length);
      if (num2 == 0)
        return num1;
      for (int index = 0; index < num2; ++index)
      {
        if (!MurmurHash2.IsWhitespaceCharacter(buffer[index]))
          ++num1;
      }
      goto label_2;
    }

    private static bool IsWhitespaceCharacter(byte b)
    {
      return b == (byte) 9 || b == (byte) 10 || b == (byte) 13 || b == (byte) 32;
    }
  }
}
}