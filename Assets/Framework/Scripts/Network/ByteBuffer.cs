using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
//using LuaInterface;

public class ByteBuffer {
	MemoryStream stream = null;
	BinaryWriter writer = null;
	BinaryReader reader = null;

	public ByteBuffer() {
		stream = new MemoryStream();
		writer = new BinaryWriter(stream);
	}

	public ByteBuffer(byte[] data) {
		if (data != null) {
			stream = new MemoryStream(data);
			reader = new BinaryReader(stream);
		} else {
			stream = new MemoryStream();
			writer = new BinaryWriter(stream);
		}
	}

	public void Close() {
		if (writer != null) writer.Close();
		if (reader != null) reader.Close();

		stream.Close();
		writer = null;
		reader = null;
		stream = null;
	}

	public void WriteByte(byte v) {
		writer.Write(v);
	}

	public void WriteInt(int v) {
		writer.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(v)));
	}

	public void WriteShort(short v) {
		writer.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(v)));
	}

	public void WriteLong(long v) {
		writer.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(v)));
	}

	public void WriteFloat(float v) {
		byte[] temp = BitConverter.GetBytes(v);
		if (BitConverter.IsLittleEndian)  Array.Reverse(temp);
		writer.Write(BitConverter.ToSingle(temp, 0));
	}

	public void WriteDouble(double v) {
		byte[] temp = BitConverter.GetBytes(v);
		if (BitConverter.IsLittleEndian) Array.Reverse(temp);
		writer.Write(BitConverter.ToDouble(temp, 0));
	}

	public void WriteString(string v) {
		byte[] bytes = Encoding.UTF8.GetBytes(v);
		writer.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)bytes.Length)));
		writer.Write(bytes);
	}

	public void WriteBytes(byte[] v) {
		writer.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)v.Length)));
		writer.Write(v);
	}

	/*
	public void WriteBuffer(LuaByteBuffer strBuffer) {
		WriteBytes(strBuffer.buffer);
	}
	*/

	public byte ReadByte() {
		return reader.ReadByte();
	}

	public int ReadInt() {
		return (int)IPAddress.HostToNetworkOrder(reader.ReadInt32());
	}

	public short ReadShort() {
		return (short)IPAddress.HostToNetworkOrder(reader.ReadInt16());
	}

	public ushort ReadUShort() {
		return (ushort)IPAddress.HostToNetworkOrder(reader.ReadInt16());
	}

	public long ReadLong() {
		return (long)IPAddress.HostToNetworkOrder(reader.ReadInt64());
	}

	public float ReadFloat() {
		byte[] temp = BitConverter.GetBytes(reader.ReadSingle());
		if(BitConverter.IsLittleEndian) Array.Reverse(temp);
		return BitConverter.ToSingle(temp, 0);
	}

	public double ReadDouble() {
		byte[] temp = BitConverter.GetBytes(reader.ReadDouble());
		if(BitConverter.IsLittleEndian) Array.Reverse(temp);
		return BitConverter.ToDouble(temp, 0);
	}

	public string ReadString() {
		int len = ReadUShort();
		byte[] buffer = new byte[len];
		buffer = reader.ReadBytes(len);
		return Encoding.UTF8.GetString(buffer);
	}

	public byte[] ReadBytes() {
		int len = ReadUShort();
		return reader.ReadBytes(len);
	}

	/*
	public LuaByteBuffer ReadBuffer() {
		byte[] bytes = ReadBytes();
		return new LuaByteBuffer(bytes);
	}
	*/

	public byte[] ToBytes() {
		writer.Flush();
		return stream.ToArray();
	}

	public void Flush() {
		writer.Flush();
	}
}