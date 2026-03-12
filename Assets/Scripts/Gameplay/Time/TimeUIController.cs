using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeSystem;
using UnityEngine.UI;
using TMPro;
public class TimeUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameDateText;
    [SerializeField] private TextMeshProUGUI gameTimeText;
    [SerializeField] private List<Image> dayProgressImages = new List<Image>();
    [SerializeField] private GameObject dayTimeIcon;
    [SerializeField] private Image seasonIcon;

    [Header("Season Icons")]
    [SerializeField] private List<Sprite> seasonIcons = new List<Sprite>();

    private int lastGameHour = -1;
    private GameSeason lastGameSeason = GameSeason.Spring;

    private TimeSubsystem timeSubsystem;
    void Awake()
    {
        timeSubsystem = TimeSubsystem.Instance;
        timeSubsystem.OnTimeChanged += HandleTimeChanged;

    }

    void Start()
    {
    }

    void HandleTimeChanged(GameTime time)
    {
        int displayMinute = (time.minute % 60 / 10) * 10; // Round down to nearest 10
        int displayHour = time.hour % 24; // Ensure hour wraps around after 23
        int displayDay = time.day % 31;
        int displayMonth = time.month % 13;
        GameSeason displaySeason = time.season;

        gameDateText.text = $"{displayDay:00}/{displayMonth:00}/{time.year:0000}";
        gameTimeText.text = $"{displayHour:00}:{displayMinute:00}";

        if(time.hour != lastGameHour)
        {
            lastGameHour = time.hour;
            UpdateDayProgress(displayHour);
            UpdateDayTimeIcon(displayHour);
        }
        if(time.season != lastGameSeason)
        {
            lastGameSeason = time.season;
            UpdateSeasonIcon(displaySeason);
        }
    }
    private void UpdateDayProgress(int hour)
    {
        float progress = hour / 24f;
        int activeSegments = Mathf.FloorToInt(progress * dayProgressImages.Count);
        for(int i = 0; i < dayProgressImages.Count; i++)
        {
            dayProgressImages[i].enabled = i < activeSegments;
        }
    }

    private void UpdateDayTimeIcon(int hour)
    {
        if(hour == 6 || hour == 10 || hour == 16 || hour == 20)
        {
            StartCoroutine(UpdateDayTimeIconCoroutine());
        }
    }

    private IEnumerator UpdateDayTimeIconCoroutine()
    {
        float elapsedTime = 0f;
        float animationDuration = 1f;
        // Rotate the icon 90 degrees anti-clockwise over the course of the animation duration
        while(elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            dayTimeIcon.transform.Rotate(Vector3.forward, 90f * Time.deltaTime / animationDuration);
            yield return null;
        }
    }

    private void UpdateSeasonIcon(GameSeason season)
    {
        seasonIcon.sprite = seasonIcons[(int)season];
    }
}
