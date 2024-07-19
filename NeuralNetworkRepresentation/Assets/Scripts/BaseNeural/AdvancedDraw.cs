using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class AdvancedDraw : MonoBehaviour
{
    Texture2D texture;
    private Color32 correctColor = new Color(0.7607843F, 1f, 0.6784314F);
    private Color32 incorrectColor = new Color(1f, 0.6862745F, 0.6862745F);
    private Color32 laneColor = new Color(0.8f, 0.8f, 0.9f);
    
    private const int MarginPx = 50;
    private const float MarginDraw = 2;
    public const int TextureSize = 512;
    
    private const float zoomX = (TextureSize - MarginPx) / 40f;
    //500 pixeles para dibujar. Rango de 0 a 40. 500/40 1
    private const float zoomY = (TextureSize - MarginPx) / 3000f;

    private const int pointSize = 5;
    //500 pixeles para dibujar. Rango de 0 a 3000. 500/3000 1
    public DataSetCafe setCafe;
    private DataSetCafe normalizedSet;
    public MultilayerNeuralNetwork neurones;

    public TextMeshProUGUI costText, correctText;
    public bool calculateCosts = false;
    public bool learning = false;
    private float nextUpdate, nextDraw;
    //private float updateTime = 0.0001f;
    private float drawTime = 0.2f;
    private bool complete = false;
    private float completeCost = 0.1f;
    private int iteration;
    private void Start()
    {
        texture = new Texture2D(TextureSize, TextureSize);
        Sprite spirte = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 100);
        GetComponent<SpriteRenderer>().sprite = spirte;
        setCafe = CreateTestSet();
        normalizedSet = GetNormalziedSet(setCafe);
        UpdateTexture();
       // nextUpdate = (Time.time)  + updateTime;
        nextDraw = (Time.time)  + drawTime;
        complete = false;
        iteration = 0;
    }

    private void Update()
    {
        if (!complete && learning)
        {
            //if ((Time.time) > nextUpdate)
            //{ 
                neurones.Learn(normalizedSet);
                UpdateCost();
                iteration++;
  
               // nextUpdate = (Time.time)  + updateTime;
               if (iteration % 10 == 0)
               {
                   Debug.Log("Iteration: " + iteration);
//                   UpdateTexture();
                   UpdateTexture();
               }
                   
            //}
            if ((Time.time) > nextDraw)
            {
               
                nextDraw = (Time.time)  + drawTime;
            }
           
        }
    }


    public void UpdateTexture()
    {
            Color32[] colors = new Color32[texture.width * texture.height]; ;
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    bool margin = x <= MarginPx && (x >= (MarginPx - MarginDraw)) || (y <= MarginPx && (y >= (MarginPx - MarginDraw)));

                    if (margin)
                    {
                        SetColor(colors, x, y, laneColor);
                        //texture.SetPixel(x, y, laneColor);
                    }
                    else
                    {
                        SetColor(colors, x, y,neurones.CalculateOutput(new []{ normalizeValue(x), normalizeValue(y)}) == 0 ? correctColor : incorrectColor);
                        //texture.SetPixel(x, y, neurones.CalculateOutput(new []{ normalizeValue(x), normalizeValue(y)}) == 0 ? correctColor : incorrectColor);
                    }
                }
            }
            DrawCafe(colors);
            texture.SetPixels32(colors);
            if(calculateCosts)
                UpdateCost();
            texture.Apply(false);
        }

    private void SetColor(Color32[] colors, int x, int y, Color32 color)
    {
        int pos = texture.width * y + x;
        colors[pos] = color;
    }

    public void UpdateCost()
    {
        double cost = neurones.CalculateCosts(normalizedSet);
        int correctAnswers = neurones.CalculateCorrects(normalizedSet);
        var completes = (cost < completeCost && correctAnswers == normalizedSet.data.Count);
        if(completes != complete)
            UpdateTexture();
        complete = completes;
        costText.text = "Cost: " + cost.ToString("F6");
        correctText.text = "Correct nodes: " + correctAnswers + "/" + normalizedSet.data.Count;
    }
    public void DrawCafe(Color32[] colors)
    {
        foreach (var cafe in setCafe.data)
        {
            var x = normalizeValue( MarginPx + cafe.temperatura * zoomX);
            var y =normalizeValue( MarginPx + cafe.altitud * zoomY);
            DrawSphere(x, y, pointSize, cafe.valid ? Color.green   : Color.red, colors);
        }
    }

    public DataSetCafe GetNormalziedSet(DataSetCafe origin)
    {
        DataSetCafe normalized = ScriptableObject.CreateInstance<DataSetCafe>();
        normalized.data = new List<CafeData>(origin.data.Count);
        for (int i = 0; i < origin.data.Count; i++)
        {
            CafeData d= new CafeData();
            d.altitud = normalizeValue( MarginPx + origin.data[i].altitud * zoomY);
            d.temperatura = normalizeValue( MarginPx + origin.data[i].temperatura * zoomX);
            d.valid = origin.data[i].valid;
            normalized.data.Add(d);
        }

        return normalized;
    }
    
    public DataSetCafe CreateTestSet()
    {
        DataSetCafe normalized = ScriptableObject.CreateInstance<DataSetCafe>();
        normalized.data = new List<CafeData>(100);
        for (int i = 0; i < 100; i++)
        {
            CafeData d= new CafeData();
            d.altitud = Random.Range(0, 3000);
            d.temperatura = Random.Range(0, 40);
            d.valid = (d.temperatura * d.altitud < 20000);
            normalized.data.Add(d);
        }

        return normalized;
    }
    public double[] NormalizeInput(CafeData cafe)
    {
        var x = normalizeValue( MarginPx + cafe.temperatura * zoomX);
        var y =normalizeValue( MarginPx + cafe.altitud * zoomY);
        return new double[] { x, y };
    }

    private void DrawSphere(float normX, float normY, int radius, Color32 color, Color32[] colors)
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
                        SetColor(colors, i, j, color);
                        //texture.SetPixel(i, j, color);
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

