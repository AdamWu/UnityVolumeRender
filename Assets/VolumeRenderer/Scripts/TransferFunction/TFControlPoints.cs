﻿using UnityEngine;

namespace VolumeRender
{
    public struct TFColourControlPoint
    {
        public float dataValue;
        public Color colourValue;

        public TFColourControlPoint(float dataValue, Color colourValue)
        {
            this.dataValue = dataValue;
            this.colourValue = colourValue;
        }
    }

    public struct TFAlphaControlPoint
    {
        public float dataValue;
        public float alphaValue;

        public TFAlphaControlPoint(float dataValue, float alphaValue)
        {
            this.dataValue = dataValue;
            this.alphaValue = alphaValue;
        }
    }
}