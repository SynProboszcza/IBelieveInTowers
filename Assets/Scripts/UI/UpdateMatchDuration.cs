using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateMatchDuration : MonoBehaviour
{
    public Text showMatchDuration;
    public int minimumMatchTime = 20;
    public int maximumMatchTime = 300;
    public int secondsMatchShouldBe = 300;

    private void Start()
    {
        showMatchDuration = transform.Find("MatchDuration").GetComponent<Text>();
        secondsMatchShouldBe = maximumMatchTime;
    }
    public void UpdateShownTime()
    {
        float _floatTime = Mathf.Lerp(minimumMatchTime, maximumMatchTime, gameObject.GetComponent<Slider>().value);
        int seconds = Mathf.FloorToInt(_floatTime % 60);
        int minutes = Mathf.FloorToInt(_floatTime / 60);
        showMatchDuration.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        secondsMatchShouldBe = Mathf.FloorToInt(_floatTime);
    }
}
