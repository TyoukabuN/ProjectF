using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using System.IO;

namespace TPPM
{
    public class PixelParamMap
    {
        private int m_RowLength;
        public int rowLength
        {
            get
            {
                return m_RowLength;
            }
            set
            {
                if (m_RowLength == value)
                    return;

                ResetTexture(value, m_ColumnLength);
                m_RowLength = value;
            }
        }
        private int m_ColumnLength;
        public int columnLength
        {
            get
            {
                return m_ColumnLength;
            }
            set
            {
                if (m_ColumnLength == value)
                    return;

                ResetTexture(m_RowLength, value);
                m_ColumnLength = value;
            }
        }

        public void ResetTexture(int rowLength, int columnLength)
        {
            var temp = new Texture2D(m_RowLength, m_ColumnLength, m_TextureFormat, m_MipChain, m_linear);

            for (int y = 0; y < columnLength; y++)
            {
                if (y >= m_ColumnLength)
                    break;

                for (int x = 0; x < rowLength; x++)
                {
                    if (x >= m_RowLength)
                        break;

                    temp.SetPixel(x, y, m_ParamTexture.GetPixel(x, y));
                }
            }
        }

        private Texture2D m_ParamTexture;

        private TextureFormat m_TextureFormat = TextureFormat.RGBAFloat;

        private bool m_MipChain = false;

        private bool m_linear = true;

        private Dictionary<int, Color[]> dict;

        public PixelParamMap(int rowLength, int columnLength)
        {
            m_RowLength = rowLength;
            m_ColumnLength = columnLength;
            m_ParamTexture = new Texture2D(m_RowLength, m_ColumnLength, m_TextureFormat, m_MipChain, m_linear);
            m_ParamTexture.hideFlags = HideFlags.DontSave;

            dict = new Dictionary<int, Color[]>();
        }

        public void SetSize(int rowLength, int columnLength)
        {
            if (rowLength == m_RowLength && columnLength == m_ColumnLength)
                return;

            ResetTexture(m_RowLength, columnLength);
            m_RowLength = rowLength;
            m_ColumnLength = columnLength;
        }

        public bool SetParam(int index, params Color[] pixels)
        {
            if (index >= m_ColumnLength)
                return false;

            if (pixels.Length > m_RowLength)
                return false;

            dict[index] = pixels;

            ApplyPixel(index);

            return true;
        }

        private bool ApplyPixel(int index)
        {
            Color[] pixels;
            if (!dict.TryGetValue(index, out pixels))
                return false;


            for (int i = 0; i < pixels.Length; i++)
            {
                if (i >= m_ParamTexture.width)
                    break;

                m_ParamTexture.SetPixel(i, index, pixels[i]);
            }

            m_ParamTexture.Apply();
            return true;
        }

        private UnityEngine.Rendering.CommandBuffer m_CommandBuffer;
        public void Apply(string name = "")
        {
            if (string.IsNullOrEmpty(name))
                name = "_ParamTexture";

            //if(m_CommandBuffer == null)
            //    m_CommandBuffer = new UnityEngine.Rendering.CommandBuffer();

            //m_CommandBuffer.SetGlobalTexture(name, m_ParamTexture);
            Shader.SetGlobalTexture(Shader.PropertyToID(name), m_ParamTexture);
        }

        public Color GetPixel(int x,int y)
        {
            return m_ParamTexture.GetPixel(x, y);
        }

#if UNITY_EDITOR
        public void OutputToPNG()
        {
            if (!m_ParamTexture)
                return;

            string path = @"E:\PycharmProjects\pythonProject\storage";
            var bytes = m_ParamTexture.EncodeToPNG();

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string s = string.Format("{0:yyyyMMdd_HHmmss}", System.DateTime.Now);
            string filePath = path + "/" + "paramsTex_" + s + ".png";

            using (FileStream file = File.Open(filePath, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(file);
                writer.Write(bytes);
                file.Close();
            }

            path = path.Replace('/', '\\');
           // System.Diagnostics.Process.Start("explorer.exe", path);
        }
#endif

    }
}

