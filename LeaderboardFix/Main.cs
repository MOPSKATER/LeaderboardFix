﻿using HarmonyLib;
using MelonLoader;
using System.Reflection;
using static MelonLoader.MelonLogger;

namespace LeaderboardFix
{
    public class Main : MelonMod
    {
        private static int page = 0;
        private static readonly FieldInfo _leaderboadsRefInfo = typeof(LeaderboardIntegrationSteam).GetField("leaderboardsRef", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly FieldInfo friendsFilter = typeof(Leaderboards).GetField("friendsFilter", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void OnApplicationLateStart()
        {
            HarmonyLib.Harmony harmony = new("de.MOPSKATER.LeaderboardFix");

            MethodInfo target = typeof(LeaderboardIntegrationSteam).GetMethod("DownloadEntries");
            HarmonyMethod patch = new(typeof(Main).GetMethod("PreDownloadEntries"));
            harmony.Patch(target, patch);

            target = typeof(Leaderboards).GetMethod("OnLeftArrowPressed");
            patch = new(typeof(Main).GetMethod("PreOnLeftArrowPressed"));
            harmony.Patch(target, patch);

            target = typeof(Leaderboards).GetMethod("OnRightArrowPressed");
            patch = new(typeof(Main).GetMethod("PreOnRightArrowPressed"));
            harmony.Patch(target, patch);
        }

        public static bool PreOnLeftArrowPressed(Leaderboards __instance)
        {
            
            if ((bool) friendsFilter.GetValue(__instance) && page > 0)
                page--;
            return true;
        }

        public static bool PreOnRightArrowPressed(Leaderboards __instance)
        {
            if ((bool)friendsFilter.GetValue(__instance))
                page++;
            return true;
        }

        public static bool PreDownloadEntries(ref int start, ref int end, ref bool friend, ref bool globalNeonRankings)
        {
            if (!friend) return true;

            if (!SteamManager.Initialized) return false;

            ScoreData[] array = new ScoreData[10];
            for (int i = 0; i < array.Length; i++)
                array[i] = LeaderboardIntegrationSteam.GetScoreDataAtGlobalRank(i + 1 + (page * 10), true, globalNeonRankings);

            Leaderboards leaderboard = (Leaderboards)_leaderboadsRefInfo.GetValue(null);
            leaderboard.DisplayScores_AsyncRecieve(array, true);
            return false;
        }
    }
}