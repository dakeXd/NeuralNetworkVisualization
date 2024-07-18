using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DrawFunction : MonoBehaviour
{
    Texture2D texture;
    private Color functionColor = new Color(0.7607843F, 1f, 0.6784314F);
    private Color bgColor = new Color(1f, 0.6862745F, 0.6862745F);
    private Color laneColor = new Color(0.8f, 0.8f, 0.9f);

    private List<float> listPositions;
    
    private const int MarginPx = 256;
    private const float MarginDraw = 2;
    public const int TextureSize = 512;
    private float scale = 6f / TextureSize;
    
    //private const int pointSize = 1;
    public Function function;
    public float learnRate = 0.5f;
    private const float H = 0.001f;

    private float functionScale = 2.9f;
    //Ball
    public float xPos, originalXpos;
    public TextMeshProUGUI textLearnRate;
    private void Start()
    {
        xPos = -2f;
        originalXpos = -2f;
        listPositions = new List<float>();
        function = new Cuadratic();
        texture = new Texture2D(TextureSize, TextureSize);
        Sprite spirte = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 100);
        GetComponent<SpriteRenderer>().sprite = spirte;
        UpdateTexture();
      
    }

    public void SetLearnRate(float newLearn)
    {
        learnRate = newLearn;
        textLearnRate.SetText("Valor de aprendizaje: " + newLearn.ToString("F2"));
    }

    public void Learn()
    {
        float slope = calculateSlope();
        listPositions.Add(xPos);
        xPos -= slope * learnRate;
        UpdateTexture();
    }

    public float calculateSlope()
    { 
        float pos1 = function.F(((xPos + H)) );
        float pos2 = function.F(((xPos)));
        float difH = pos1 - pos2;
        float slope = difH / H;
        Debug.Log("PosDer: " + pos1 + " PosIzq: "+ pos2+ " Dif: " + difH + " slop: " + slope);
        return slope;

    }
    public void Restart()
    {
        xPos = originalXpos;
        listPositions.Clear();
        UpdateTexture();
    }

    public void Random()
    {
        originalXpos = UnityEngine.Random.Range(-functionScale*0.85f, functionScale*0.85f);
        xPos = originalXpos;
        Restart();
    }
    public void UpdateTexture()
    {
        DrawBG();
        DrawF(function, functionColor);
        for (int i = 0; i < listPositions.Count; i++)
        {
            DrawSphere(listPositions[i], function.F(listPositions[i]), 8, Color.gray);
        }
        float yPos = function.F(xPos);
        float slope = calculateSlope();
        Segment s = new Segment(xPos, yPos, slope, 1);
        DrawF(s, Color.red);
        DrawSphere(xPos, yPos, 10, Color.white);
        texture.Apply();
    }

    public void DrawBG()
    {
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                bool margin = x <= MarginPx && (x >= (MarginPx - MarginDraw)) ||
                              (y <= MarginPx && (y >= (MarginPx - MarginDraw)));
                texture.SetPixel(x, y, margin ? laneColor : bgColor);
            }
        }
    }
    public void DrawF(Function f, Color color)
    {
        bool first = true;
        int lastY = 0;
        for (int x = 0; x < texture.width; x++)
        {
            float point = f.F(normalizeValue(x)); //y normalizado
            int yPoint = Mathf.RoundToInt(normalziedValueToPixels(point)); //posicion y en la textura
           
            int i1 =0;
            int i2 = 0;
            bool set = false;
            
            if ( yPoint >= 0 &&  yPoint <= TextureSize)
            {
                texture.SetPixel(x, yPoint, color);
                if (!set)
                {
                    i1 = lastY > yPoint ? yPoint: lastY;
                    i2 = lastY > yPoint ? lastY : yPoint;
                    lastY = yPoint;
                    set = true;
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
                        texture.SetPixel(x, i, color);
                    }
                }
            }
        }
    }
    
    private void DrawSphere(float normX, float normY, int radius, Color color)
    {
        int x = Mathf.RoundToInt(normalziedValueToPixels(normX));
        int y = Mathf.RoundToInt(normalziedValueToPixels(normY));
        
        //Debug.Log(normX + " - " + x);
        //Debug.Log(normY + " - " + y);
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

  
    private float normalizeValue(int v)
    {
        return ((float)v - MarginPx) / (TextureSize - MarginPx) * functionScale;
    }
    
    private float normalizeValue(float v)
    {
        return (v - MarginPx) / (TextureSize - MarginPx) * functionScale;
    }

    private float normalziedValueToPixels(float v)
    {
        return (v * (TextureSize - MarginPx)/functionScale + MarginPx);
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

public class Segment : Function
{
    private float posX, posY, slope, range;

    public Segment(float posX, float posY, float slope, float range)
    {
        this.posX = posX;
        this.posY = posY;
        this.slope = slope;
        this.range = range;
    }
    
    public override float F(float x)
    {
        
        float y = slope*(x- posX) + posY;
        if(Distancia(posX, posY, x, y)> 0.5f)
            return 9999;
        return y;
    }

    public float Distancia(float x1, float y1, float x2, float y2)
    {
        return Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
    }
}