using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveNickname : MonoBehaviour
{
    public TMP_InputField nicknameInput;
    public TMP_Text title;
    
    public void SaveNickNameFromInputField()
    {
        if (nicknameInput.text.Length < 3)
        {
            title.text = "Nick has to be longer than 3 characters!";
        } else
        {
            PlayerPrefs.SetString("LocalNickName", nicknameInput.text.ToString());
            gameObject.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = "Saved!";
            gameObject.GetComponent<Button>().interactable = false;
        }
    }
}
