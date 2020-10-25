using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Logger
{
    public static void Write(string line)
    {
        Debug.Log(line);
        System.Console.Write(line);
    }

    public static void WriteLine(string line)
    {
        Debug.Log(line);
        System.Console.WriteLine(line);
    }
    public static void Error(string line)
    {

        throw new System.Exception(line);
    }
}
