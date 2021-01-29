using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System;

public class AES
{
	private static string Key
	{
		get
		{
			return "abcdef1234567890";    ////必须是16位
		}
	}
	//默认密钥向量 
	private static byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
	/// <summary>
	/// AES加密算法
	/// </summary>
	/// <param name="byteText">明文字符串</param>
	/// <returns>将加密后的密文转换为Base64编码，以便显示</returns>
	public static byte[]  AESEncrypt(byte[] data)
	{
		//分组加密算法
		SymmetricAlgorithm des = Rijndael.Create();
		byte[] inputByteArray = data;//得到需要加密的字节数组 
		//设置密钥及密钥向量
		des.Key = Encoding.UTF8.GetBytes(Key);
		des.IV = _key1;
		byte[] cipherBytes = null;
		using (MemoryStream ms = new MemoryStream())
		{
			using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
			{
				cs.Write(inputByteArray, 0, inputByteArray.Length);
				cs.FlushFinalBlock();
				cipherBytes = ms.ToArray();//得到加密后的字节数组
				cs.Close();
			}
			ms.Close();
		}
		return cipherBytes;
	}
	/// <summary>
	/// AES解密
	/// </summary>
	/// <param name="cipherText">密文字符串</param>
	/// <returns>返回解密后的明文字符串</returns>
	public static byte[] AESDecrypt(byte[] data)
	{
		Debug.Log ("AESDecrypt " + data.Length);
		byte[] cipherText = data;
		SymmetricAlgorithm des = Rijndael.Create();
		des.Key = Encoding.UTF8.GetBytes(Key);
		des.IV = _key1;
		byte[] decryptBytes = new byte[cipherText.Length];
		using (MemoryStream ms = new MemoryStream(cipherText))
		{
			using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
			{
				cs.Read(decryptBytes, 0, decryptBytes.Length);
				cs.Close();
			}
			ms.Close();
		}
		return decryptBytes;  ///将字符串后尾的'\0'去掉
	}
}