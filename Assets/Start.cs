using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class StartRace : MonoBehaviour
{
    private bool hit = false;
    private int enterbefore = 0;
    private int tempint = 2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (enterbefore != 3) //has the player passed the start line
            {
                hit = true;
                enterbefore++;
                Debug.Log("Lap " + enterbefore);
            }
            else
            {
                //Debug.Log("I hit");
                resettimer(); // once player hits the start line again 
            }

            tempint++;
        }
        
    }


    public TextMeshProUGUI TimerText;//the timer and best time
    public TextMeshProUGUI bestscore;
    private float CurrentTime = 0; //holds the current time of the lap as a float
    private float BestTime = 999999;

    void Start()
    {

    }

    void Update()
    {
        if (hit) //once player has passed the start line start time
        {
            tempfunc();
        }
    }

    void tempfunc()
    {
        CurrentTime += Time.deltaTime; //constantly increases timer 
        TimerText.text = CurrentTime.ToString("Lap 0.000"); //sets the Laptime to the current time
    }

    void resettimer()
    {
        if (CurrentTime < BestTime) //check if current lap time is quicker that previous tries
        {
            bestscore.text = CurrentTime.ToString("Best 0.000");//if so write do best time
            BestTime = CurrentTime;//holds fastest time
        }
        CurrentTime = 0; //resets the timer
        enterbefore = 0;
    }
}