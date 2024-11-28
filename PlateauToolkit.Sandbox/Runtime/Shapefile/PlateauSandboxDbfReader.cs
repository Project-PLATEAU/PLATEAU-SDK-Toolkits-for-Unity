using System;
using System.IO;
using System.Text;

namespace PlateauToolkit.Sandbox.Runtime
{
    public class DbfRecord
    {
        public bool IsDeleted { get; set; }
        public string[] Fields { get; set; }
    }

    public enum SupportedEncoding
    {
        k_ShiftJis,      // 0
        k_UTF8   // 1
    }

    public class PlateauSandboxDbfReader : IDisposable
    {
        BinaryReader m_Reader;
        DbfRecord m_Record;

        int m_HeaderLength;
        int m_RecordLength;
        int m_RecordCount;
        int m_FieldCount;
        int[] m_FieldLengths;
        string[] m_FieldNames;

        readonly string m_DbfFilePath;
        FileStream m_FileStream;
        readonly SupportedEncoding m_SupportedEncoding;

        public PlateauSandboxDbfReader(string filePath, SupportedEncoding supportedEncoding)
        {
            m_DbfFilePath = filePath;
            m_SupportedEncoding = supportedEncoding;
        }

        public void ReadHeader()
        {
            if (File.Exists(m_DbfFilePath))
            {
                m_FileStream = new FileStream(m_DbfFilePath, FileMode.Open);
                m_Reader = new BinaryReader(m_FileStream);
                m_Reader.BaseStream.Seek(4, SeekOrigin.Begin);
                m_RecordCount = m_Reader.ReadInt32();
                m_Reader.BaseStream.Seek(8, SeekOrigin.Begin); // skip initial bytes
                m_HeaderLength = m_Reader.ReadInt16();
                m_RecordLength = m_Reader.ReadInt16();
                m_FieldCount = (m_HeaderLength - 32) / 32;
                // Read the field names and lengths
                m_FieldNames = new string[m_FieldCount];
                m_FieldLengths = new int[m_FieldCount];
                for (int i = 0; i < m_FieldCount; i++)
                {
                    m_Reader.BaseStream.Seek(32 + i * 32, SeekOrigin.Begin);
                    byte[] nameBytes = m_Reader.ReadBytes(11);
                    m_FieldNames[i] = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0'); // remove trailing null characters
                    m_Reader.BaseStream.Seek(32 + i * 32 + 16, SeekOrigin.Begin);
                    m_FieldLengths[i] = m_Reader.ReadByte();
                }

                m_Reader.BaseStream.Seek(m_HeaderLength, SeekOrigin.Begin);
            }
        }

        public int GetRecordLength()
        {
            return m_RecordCount;
        }

        public string[] GetFieldNames()
        {
            return m_FieldNames;
        }

        public DbfRecord ReadNextRecord()
        {
            if (m_Reader.BaseStream.Position >= m_Reader.BaseStream.Length) // end of file
            {
                return null;
            }

            char firstChar = m_Reader.ReadChar();
            if (firstChar == (char)0x1A) // end-of-file marker in DBF file format
            {
                return null;
            }

            var record = new DbfRecord
            {
                IsDeleted = firstChar == '*',
                Fields = new string[m_FieldCount]
            };

            Encoding stringEncoding = Encoding.UTF8;
            if (m_SupportedEncoding == SupportedEncoding.k_ShiftJis)
            {
                stringEncoding = Encoding.GetEncoding(932);
            }
            for (int i = 0; i < m_FieldCount; i++)
            {
                byte[] buffer = m_Reader.ReadBytes(m_FieldLengths[i]);
                record.Fields[i] = stringEncoding.GetString(buffer);
            }

            return record;
        }

        public void ParseDbf()
        {
            if (m_HeaderLength <= 0)
            {
                return;
            }

            // Print the field names
            foreach (string t in m_FieldNames)
            {
                UnityEngine.Debug.Log(t + "\t");
            }

            int records = 0;
            while ((m_Record = ReadNextRecord()) != null)
            {
                if (!m_Record.IsDeleted)
                {
                    records++;
                    foreach (string field in m_Record.Fields)
                    {
                        UnityEngine.Debug.Log(field);
                    }
                }
            }
            UnityEngine.Debug.Log(records);
        }

        public void Dispose()
        {
            if (m_Reader != null)
            {
                m_FileStream.Close();
                m_FileStream.Dispose();
                m_Reader.Close();
                m_Reader.Dispose();
            }
        }
    }
}