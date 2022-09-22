using HarmonyLib;
using MelonLoader;
using System.Reflection;
using UnityEngine;

namespace LeaderboardFix
{
    public class Main : MelonMod
    {
        private static int page = 0;
        private static FieldInfo _leaderboadsRefInfo;

        public override void OnApplicationLateStart()
        {
            HarmonyLib.Harmony harmony = new("de.MOPSKATER.LeaderboardFix");

            MethodInfo target = typeof(LeaderboardIntegrationSteam).GetMethod("DownloadEntries");
            Debug.Log(target);
            HarmonyMethod patch = new(typeof(Main).GetMethod("PreDownloadEntries"));
            harmony.Patch(target, patch);

            target = typeof(Leaderboards).GetMethod("OnLeftArrowPressed");
            Debug.Log(target);
            patch = new(typeof(Main).GetMethod("PreOnLeftArrowPressed"));
            harmony.Patch(target, patch);

            target = typeof(Leaderboards).GetMethod("OnRightArrowPressed");
            Debug.Log(target);
            //Debug.Log(typeof(Main).GetMethod("PreOnRightArrowPressed"));
            patch = new(typeof(Main).GetMethod("PreOnRightArrowPressed"));
            harmony.Patch(target, patch);
        }

        public static bool PreOnLeftArrowPressed()
        {
            if (page > 0)
                page--;
            Debug.Log("--Current Page: " + page);
            return true;
        }

        public static bool PreOnRightArrowPressed()
        {
            page++;
            Debug.Log("++Current Page: " + page);
            return true;
        }

        public static bool PreDownloadEntries(ref int start, ref int end, ref bool friend, ref bool globalNeonRankings)
        {
            _leaderboadsRefInfo = typeof(LeaderboardIntegrationSteam).GetField("leaderboardsRef", BindingFlags.NonPublic | BindingFlags.Static);


            Debug.Log("1");
            if (!friend) return true;
            Debug.Log("2");

            if (!SteamManager.Initialized) return false;

            Debug.Log("3");

            ScoreData[] array = new ScoreData[10];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = LeaderboardIntegrationSteam.GetScoreDataAtGlobalRank(i + 1 + (page * 10), true, globalNeonRankings);
                Debug.Log(array[i]._username);
            }

            Leaderboards leaderboard = (Leaderboards)_leaderboadsRefInfo.GetValue(null);
            leaderboard.DisplayScores_AsyncRecieve(array, true);
            Debug.Log("4");
            return false;
        }
    }
}