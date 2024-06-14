using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class AdvancedDraw : MonoBehaviour
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
    public MultilayerNeuralNetwork neurones;

    public TextMeshProUGUI costText, correctText;
    public bool calculateCosts = false;
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
                        texture.SetPixel(x, y, neurones.CalculateOutput(new []{ normalizeValue(x), normalizeValue(y)}) == 0 ? correctColor : incorrectColor);
                    }
                }
            }
            DrawCafe();
            if(calculateCosts)
                UpdateCost();
            texture.Apply();
        }

    public void UpdateCost()
    {
        double cost = neurones.CalculateCosts(setCafe);
        int correctAnswers = neurones.CalculateCorrects(setCafe);
        costText.text = "Cost: " + cost.ToString("F6");
        correctText.text = "Correct nodes: " + correctAnswers + "/" + setCafe.data.Count;
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

    public double[] NormalizeInput(CafeData cafe)
    {
        var x = normalizeValue( MarginPx + cafe.temperatura * zoomX);
        var y =normalizeValue( MarginPx + cafe.altitud * zoomY);
        return new double[] { x, y };
    }

    private void DrawSphere(float normX, float normY, int radius, Color color)
    {
        int x = Mathf.RoundToInt(normalziedValueToPixels(normX));
        int y = Mathf.RoundToInt(normalziedValueToPixels(normY));
        

        for (int i = x - radius; i <= x + radius; i++)
        {
            for (int j =y - radius; j <= y + radius; j++)
            {
                float distance = Mathf.Sqrt(Mathf.Pow(i - x, 2f ) + Mathf.Pow(j - y, 2f));
                if (distance <= radius && i < texture.width && y > 0 && j < texture.height && j > 0)
                {
                    
                    if (distance / radius < 0.9f)
                    {
                        texture.SetPixel(i, j, color);
                    }
                }
            }
        }
 
    }

    private double normalizeValue(int v)
    {
        return ((double)v - MarginPx) / (TextureSize - MarginPx);
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

