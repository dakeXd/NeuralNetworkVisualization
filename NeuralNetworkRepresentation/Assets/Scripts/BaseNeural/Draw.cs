using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Draw : MonoBehaviour
{
    Texture2D texture;
    private Color correctColor = new Color(0.7607843F, 1f, 0.6784314F);
    private Color incorrectColor = new Color(1f, 0.6862745F, 0.6862745F);
    private Color laneColor = new Color(0.8f, 0.8f, 0.9f);
    private const int MarginPx = 50;
    private const float MarginDraw = 2;
    public const int TextureSize = 512;
    private const float zoomX = (TextureSize - MarginPx) / 40f;
    //500 pixeles para dibujar. Rango de 0 a 40. 500/40 1
    private const float zoomY = (TextureSize - MarginPx) / 3000f;

    private const int pointSize = 5;
    //500 pixeles para dibujar. Rango de 0 a 3000. 500/3000 1
    public DataSetCafe setCafe;
    public NeuralChecker neurones;
    private void Start()
    {
        texture = new Texture2D(TextureSize, TextureSize);
        Sprite spirte = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 100);
        GetComponent<SpriteRenderer>().sprite = spirte;
        UpdateTexture();
        
    }

    public void UpdateTexture()
        {
            
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    bool margin = x <= MarginPx && (x >= (MarginPx - MarginDraw)) || (y <= MarginPx && (y >= (MarginPx - MarginDraw)));

                    if (margin)
                    {
                        texture.SetPixel(x, y, laneColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, neurones.calculateValue(normalizeValue(x), normalizeValue(y)) ? correctColor : incorrectColor);
                    }
                }
            }
            DrawCafe();
            texture.Apply();
        }
    
    public void DrawCafe()
    {
        foreach (var cafe in setCafe.data)
        {
            var x = normalizeValue( MarginPx + cafe.temperatura * zoomX);
            var y =normalizeValue( MarginPx + cafe.altitud * zoomY);
            DrawSphere(x, y, pointSize, cafe.valid ? Color.green   : Color.red);
        }
    }

    private void DrawSphere(float normX, float normY, int radius, Color color)
    {
        int x = Mathf.RoundToInt(normalziedValueToPixels(normX));
        int y = Mathf.RoundToInt(normalziedValueToPixels(normY));
        
        //Debug.Log("DrawingSphere " + x + " , " + y + " , " + pointSize);
        for (int i = x - radius; i <= x + radius; i++)
        {
            for (int j =y - radius; j <= y + radius; j++)
            {
                float distance = Mathf.Sqrt(Mathf.Pow(i - x, 2f ) + Mathf.Pow(j - y, 2f));
               //Debug.Log(i + ", " + j + ": " + distance);
               //float distance = radius;
                if (distance <= radius && i < texture.width && y > 0 && j < texture.height && j > 0)
                {
                    
                    if (distance / radius < 0.9f)
                    {
                        texture.SetPixel(i, j, color);
                    }
                }
            }
        }
 
        /*
        float i, angle, x1, y1;
        //float minAngle = math.acos(1 - 1/r);
        for(i = 0; i < 360; i += 1f)
        {
            angle = i;
            x1 = r * Mathf.Cos(angle * Mathf.PI / 180);
            y1 = r * Mathf.Sin(angle * Mathf.PI / 180);
            texture.SetPixel(Mathf.RoundToInt(x + x1), Mathf.RoundToInt(y + y1), color);
        }*/
/*
        for (float t = 0; t < 2.0 * r; t+=0.1f)
        {
            var x = Mathf.Sin(t) * r + center_x;
            var y = Mathf.Cos(t) * r + center_y;
            texture.SetPixel(Mathf.RoundToInt(x), Mathf.RoundToInt(y), color);
        }*/
    }

    private float normalizeValue(float v)
    {
        return (v - MarginPx) / (TextureSize - MarginPx);
    }

    private float normalziedValueToPixels(float v)
    {
        return v * (TextureSize - MarginPx) + MarginPx;
    }
    
    
}
