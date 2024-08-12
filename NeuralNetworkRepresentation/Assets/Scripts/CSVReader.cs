using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;



public class CSVReader : MonoBehaviour
{
    public TextAsset file;
    private Student[] trainingData, testData;
    [Range(2, 100)]
    public int dataParts = 10;
    public int invalidlanes = 2;
    public float costLimit = 0.1f;
    public StudentNeuralNetwork studentNetwork;


    public void Start()
    {
        string text = file.text;
        string[] students = text.Split("\n");
        int studentsCount = students.Length - invalidlanes; //headers and EOS
        int batchSize = studentsCount / dataParts;
        int leftOver = studentsCount % dataParts;
        trainingData = new Student[batchSize * (dataParts - 1) + leftOver];
        testData = new Student[batchSize];
        bool headers = true;
        int id = 0;
        Debug.Log($"Reading {studentsCount} students");
        foreach (var student in students)
        {

            if (headers)
            {
                headers = false;
                continue;
            }
            if (student == null || student.Equals(""))
                continue;
            if (id < batchSize)
                testData[id] = new Student(student);
            else
            {
                //if (id < batchSize * 2 + leftOver)

                {
                    trainingData[id - batchSize] = new Student(student);
                    //Debug.Log(trainingData[id - batchSize].ToString());
                }
                    
            }
                
            id++;

        }

        Debug.Log($"Training data: {trainingData.Length}, Test data: {testData.Length}");
        StartCoroutine(Training());
    }

    public IEnumerator Training()
    {
        double cost = 0;
        int iteration = 0;
        int correct = 0; 
        do
        {
            studentNetwork.Learn(trainingData);
            if(iteration%100 == 0)
            {

                cost = studentNetwork.CalculateCosts(trainingData);
                correct = studentNetwork.CalculateCorrects(trainingData);
                Debug.Log($"Iteration {iteration}, {correct}/{trainingData.Length} correct samples, cost function: {cost} ");
          
            }
            iteration++;
            yield return null;
        } while (costLimit < cost);
 
    }
}






[Serializable]
public class Student
{
    public int id { get; set; }
    public int age { get; set; }
    public int gender { get; set; }
    public int ethnicity { get; set; }
    public int parentsEducation { get; set; }
    public float studyTimeWeekly { get; set; }
    public int absenses { get; set; }
    public int tutoring { get; set; }
    public int parentalSupport { get; set; }
    public int extracurricular { get; set; }
    public int sports { get; set; }
    public int music { get; set; }
    public int volunteer { get; set; }
    //Guess
    public float gpa { get; set; }
    public float gradeclass { get; set; }

    public Student(string csv)
    {
        string[] fields = csv.Split(",");
        var properties = this.GetType().GetProperties();
        for(int i = 0; i < properties.Length; i++)
        {
            if(i != 5 && i < 13)
            {
                properties[i].SetValue(this, Int32.Parse(fields[i]));
            }
            else
            {
                properties[i].SetValue(this,  float.Parse(fields[i], CultureInfo.InvariantCulture.NumberFormat));
            }
          
        }
        gradeclass = gpa >= 3.5 ? 0 : gpa >= 3 ? 1 : gpa >= 2.5 ? 2 : gpa >= 2 ? 3 : 4;
        

    }

    public double[] AsDataInputNotNormalized()
    {
        double[] input = new double[12];
        var properties = this.GetType().GetProperties();
        for(int i = 1; i< properties.Length -2; i++){
            input[i - 1] = double.Parse(properties[i].GetValue(this).ToString());
        }
        return input;
    }

    public double[] AsDataInput()
    {
        /*
        double[] input = new double[12];
        input[0] = (age - 15) / 18 - 15;
        input[1] = gender;
        input[2] = (ethnicity) / 3;
        input[3] = (parentsEducation)/4;
        input[4] = (studyTimeWeekly) / 20;
        input[5] = (absenses) / 30;
        input[6] = tutoring;
        input[6] = tutoring;
        input[7] = (parentalSupport) / 4;
        input[8] = extracurricular;
        input[9] = sports;
        input[10] = music;
        input[11] = volunteer;
        */
        double[] input = new double[5];
        //input[0] = (age - 15) / 18 - 15;
        //input[1] = gender;
        //input[2] = (ethnicity) / 3;
        //input[3] = (parentsEducation) / 4;
        //input[0] = gpa;
        //input[1] = gender;

        input[0] = (absenses) / 30;
        input[1] = tutoring;
        input[2] = (parentalSupport) / 4;
        input[3] = extracurricular;
        input[4] = studyTimeWeekly/20;
        //input[9] = sports;
        //input[10] = music;
        //input[11] = volunteer;
        return input;
    }

    public double[] ExpectedOutput()
    {
        double[] result = new double[5];
        result[(int) gradeclass] = 1;
        //result[0] = gpa;
        return result;
    }
    public override string ToString()
    {
        String text = $"Student {id}:\nInput: [";
        double[] input = AsDataInput();
        for(int i = 0; i < input.Length; i++)
        {
            text += $"{input[i].ToString("N2")}, ";
        }
        text += "]\nOutput: [";
        double[] output = ExpectedOutput();
        for (int i = 0; i < output.Length; i++)
        {
            text += $"{output[i].ToString("N2")}, ";
        }
        text += "]";
        return text;
    }

   
}