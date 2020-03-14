using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class img : MonoBehaviour {
  
    public RawImage rimage;
    public bool marker = false;
    public float timer;
    public GameObject menu;

	// Use this for initialization
	void Start () {
        timer = 0f;
      //  menu.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if(marker == true)
        {
            rimage.enabled = true;
            timer += Time.deltaTime;
        }
        else
        {
            rimage.enabled = false;
            timer = 0f;
        }

        if (timer > 5f)
        {
            menu.SetActive(true);
        }
	}
}
