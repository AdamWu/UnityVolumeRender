﻿using UnityEngine;
using System.Collections.Generic;

namespace VolumeRender
{

    public class TransferFunction
    {
        public List<TFColourControlPoint> colourControlPoints = new List<TFColourControlPoint>();
        public List<TFAlphaControlPoint> alphaControlPoints = new List<TFAlphaControlPoint>();

        private Texture2D texture = null;
        private Texture2D textureNoAlpha = null;

        private const int TEXTURE_WIDTH = 512;
        private const int TEXTURE_HEIGHT = 1;

        public TransferFunction()
        {
            texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBAFloat, false);
            textureNoAlpha = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBAFloat, false);
        }

        public void AddControlPoint(TFColourControlPoint ctrlPoint)
        {
            colourControlPoints.Add(ctrlPoint);
        }

        public void AddControlPoint(TFAlphaControlPoint ctrlPoint)
        {
            alphaControlPoints.Add(ctrlPoint);
        }

        public Texture2D GetTexture()
        {
            if (texture == null)
                GenerateTexture();

            return texture;
        }
        public Texture2D GetTextureNoAlpha()
        {
            if (textureNoAlpha == null)
                GenerateTexture();

            return textureNoAlpha;
        }

        public void GenerateTexture()
        {
            List<TFColourControlPoint> cols = new List<TFColourControlPoint>(colourControlPoints);
            List<TFAlphaControlPoint> alphas = new List<TFAlphaControlPoint>(alphaControlPoints);

            // Sort lists of control points
            cols.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));
            alphas.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));

            // Add colour points at beginning and end
            if (cols.Count == 0 || cols[cols.Count - 1].dataValue < 1.0f)
                cols.Add(new TFColourControlPoint(1.0f, Color.white));
            if (cols[0].dataValue > 0.0f)
                cols.Insert(0, new TFColourControlPoint(0.0f, new Color(0, 0, 0, 0)));

            // Add alpha points at beginning and end
            if (alphas.Count == 0 || alphas[alphas.Count - 1].dataValue < 1.0f)
                alphas.Add(new TFAlphaControlPoint(1.0f, 0.0f));
            if (alphas[0].dataValue > 0.0f)
                alphas.Insert(0, new TFAlphaControlPoint(0.0f, 0.0f));

            int numColours = cols.Count;
            int numAlphas = alphas.Count;
            int iCurrColour = 0;
            int iCurrAlpha = 0;

            Color[] tfCols = new Color[TEXTURE_WIDTH * TEXTURE_HEIGHT];
            for (int iX = 0; iX < TEXTURE_WIDTH; iX++)
            {
                float t = iX / (float)(TEXTURE_WIDTH - 1);
                while (iCurrColour < numColours - 2 && cols[iCurrColour + 1].dataValue < t)
                    iCurrColour++;
                while (iCurrAlpha < numAlphas - 2 && alphas[iCurrAlpha + 1].dataValue < t)
                    iCurrAlpha++;

                TFColourControlPoint leftCol = cols[iCurrColour];
                TFColourControlPoint rightCol = cols[iCurrColour + 1];
                TFAlphaControlPoint leftAlpha = alphas[iCurrAlpha];
                TFAlphaControlPoint rightAlpha = alphas[iCurrAlpha + 1];

                float tCol = (Mathf.Clamp(t, leftCol.dataValue, rightCol.dataValue) - leftCol.dataValue) / (rightCol.dataValue - leftCol.dataValue);
                float tAlpha = (Mathf.Clamp(t, leftAlpha.dataValue, rightAlpha.dataValue) - leftAlpha.dataValue) / (rightAlpha.dataValue - leftAlpha.dataValue);

                Color pixCol = rightCol.colourValue * tCol + leftCol.colourValue * (1.0f - tCol);
                pixCol.a = rightAlpha.alphaValue * tAlpha + leftAlpha.alphaValue * (1.0f - tAlpha);
                //pixCol.a = 1.0f;

                for (int iY = 0; iY < TEXTURE_HEIGHT; iY++)
                {
                    tfCols[iX + iY * TEXTURE_WIDTH] = pixCol;
                }
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(tfCols);
            texture.Apply();

            for (int iX = 0; iX < TEXTURE_WIDTH; iX++)
            {
                for (int iY = 0; iY < TEXTURE_HEIGHT; iY++)
                {
                    Color color = tfCols[iX + iY * TEXTURE_WIDTH];
                    color.a = 1;
                    tfCols[iX + iY * TEXTURE_WIDTH] = color;
                }
            }
            textureNoAlpha.wrapMode = TextureWrapMode.Clamp;
            textureNoAlpha.SetPixels(tfCols);
            textureNoAlpha.Apply();

            tfCols = null;
        }
    }

}