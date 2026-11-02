using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.SceneManagement;

public class ExitBox : MonoBehaviour
{
    public bool LastScene = false;
    public int ourVRLevel = 0;

    public GameObject currentVRLevelChunk;
    public GameObject nextVRLevelChunk;

    public GameObject player;

    //public MainLevelController ourMLC;

    public TMPro.TMP_Text debugText;

    public int counter = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != null)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log("Player collided with exit box");
                counter++;

                currentVRLevelChunk = this.transform.parent.gameObject;

                TransportAudience();
            }
        }
    }

    public void TransportAudience()
    {
        if (LastScene == true)
        {
            // What to do when the level is over... 
            //ourMLC.vrLevelsComplete[ourVRLevel] = true;
            return;
        }

        if (LastScene == false)
        {
            // if it's not the last scene, the exchange VR level chunks
            //currentVRLevelChunk.SetActive(false);
            //nextVRLevelChunk.SetActive(true);

            nextVRLevelChunk.transform.parent.gameObject.SetActive(true);
            currentVRLevelChunk.SetActive(false);
            

            player.transform.parent.transform.position = nextVRLevelChunk.transform.position;
            player.transform.localPosition = Vector3.zero;

        }
            
    }

}
