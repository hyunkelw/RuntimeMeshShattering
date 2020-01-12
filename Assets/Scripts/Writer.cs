using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Writer
{

    private string myFilePath;

    public Writer(string myFilePath)
    {
        this.myFilePath = myFilePath;
    }

    // Use this for initialization
    public void Initialize()
    {

        if (File.Exists(myFilePath))
        {
            try
            {
                File.Delete(myFilePath);
                Debug.Log("file deleted");
            }
            catch(System.Exception e)
            {       
                Debug.LogError("Cannot delete the file!\n" + e.Message);
            }
        }
        
    }

    public void WriteToFile(string message)
    {
        try { 
            StreamWriter fileWriter = new StreamWriter(myFilePath, true);

            fileWriter.Write(message);
            fileWriter.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cannot write to the file!\n" + e.Message);
        }
    }
}
