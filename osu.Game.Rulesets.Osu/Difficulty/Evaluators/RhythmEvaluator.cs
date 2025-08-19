// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class RhythmEvaluator
    {
        private const int history_time_max = 5 * 1000; // 5 seconds
        private const int history_objects_max = 32;
        private const double rhythm_overall_multiplier = 1.0;
        private const double rhythm_ratio_multiplier = 1.0;

        /// <summary>
        /// Calculates a rhythm multiplier for the difficulty of the tap associated with historic data of the current <see cref="OsuDifficultyHitObject"/>.
        /// </summary>
        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            if (current.BaseObject is Spinner && current.Index < 2)
                return 0;

            var osuCurrObj = (OsuDifficultyHitObject)current;

            List<deltaTimeCount> rhythmList = rhythmGroup(osuCurrObj);

            double rhythmDifficulty = computeRhythm(rhythmList) * rhythm_overall_multiplier;

            return rhythmDifficulty;
        }

        private static double computeRhythm(List<deltaTimeCount> rhythmList)
        {
            double totalComplexity = 1;

            foreach (var rhythm in rhythmList)
            {
                int noteCount = rhythm.RhythmIndexCount;
                double chunkComplexity = 1;

                foreach (var osuCurrObj in rhythm.OsuDifficultyHitObjList)
                {
                    var osuPrevObj = (OsuDifficultyHitObject)osuCurrObj.Previous(0);
                    var osuPrev2Obj = (OsuDifficultyHitObject)osuCurrObj.Previous(1);

                    double currDeltaTime = Math.Max(osuCurrObj.DeltaTime, 1);
                    double prevDeltaTime = Math.Max(osuPrevObj.DeltaTime, 1);

                    double currDeltaTimeHead = osuPrev2Obj.DeltaTime;
                    if (osuPrevObj.BaseObject is Slider)
                        currDeltaTimeHead += osuPrevObj.LazyTravelTime;

                    double prevDeltaTimeHead = osuPrevObj.DeltaTime;
                    if (osuPrev2Obj.BaseObject is Slider)
                        prevDeltaTimeHead += osuPrev2Obj.LazyTravelTime;

                    double deltaTimeMultiplierHead = deltaTimeMultiplier(currDeltaTimeHead, prevDeltaTimeHead);

                    double complexity = Math.Min(DifficultyCalculationUtils.Smootherstep(deltaTimeMultiplierHead, 2, 1.5),
                                                     DifficultyCalculationUtils.Smootherstep(deltaTimeMultiplierHead, 1, 1.5));

                    double hitWindowComplexity = osuCurrObj.HitWindowGreat / currDeltaTimeHead;
                    chunkComplexity += complexity / hitWindowComplexity;
                }
                chunkComplexity /= noteCount;

                totalComplexity += chunkComplexity * rhythm_ratio_multiplier;
            }

            totalComplexity /= rhythmList.Count;

            return totalComplexity;
        }

        private static List<deltaTimeCount> rhythmGroup(OsuDifficultyHitObject initial)
        {
            var startObj = initial;

            int maxHystory = Math.Min(history_objects_max - 1, startObj.Index);

            List<deltaTimeCount> deltaTimeCountsList = new List<deltaTimeCount>();
            List<OsuDifficultyHitObject> osuDifficultyHitObjList = new List<OsuDifficultyHitObject>(maxHystory);

            int totalIndexCount = 0;
            double totalDeltaTime = 0;

            for (int i = 0; totalIndexCount < maxHystory && i < maxHystory && totalDeltaTime < history_time_max; i++)
            {
                int indexCount = rhythmIndexCount(startObj);

                deltaTimeCount currDeltaTimeCount = new deltaTimeCount(osuDifficultyHitObjList, indexCount, i);

                deltaTimeCountsList.Add(currDeltaTimeCount);

                totalIndexCount += indexCount;

                startObj = (OsuDifficultyHitObject)startObj.Previous(totalIndexCount - 1);
            }

            return deltaTimeCountsList;

            int rhythmIndexCount(OsuDifficultyHitObject start)
            {
                var curr = start;

                int maxHystory = Math.Min(history_objects_max, start.Index);

                int count = 1;

                double prevMultiplier = 0;
                totalDeltaTime = curr.DeltaTime;

                for (int i = 0; i < maxHystory && totalDeltaTime < maxHystory; i++)
                {
                    var prev = (OsuDifficultyHitObject)curr.Previous(0);
                    var prev2 = (OsuDifficultyHitObject)curr.Previous(1);

                    double currDeltaTimeHead = curr.DeltaTime;
                    if (prev.BaseObject is Slider)
                        currDeltaTimeHead += prev.LazyTravelTime;

                    double prevDeltaTimeHead = prev.DeltaTime;
                    if (prev2.BaseObject is Slider)
                        currDeltaTimeHead += prev2.LazyTravelTime;

                    double multiplier = deltaTimeMultiplier(currDeltaTimeHead, prevDeltaTimeHead);

                    double deltaMultiplier = multiplier - prevMultiplier;

                    if (deltaMultiplier < 0.125)
                        osuDifficultyHitObjList.Add(curr);

                    if (deltaMultiplier > 0.125 && i > 0)
                    {
                        count = i + 1;
                        break;
                    }

                    prevMultiplier = multiplier;
                    totalDeltaTime += prev.DeltaTime;
                    curr = prev;
                }

                return count;
            }
        }

        private static double deltaTimeMultiplier(double deltaTime1, double deltaTime2)
        {
            double maxDeltaTime = Math.Max(deltaTime1, deltaTime2);
            double minDeltaTime = Math.Max(Math.Min(deltaTime1, deltaTime2), 1);

            return maxDeltaTime / minDeltaTime;
        }

        private readonly struct deltaTimeCount
        {
            public deltaTimeCount(List<OsuDifficultyHitObject> osuDifficultyHitObjList, int rhythmIndexCount, int id)
            {
                OsuDifficultyHitObjList = osuDifficultyHitObjList;
                RhythmIndexCount = rhythmIndexCount;
                Id = id;
            }
            public List<OsuDifficultyHitObject> OsuDifficultyHitObjList { get; init; }
            public int RhythmIndexCount { get; init; }
            public int Id { get; init; }

            public override string ToString() => $"({OsuDifficultyHitObjList}, {RhythmIndexCount})";
        }
    }
}
