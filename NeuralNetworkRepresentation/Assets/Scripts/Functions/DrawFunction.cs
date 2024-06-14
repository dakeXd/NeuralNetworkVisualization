using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawFunction : MonoBehaviour
{
    Texture2D texture;
    private Color functionColor = new Color(0.7607843F, 1f, 0.6784314F);
    private Color bgColor = new Color(1f, 0.6862745F, 0.6862745F);
    private Color laneColor = new Color(0.8f, 0.8f, 0.9f);
    
    private const int MarginPx = 256;
    private const float MarginDraw = 2;
    public const int TextureSize = 512;
    private float scale = 6f / TextureSize;
    
    private const int pointSize = 1;
    public Function function;


    private void Start()
    {
        function = new Cuadratic();
        texture = new Texture2D(TextureSize, TextureSize);
        Sprite spirte = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 100);
        GetComponent<SpriteRenderer>().sprite = spirte;
        UpdateTexture();
        
    }
    public void UpdateTexture()
    {
        bool first = true;
        int lastY = 0;
        for (int x = 0; x < texture.width; x++)
        {
            float point = function.F(x * scale- MarginPx * scale );
            point /= scale;
            point += MarginPx;
            int yPoint = Mathf.RoundToInt(point);
           
            int i1 =0;
            int i2 = 0;
            bool set = false;
            for (int y = 0; y < texture.height; y++)
            {
                bool margin = x <= MarginPx && (x >= (MarginPx - MarginDraw)) ||
                              (y <= MarginPx && (y >= (MarginPx - MarginDraw)));

                    
                    if (y > yPoint - pointSize && y < yPoint + pointSize)
                    {
                        texture.SetPixel(x, y, functionColor);
                        if (!set)
                        {
                            i1 = lastY > y ? y : lastY;
                            i2 = lastY > y ? lastY : y;
                            lastY = y;
                            set = true;
                        } 
                        
                    }
                    else
                    {
                        texture.SetPixel(x, y, margin ? laneColor : bgColor);
                    }
            }
            if (set)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    for (int i = i1; i < i2; i++)
                    {
                        texture.SetPixel(x, i, functionColor);
                    }
                }
            }
        }
        texture.Apply();
    }

    public void DrawSlope(float x)
    {
        float h = 0.01f;
        float deltaOut = function.F(x + h) - function.F(x);
        float slope = deltaOut / h;

        Vector2 slopeDir = new Vector2(1, slope).normalized;
        Vector2 point = new Vector2(x, function.F(x));
        
    }
    
}


public abstract class Function
{
    public abstract float F(float x);
}

public class Cuadratic : Function
{
    public override float F(float x)
    {
        return 0.2f * x * x * x * x - 0.2f * x * x * x - x * x + .5f;
    }
}