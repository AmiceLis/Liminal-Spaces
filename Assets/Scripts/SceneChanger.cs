using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{

    public bool changeAtSpecificTime = false;
    public float sceneTimer = 0;
    public int sceneTimerLimit;

    public int nextSceneNum = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        sceneTimer += Time.deltaTime;

        if (changeAtSpecificTime == true)
        {
            // trigger fade out


            //call for scenechange after delay
            Invoke("ChangeToNextScene", 3);
        }

    }


    void ChangeToNextScene()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        SceneManager.LoadScene(nextSceneNum);
    }

}
