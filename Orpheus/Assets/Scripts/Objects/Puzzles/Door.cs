using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

    public float openSpeed = 3f;
    private float currentLerpTime;
    public bool isOpened;
    
    public AudioClip openSound;
    private AudioSource theAudio;
    private SpriteRenderer doorSprite;

    

    // where the door starts
    private Vector3 doorStartPosition, doorEndPosition;


    // Use this for initialization
    void Start () {
        theAudio = GetComponent<AudioSource>();
        doorSprite = GetComponent<SpriteRenderer>();


        doorStartPosition = transform.position;  
        float doorHeight = doorSprite.bounds.max.y - doorSprite.bounds.min.y; // the door's height, essentially the distance we want to move
        doorEndPosition = new Vector3(doorStartPosition.x, doorStartPosition.y - doorHeight, doorStartPosition.z);
    }
	
    public void open()
    {
        // play the opening sound
        theAudio.PlayOneShot(openSound);
        Debug.Log("Open? " + isOpened);
        if (!isOpened) StartCoroutine(MoveObject(doorStartPosition, doorEndPosition, openSpeed));
    }

    IEnumerator MoveObject(Vector3 source, Vector3 target, float overTime)
    {
        Debug.Log("opening door...");
        float startTime = Time.time;
        while (Time.time < startTime + overTime)
        {
            transform.position = Vector3.Lerp(source, target, (Time.time - startTime) / overTime);
            yield return null;
        }
        transform.position = target;
        isOpened = true;
    }
}
