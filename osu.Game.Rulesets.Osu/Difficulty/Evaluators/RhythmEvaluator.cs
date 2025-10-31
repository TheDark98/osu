// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
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
            var osuHCurrObj = osuCurrObj; // Historic object for rhythm calculations

            double rhythmDifficulty = 2 / osuCurrObj.HitWindowGreat;

            for (int i = 0; i < Math.Min(history_objects_max, current.Index); i++)
            {
                var osuHPrevObj = (OsuDifficultyHitObject)current.Previous(i);

                if (osuHCurrObj.BaseObject is Spinner || osuHPrevObj.BaseObject is Spinner)
                    continue;

                double historicDeltaTime = osuCurrObj.StartTime - osuHPrevObj.StartTime;

                if (historicDeltaTime > history_time_max)
                    break;

                double timeRatio = Math.Max(osuHCurrObj.DeltaTime, 1) / Math.Max(osuHPrevObj.DeltaTime, 1);
                double rhythmComplexity = Math.Pow(Math.Abs(1 - timeRatio), 2);

                // Weight by time distance
                double timeWeight = 1 - (historicDeltaTime / history_time_max);
                rhythmDifficulty *= rhythmComplexity * timeWeight * rhythm_ratio_multiplier;

                osuHCurrObj = osuHPrevObj;
            }

            return rhythmDifficulty * rhythm_overall_multiplier;
        }
    }
}
