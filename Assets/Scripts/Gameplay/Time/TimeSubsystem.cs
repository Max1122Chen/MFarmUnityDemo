using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeSystem
{
    [System.Serializable]
    public enum GameSeason
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    [System.Serializable]
    public class GameTime : IComparable<GameTime>
    {
        public int minute;
        public int hour;
        public int day;
        public int month;
        public int year;
        public GameSeason season { 
            get
            {
                return (GameSeason)(((month - 1) / 3) % 4);
            }
        }

        public GameTime(int minute, int hour, int day, int month, int year)
        {
            this.minute = minute;
            this.hour = hour;
            this.day = day;
            this.month = month;
            this.year = year;
        }

        public int CompareTo(GameTime other)
        {
            if (other == null) return 1;

            if (year != other.year) return year.CompareTo(other.year);
            if (month != other.month) return month.CompareTo(other.month);
            if (day != other.day) return day.CompareTo(other.day);
            if (hour != other.hour) return hour.CompareTo(other.hour);
            if (minute != other.minute) return minute.CompareTo(other.minute);
            return 0;
        }
    }
    public class TimeSubsystem : Singleton<TimeSubsystem>
    {
        private bool isTimePaused = false;
        private float realTimeAccumulator; // Measured in seconds. To track the accumulated real time for converting to game time.

        // Record the time has passed in the game. Time is measured in game minutes, hours, days, months, years, and seasons.
        private int gameMinute, gameHour, gameDay, gameMonth, gameYear;
        private GameSeason gameSeason;

        public Action<int> onMinutePassed;
        public Action<int> onHourPassed;
        public Action<int> onDayPassed;
        public Action<int> onMonthPassed;
        public Action<int> onYearPassed;
        public Action<GameSeason> onSeasonPassed;
        public Action<GameTime> OnTimeChanged;

        // TODO:
        public void Initialize(GameSaveData saveData)
        {
            
        }
        private void Start()
        {
            GameMapSubsystem.Instance.onNewSceneLoaded += (string sceneName) => { isTimePaused = false; };
            GameMapSubsystem.Instance.onOldSceneStartUnloading += (string sceneName) => { isTimePaused = true; };

            // TODO: For testing purposes, we can set the time to start at 6 AM on the first day of spring.
            gameMinute = 0;
            gameHour = 6; // Start at 6 AM
            gameDay = 1;
            gameMonth = 1;
            gameYear = 1;
            gameSeason = GameSeason.Spring;
            
            BroadcastTimeChangedEvent();
        }

        public void Update()
        {
            if(isTimePaused)
            {
                return;
            }

            realTimeAccumulator += Time.deltaTime;
            while(realTimeAccumulator >= GameInstance.Instance.gameSettings.timePerGameMinute)
            {
                realTimeAccumulator -= GameInstance.Instance.gameSettings.timePerGameMinute;
                UpdateGameTime(1);
            }
        }

        public GameTime GetCurrentGameTime()
        {
            return new GameTime(gameMinute, gameHour, gameDay, gameMonth, gameYear);
        }

        private void UpdateGameTime(int minutesToAdd)
        {
            minutesToAdd = Mathf.RoundToInt(minutesToAdd * GameInstance.Instance.gameSettings.timeScale);

            int oldMinute = gameMinute;
            int oldHour = gameHour;
            int oldDay = gameDay;
            int oldMonth = gameMonth;
            int oldYear = gameYear;
            GameSeason oldSeason = gameSeason;

            gameMinute += minutesToAdd;
            gameHour = gameMinute / 60;
            gameDay = gameHour / 24;
            gameMonth = (gameDay - 1) / 30;
            gameYear = (gameMonth - 1) / 12;
            gameSeason = (GameSeason)(((gameMonth - 1) / 3) % 4);

            BroadcastTimePassedEvents(new GameTime(oldMinute, oldHour, oldDay, oldMonth, oldYear));
            BroadcastTimeChangedEvent();

        }

        private void BroadcastTimePassedEvents(GameTime oldTime)
        {
            if(gameMinute != oldTime.minute)
            {
                onMinutePassed?.Invoke(gameMinute - oldTime.minute);
            }
            if(gameHour != oldTime.hour)
            {
                onHourPassed?.Invoke(gameHour - oldTime.hour);
            }
            if(gameDay != oldTime.day)
            {
                onDayPassed?.Invoke(gameDay - oldTime.day);
            }
            if(gameMonth != oldTime.month)
            {
                onMonthPassed?.Invoke(gameMonth - oldTime.month);
            }
            if(gameYear != oldTime.year)
            {
                onYearPassed?.Invoke(gameYear - oldTime.year);
            }
            if(gameSeason != oldTime.season)
            {
                onSeasonPassed?.Invoke(gameSeason);
            }

            GameTime time = new GameTime(gameMinute, gameHour, gameDay, gameMonth, gameYear);
        }

        private void BroadcastTimeChangedEvent()
        {
            // Debug.Log($"Time Changed: minute={gameMinute}, hour={gameHour}, day={gameDay}, month={gameMonth}, year={gameYear}, season={gameSeason}");
            GameTime time = new GameTime(gameMinute, gameHour, gameDay, gameMonth, gameYear);
            OnTimeChanged?.Invoke(time);
        }
    }

}