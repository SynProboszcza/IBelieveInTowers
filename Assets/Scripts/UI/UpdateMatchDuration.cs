using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateMatchDuration : MonoBehaviour
{
    public Text showMatchDuration;
    public int minimumMatchTime = 30;
    public int maximumMatchTime = 300;
    public int step = 5;
    [Tooltip("Default time if not set. Set to be max time by default")]
    public int secondsMatchShouldBe = 300;

    private void Start()
    {
        showMatchDuration = transform.Find("MatchDuration").GetComponent<Text>();
        secondsMatchShouldBe = maximumMatchTime;
    }
    public void UpdateShownTime()
    {
        float _floatTime = Mathf.Lerp(minimumMatchTime, maximumMatchTime, gameObject.GetComponent<Slider>().value);
        int steppedTime = Mathf.FloorToInt(_floatTime);
        if ((steppedTime % step) == 0)
        {
            _floatTime = steppedTime;
        }
        else
        {
            _floatTime = ((steppedTime / step) + 1) * step;
        }
        int seconds = Mathf.FloorToInt(_floatTime % 60);
        int minutes = Mathf.FloorToInt(_floatTime / 60);
        showMatchDuration.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        secondsMatchShouldBe = Mathf.FloorToInt(_floatTime);
    }
}
