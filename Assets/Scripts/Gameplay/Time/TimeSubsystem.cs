using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeSystem
{
    public enum GameSeason
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
    public class TimeSubsystem : Singleton<TimeSubsystem>
    {
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
        public Action<int, int, int, int, int, GameSeason> OnTimeChanged;

        private void Start()
        {
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
            realTimeAccumulator += Time.deltaTime;
            while(realTimeAccumulator >= GameInstance.Instance.gameSettings.timePerGameMinute)
            {
                realTimeAccumulator -= GameInstance.Instance.gameSettings.timePerGameMinute;
                UpdateGameTime(1);
            }
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

            BroadcastTimePassedEvents(oldMinute, oldHour, oldDay, oldMonth, oldYear, oldSeason);
            BroadcastTimeChangedEvent();

        }

        private void BroadcastTimePassedEvents(int oldMinute, int oldHour, int oldDay, int oldMonth, int oldYear, GameSeason oldSeason)
        {
            if(gameMinute != oldMinute)
            {
                onMinutePassed?.Invoke(gameMinute - oldMinute);
            }
            if(gameHour != oldHour)
            {
                onHourPassed?.Invoke(gameHour - oldHour);
            }
            if(gameDay != oldDay)
            {
                onDayPassed?.Invoke(gameDay - oldDay);
            }
            if(gameMonth != oldMonth)
            {
                onMonthPassed?.Invoke(gameMonth - oldMonth);
            }
            if(gameYear != oldYear)
            {
                onYearPassed?.Invoke(gameYear - oldYear);
            }
            if(gameSeason != oldSeason)
            {
                onSeasonPassed?.Invoke(gameSeason);
            }

            OnTimeChanged?.Invoke(gameMinute, gameHour, gameDay, gameMonth, gameYear, gameSeason);
        }

        private void BroadcastTimeChangedEvent()
        {
            // Debug.Log($"Time Changed: minute={gameMinute}, hour={gameHour}, day={gameDay}, month={gameMonth}, year={gameYear}, season={gameSeason}");
            OnTimeChanged?.Invoke(gameMinute, gameHour, gameDay, gameMonth, gameYear, gameSeason);
        }
    }

}