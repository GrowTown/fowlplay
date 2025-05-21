using UnityEngine;
using UnityEngine.UI;

public class ButtonFunction : MonoBehaviour
{
    [SerializeField] GameObject musicPanel;
    private void Start()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(()=>
        {
            musicPanel.SetActive(true);
            Audio_Manager.Instance.PlayMusic(Audio_Manager.Instance.buttonclick, Audio_Manager.Instance.sfxVolume);
        });
    }
}
