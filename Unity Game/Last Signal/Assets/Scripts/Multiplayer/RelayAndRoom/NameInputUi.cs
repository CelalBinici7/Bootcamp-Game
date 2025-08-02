using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NameInputUi : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public GameObject namePanel;
    public GameObject mainPanel;
    public TMP_Text text;

    void Start()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            // Daha önce isim alınmış → sahneyi atla
            namePanel.SetActive(false);
            mainPanel.SetActive(true);
           // Networkmanager.Instance.PreparePlayer();
            text.text =  PlayerPrefs.GetString("PlayerName", "Player_" + Random.Range(1000, 9999));
        }
        else
        {
            namePanel.SetActive(true); // İsim paneli açık
            mainPanel.SetActive(false);
        }
       
    }

    public void OnConfirmNameClick()
    {
        string inputName = nameInputField.text;

        if (!string.IsNullOrEmpty(inputName))
        {
            PlayerPrefs.SetString("PlayerName", inputName);
            PlayerPrefs.Save();
            Networkmanager.Instance.PreparePlayer();
            namePanel.SetActive(false);
            mainPanel.SetActive(true);
            text.text = inputName;
            // SceneManager.LoadScene("MainMenu");
        }
    }

   
}
