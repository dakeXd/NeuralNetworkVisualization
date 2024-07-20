using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
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

    public TextMeshProUGUI costText, correctText, iterationTime, updateTime;
    public bool calculateCosts = false;
    public bool learning = false;
    //private float nextUpdate, nextDraw;
    //private float updateTime = 0.0001f;
   // private float drawTime = 0.2f;
    private bool complete = false;
    private float completeCost = 0.1f;
    public int aproxDrawSize = 7;
    private int halfAproxDraw;
    private int iteration;
    public int updateScreenTIme = 10;
    public bool createRandomData = false;
    public int randomDataSize = 100;
    
    private void Start()
    {
        //Para poder hacer areas simetricas al dibujar aproximadamente el efecto de la red neuronal, el area afectada debe ser inpar
        if (aproxDrawSize % 2 == 0)
            aproxDrawSize++;
        updateScreenTIme =   PlayerPrefs.GetInt("ScreenUpdateTime", updateScreenTIme);
        SetScreenTime(updateScreenTIme);
        //El radio de efecto es la mitad
        halfAproxDraw = (aproxDrawSize - 1) / 2;
        //La textura se genera en tiempo de ejecucion para evitar problemas con los assets.
        texture = new Texture2D(TextureSize, TextureSize);
        Sprite spirte = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 100);
        GetComponent<SpriteRenderer>().sprite = spirte;
        //Se crea un set de data random de ser necesario
        if(createRandomData)
            setCafe = CreateTestSet();
        //Para alimentar la red neuronal, el set de datos debe estar normalizado en la misma medida que el resto del sistema
        normalizedSet = GetNormalziedSet(setCafe);
        UpdateTextureFull();
        
        complete = false;
        iteration = 0;
    }

    private void Update()
    {
        //Se realiza una pasada de aprendizaje por el set de datos si el aprendizaje esta activado
        if (!complete && learning)
        {
            neurones.Learn(normalizedSet);
            UpdateCost();
            iteration++;
            //Actualziar la textura es una funcion muy costosa que ralentiza enormemente el sistema, asique se llama cada cierta  cantida de iteraciones
            if (iteration % updateScreenTIme == 0)
            {
               iterationTime.text = "Iteration " + iteration;
               UpdateTextureAprox();
            }
        }
    }

    /**
     * Actualiza la textura aproximadamente.
     * En vez de comprobar la red para cada pixel de la textura, se calcula un pixel cada aproxDrawSize pixeles.
     * Todos los pixeles dentro de un area de radio halfAproxDraw se dibujaran del mismo color.
     * De esta forma, para una textura de 512x512 en vez de calcular el resultado de la red 262144 veces
     * Con un aproxDrawSize de 7 se calcularia solo 5350 veces.
     */
    public void UpdateTextureAprox()
    {
        //Array con todos los nuevos pixeles de la textura
        Color32[] colors = new Color32[texture.width * texture.height]; 
        
        //Se recorren todos los pixeles de la textura
        for (int y = 0; y < texture.height; y+=aproxDrawSize)
        {
            for (int x = 0; x < texture.width; x+=aproxDrawSize)
            {
                //Comprobar que el pixel a calcular no esta fuera de la textura
                if (y < 0 || y >= texture.height || x < 0 || x > texture.width)
                    continue;
                
                //Comprobar si el pixel es un pixel de margen (eje de coordenadas)
                bool margin = x <= MarginPx && (x >= (MarginPx - MarginDraw)) || (y <= MarginPx && (y >= (MarginPx - MarginDraw)));
                
                //El color de este pixel puede ser el del margen, y si no lo es, el color de calcular el resultado de la red neuronal en este punto
                Color color = margin ? laneColor :
                    neurones.CalculateOutput(new[] { normalizeValue(x), normalizeValue(y) }) == 0 ? correctColor :
                    incorrectColor;
                
                //A todos los pixeles dentro de un area de halfAproxDraw se les aplicara este mismo color
                for (int i = y - halfAproxDraw; i <= y + halfAproxDraw; i++)
                {
                    //Comprobar si estamos dentro de la textura
                    if(i < 0 || i >= texture.height)
                        continue;
                    
                    for (int j = x - halfAproxDraw; j <= x + halfAproxDraw; j++)
                    {
                        //Comprobar si estamos dentro de la textura
                        if(j < 0 || j >= texture.width)
                            continue;
                        
                        SetColor(colors, j, i, color);
                    }
                }
            }
        }
            
        //Dibujar como puntos el set de datos
        DrawCafe(colors);

        //Colocar todos los colores en la textura
        texture.SetPixels32(colors);
  
        //Actualizar el coste de la red
        if(calculateCosts)
            UpdateCost();
        
        //Aplicar la nueva textura
        texture.Apply(false);
    }
    
    /**
     * Actualiza la textura con exactitud.
     * Para cada pixel de la textura calcula el resultado de la red y aplica su color en el
     */
    public void UpdateTextureFull()
    {
        Color32[] colors = new Color32[texture.width * texture.height]; ;
        
        //Para cada pixel de la textura
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                //Comprobar si el pixel es un pixel de margen (eje de coordenadas)
                bool margin = x <= MarginPx && (x >= (MarginPx - MarginDraw)) || (y <= MarginPx && (y >= (MarginPx - MarginDraw)));
                
                //Asignar el color del pixel dependiendo si es un pixel de margen o del resutltado de la red
                Color color = margin ? laneColor :
                    neurones.CalculateOutput(new[] { normalizeValue(x), normalizeValue(y) }) == 0 ? correctColor :
                    incorrectColor;
                SetColor(colors, x, y, color);
            }
        }
        
        //Dibujar el set de datos y aplicar la nueva textura
        DrawCafe(colors);
        texture.SetPixels32(colors);
        texture.Apply(false);
    }

    /**
     * Asigna un color con una posicion en un sistema de dos dimensiones a un array de una dimension
     */
    private void SetColor(Color32[] colors, int x, int y, Color32 color)
    {
        int pos = texture.width * y + x;
        colors[pos] = color;
    }

    /**
     * Actualiza el coste de la red y las respuestas correctas y los muestra en la pantalla
     */
    public void UpdateCost()
    {
        double cost = neurones.CalculateCosts(normalizedSet);
        int correctAnswers = neurones.CalculateCorrects(normalizedSet);
        
        //Si se ha pasado a tener todos las respuestas correctas con un coste inferior al delimitado, se considera
        //que la red ha completado su aprendizajo y se actualiza la textura con la maxima resolucion.
        var completes = (cost < completeCost && correctAnswers == normalizedSet.data.Count);
        if (completes && !complete)
        {
            complete = true;   
            Debug.Log("CompletedDrawing");
            UpdateTextureFull();
        }
        else
        {
            complete = completes;
        }

        //Mostrar los resultados en pantalla
        costText.text = "Cost: " + cost.ToString("F6");
        correctText.text = "Correct nodes: " + correctAnswers + "/" + normalizedSet.data.Count;
    }
    
    /**
     * Dibujar del set de datos, se necesita pasar el array de colores donde se almacena la informacion de la textura
     */
    public void DrawCafe(Color32[] colors)
    {
        foreach (var cafe in setCafe.data)
        {
            //normalizar los valores para el sistema.
            var x = normalizeValue( MarginPx + cafe.temperatura * zoomX);
            var y =normalizeValue( MarginPx + cafe.altitud * zoomY);
            //Cada punto sera represntado con un circulo
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
        normalized.data = new List<CafeData>(randomDataSize);
        int variance = Random.Range(10000, 30000);
        for (int i = 0; i < randomDataSize; i++)
        {
            CafeData d= new CafeData();
            d.altitud = Random.Range(0, 3000);
            d.temperatura = Random.Range(0, 40);
            d.valid = (d.temperatura * d.altitud < variance && d.temperatura * d.altitud > 5000 );
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

    public void SetScreenTime(int screenTime)
    {
        updateScreenTIme = screenTime;
        PlayerPrefs.SetInt("ScreenUpdateTime", screenTime);
        if (updateTime != null)
        {
            updateTime.text = "Frecuencia actualización visual: " + screenTime;
            updateTime.transform.parent.GetComponentInChildren<Slider>().SetValueWithoutNotify(screenTime);
        }
          
    }
    
    public void SetScreenTime(float screenTime)
    {
        int sc = Mathf.FloorToInt(screenTime);
        PlayerPrefs.SetInt("ScreenUpdateTime", sc);
        updateScreenTIme = sc;
        if (updateTime != null)
        {
            updateTime.text = "Frecuencia actualización visual: " + sc;
            updateTime.transform.parent.GetComponentInChildren<Slider>().SetValueWithoutNotify(sc);
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

