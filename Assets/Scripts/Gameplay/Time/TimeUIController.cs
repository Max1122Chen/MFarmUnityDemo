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

    void HandleTimeChanged(int gameMinute, int gameHour, int gameDay, int gameMonth, int gameYear, GameSeason gameSeason)
    {
        int displayMinute = (gameMinute % 60 / 10) * 10; // Round down to nearest 10
        int displayHour = gameHour % 24; // Ensure hour wraps around after 23
        int displayDay = gameDay % 31;
        int displayMonth = gameMonth % 13;
        GameSeason displaySeason = gameSeason;

        gameDateText.text = $"{displayDay:00}/{displayMonth:00}/{gameYear:0000}";
        gameTimeText.text = $"{displayHour:00}:{displayMinute:00}";

        if(gameHour != lastGameHour)
        {
            lastGameHour = gameHour;
            UpdateDayProgress(displayHour);
            UpdateDayNightIcon(displayHour);
        }
        if(gameSeason != lastGameSeason)
        {
            lastGameSeason = gameSeason;
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

    private void UpdateDayNightIcon(int hour)
    {
        
    }

    private void UpdateSeasonIcon(GameSeason season)
    {
        seasonIcon.sprite = seasonIcons[(int)season];
    }
}
