using UnityEngine;

public class TabSwitcher : MonoBehaviour
{
    public GameObject[] panels;
    public AudioSource audioSource;
    public AudioClip pageAudio;

    private int tempIndex = -1;

    public void OpenTab(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == index);
        }
        if (tempIndex == index) return;
        tempIndex = index;
        audioSource.PlayOneShot(pageAudio);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
