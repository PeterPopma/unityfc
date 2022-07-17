using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowd : MonoBehaviour
{
    [SerializeField] private GameObject pfSpectator1;
    [SerializeField] private GameObject pfSpectator2;
    [SerializeField] private GameObject pfSpectator3;
    [SerializeField] private GameObject pfSpectator4;
    [SerializeField] private GameObject pfSpectator5;
    [SerializeField] private GameObject pfSpectator6;
    [SerializeField] private GameObject pfSpectator7;
    [SerializeField] private GameObject pfSpectator8;
    [SerializeField] private float stadiumFillRatio = 0.7f;
    readonly System.Random rnd = new();

    private GameObject SelectRandomSpectator()
    {
        int value = rnd.Next(8);
        switch(value)
        {
            case 0:
                return pfSpectator1;
            case 1:
                return pfSpectator2;
            case 2:
                return pfSpectator3;
            case 3:
                return pfSpectator4;
            case 4:
                return pfSpectator5;
            case 5:
                return pfSpectator6;
            case 6:
                return pfSpectator7;
            case 7:
                return pfSpectator8;

            default: return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject crowdRoot = GameObject.Find("Crowd");
        var crowdList = new List<GameObject>();
        for (int stand = 0; stand <4; stand++)
        {
         /* 
            x start: -36.6809998
            x end: -20.9599991
            x step: 1.0629998
            y start: 2.75
            y end: 11.5670004
            y step: 0.75600004
            z start: -29.8910007
            z end: -40.5419998
            z step: -0.9889985
        */        
            float z = -29.8910007f;
            for (float y = 2.756f; y <= 11.5670004; y += 0.80f)
            {
                for (float x = -36.6f; x <= -20.9599991; x += 1.09f)
                {
                    if (Random.value < stadiumFillRatio)
                    {
                        float random_x_offset = (float)((Random.value - 0.5) * 0.6f);
                        GameObject newPerson = Instantiate(SelectRandomSpectator(), new Vector3((float)(random_x_offset + x + 19.23 * stand), y, z), Quaternion.identity);
                        newPerson.transform.Rotate(new Vector3(277.846069f, 180f, 23.5500908f));
                        newPerson.isStatic = true;
                        newPerson.transform.parent = crowdRoot.transform;
                        crowdList.Add(newPerson);
                    }

                    // Mirror the z-axis for the other side
                    if (Random.value < stadiumFillRatio)
                    {
                        float random_x_offset = (float)((Random.value - 0.5) * 0.6f);
                        GameObject newPerson = Instantiate(SelectRandomSpectator(), new Vector3((float)(random_x_offset + 0.7f + x + 19.23 * stand), y, -z + 0.5f), Quaternion.identity);
                        newPerson.transform.Rotate(new Vector3(277.846069f, 0f, 23.5500908f));
                        newPerson.isStatic = true;
                        newPerson.transform.parent = crowdRoot.transform;
                        crowdList.Add(newPerson);
                    }
                }
                z -= 0.96f;
            }
        }

        for (int stand = 0; stand < 2; stand++)
        {
            /* 
               z start: -3.1
               z end: -23.1599998
               z step: -1.04999995
               x start: 70.5080032
               x end: 59.6609993
               x step: -0.7180023
               y start: 11.6199999
               y end: 2.73200011
               y step: -0.8400002
           */
            float x = 70.508f;
            for (float y = 11.62f; y >= 2.63; y -= 0.81f)
            {
                for (float z = -3.6f; z >= -23.16; z -= 1.05f)
                {
                    if (Random.value < stadiumFillRatio)
                    {
                        float random_x_offset = (float)((Random.value - 0.5) * 0.6f);
                        GameObject newPerson = Instantiate(SelectRandomSpectator(), new Vector3(x, y, (float)(random_x_offset + z + 25.03 * stand)), Quaternion.identity);
                        newPerson.transform.Rotate(new Vector3(277.846069f, 90f, 23.5500908f));
                        newPerson.isStatic = true;
                        newPerson.transform.parent = crowdRoot.transform;
                        crowdList.Add(newPerson);
                    }

                    // Mirror the x-axis for the other side
                    if (Random.value < stadiumFillRatio)
                    {
                        float random_x_offset = (float)((Random.value - 0.5) * 0.6f);
                        GameObject newPerson = Instantiate(SelectRandomSpectator(), new Vector3(-x + 0.7f, y, (float)(random_x_offset + z + 1.2f + 25.03 * stand)), Quaternion.identity);
                        newPerson.transform.Rotate(new Vector3(277.846069f, 270f, 23.5500908f));
                        newPerson.isStatic = true;
                        newPerson.transform.parent = crowdRoot.transform;
                        crowdList.Add(newPerson);
                    }
                }
                x -= 1.0f;
            }
        }

        Debug.Log(crowdList.Count);
        GameObject[] gos = crowdList.ToArray();
        StaticBatchingUtility.Combine(gos, crowdRoot);
    }
}
