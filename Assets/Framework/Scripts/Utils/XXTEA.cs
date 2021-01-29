using System;

namespace Framework
{
	/// <summary>
	/// xxtea 的摘要说明。
	/// </summary>
	public class XXTEA
	{
        const string DefaultKey = "%zfp1YTxZ%I&t*Zt";

		private static Byte[] Encrypt(Byte[] Data, Byte[] Key)
		{
			if (Data.Length == 0)
			{
				return Data;
			}
            // 1:private static UInt32[] ToUInt32Array(Byte[] Data, Boolean IncludeLength)  xxtea
            // 2:private static UInt32[] Encrypt(UInt32[] v, UInt32[] k)                    xxtea
            // 3:private static Byte[] ToByteArray(UInt32[] Data, Boolean IncludeLength)    xxtea
			return ToByteArray(Encrypt(ToUInt32Array(Data, true), ToUInt32Array(Key, false)), false);
		}
		private static Byte[] Decrypt(Byte[] Data, Byte[] Key)
		{
			if (Data.Length == 0)
			{
				return Data;
			}
			return ToByteArray(Decrypt(ToUInt32Array(Data, false), ToUInt32Array(Key, false)), true);
		}

		private static UInt32[] Encrypt(UInt32[] v, UInt32[] k)
		{
			Int32 n = v.Length - 1;
			if (n < 1)
			{
				return v;
			}
			if (k.Length < 4)
			{
				UInt32[] Key = new UInt32[4];
				k.CopyTo(Key, 0);
				k = Key;
			}
			UInt32 z = v[n], y = v[0], delta = 0x9E3779B9, sum = 0, e;
			Int32 p, q = 6 + 52 / (n + 1);
			while (q-- > 0)
			{
				sum = unchecked(sum + delta);
				e = sum >> 2 & 3;
				for (p = 0; p < n; p++)
				{
					y = v[p + 1];
					z = unchecked(v[p] += (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z));
				}
				y = v[0];
				z = unchecked(v[n] += (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z));
			}
			return v;
		}
		private static UInt32[] Decrypt(UInt32[] v, UInt32[] k)
		{
			Int32 n = v.Length - 1;
			if (n < 1)
			{
				return v;
			}
			if (k.Length < 4)
			{
				UInt32[] Key = new UInt32[4];
				k.CopyTo(Key, 0);
				k = Key;
			}
			UInt32 z = v[n], y = v[0], delta = 0x9E3779B9, sum, e;
			Int32 p, q = 6 + 52 / (n + 1);
			sum = unchecked((UInt32)(q * delta));
			while (sum != 0)
			{
				e = sum >> 2 & 3;
				for (p = n; p > 0; p--)
				{
					z = v[p - 1];
					y = unchecked(v[p] -= (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z));
				}
				z = v[n];
				y = unchecked(v[0] -= (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z));
				sum = unchecked(sum - delta);
			}
			return v;
		}
		private static UInt32[] ToUInt32Array(Byte[] Data, Boolean IncludeLength)
		{
			Int32 n = (((Data.Length & 3) == 0) ? (Data.Length >> 2) : ((Data.Length >> 2) + 1));
			UInt32[] Result;
			if (IncludeLength)
			{
				Result = new UInt32[n + 1];
				Result[n] = (UInt32)Data.Length;
			}
			else
			{
				Result = new UInt32[n];
			}
			n = Data.Length;
			for (Int32 i = 0; i < n; i++)
			{
				Result[i >> 2] |= (UInt32)Data[i] << ((i & 3) << 3);
			}
			return Result;
		}
		private static Byte[] ToByteArray(UInt32[] Data, Boolean IncludeLength)
		{
			Int32 n;
			if (IncludeLength)
			{
				n = (Int32)Data[Data.Length - 1];
			}
			else
			{
				n = Data.Length << 2;
			}
			Byte[] Result = new Byte[n];
			for (Int32 i = 0; i < n; i++)
			{
				Result[i] = (Byte)(Data[i >> 2] >> ((i & 3) << 3));
			}
			return Result;
		}

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Target"></param>
        /// <returns></returns>
		public static byte[] Encrypt(byte[] Target)
		{
            return Encrypt(Target, string.Empty);
		}
        public static byte[] Encrypt(byte[] Target, string Key)
        {
            if (0 == Key.Length)
                Key = DefaultKey;
			System.Text.Encoding encoder = System.Text.Encoding.UTF8;
			Byte[] data = Encrypt(Target, encoder.GetBytes(Key));
			return data;

        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Target"></param>
        /// <returns></returns>
		public static byte[] Decrypt(byte[] Target)
		{
            return Decrypt(Target, string.Empty);
		}
        public static byte[] Decrypt(byte[] Target, string Key)
        {
            if(0==Key.Length)
                Key= DefaultKey;
			System.Text.Encoding encoder = System.Text.Encoding.UTF8;
            
			return Decrypt(Target, encoder.GetBytes(Key));

        }



	}
}
