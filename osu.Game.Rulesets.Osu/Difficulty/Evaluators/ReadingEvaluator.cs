// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class ReadingEvaluator
    {
        private const double max_opacity_bonus = 0.4;
        private const double hidden_bonus = 0.2;

        private const double min_velocity = 0.5;
        private const double slider_multiplier = 1.3;

        private const double min_angle_multiplier = 0.2;

        private static double density;
        private static double cloackRate;
        private static double approachRate;
        private static double preempt;

        /// <summary>
        /// Evaluates the difficulty of memorising and hitting an object, based on:
        /// <list type="bullet">
        /// <item><description>distance between a number of previous objects and the current object,</description></item>
        /// <item><description>the visual opacity of the current object,</description></item>
        /// <item><description>the angle made by the current object,</description></item>
        /// <item><description>length and speed of the current object (for sliders),</description></item>
        /// <item><description>and whether the hidden mod is enabled.</description></item>
        /// </list>
        /// </summary>
        public static double EvaluateDifficultyOf(IReadOnlyList<Mod> mods, DifficultyHitObject current, IBeatmap beatmap)
        {
            if (current.BaseObject is Spinner || current.Index < 2 || current.LastObject is Spinner)
                return 0;

            var osuCurrent = (OsuDifficultyHitObject)current;

            bool isHidden = mods.Any(h => h is OsuModHidden);

            approachRate = beatmap.Difficulty.ApproachRate;

            //CloakRate is influenced only by DT/HT; NM remains fixed at 1.0.
            cloackRate = 1.0;

            if (mods.Any(h => h is OsuModDoubleTime))
            {
                OsuModDoubleTime doubleTime = (OsuModDoubleTime)mods.First(h => h is OsuModDoubleTime);
                cloackRate = doubleTime.SpeedChange.Value;
            }
            else if (mods.Any(h => h is OsuModNightcore))
            {
                OsuModNightcore nightcore = (OsuModNightcore)mods.First(h => h is OsuModNightcore);
                cloackRate = nightcore.SpeedChange.Value;
            }
            else if (mods.Any(h => h is OsuModHalfTime))
            {
                OsuModHalfTime halfTime = (OsuModHalfTime)mods.First(h => h is OsuModHalfTime);
                cloackRate = halfTime.SpeedChange.Value;
            }
            else if (mods.Any(h => h is OsuModDaycore))
            {
                OsuModDaycore daycore = (OsuModDaycore)mods.First(h => h is OsuModDaycore);
                cloackRate = daycore.SpeedChange.Value;
            }

            preempt = IBeatmapDifficultyInfo.DifficultyRange(, 1800, 1200, 450) / cloackRate;

            //Density is calculated as approach rate (ms) / current strain.
            density = preempt / osuCurrent.StrainTime;

            double overlapp = processOverlapp(osuCurrent);

            double approachRateCurve;
            if (isHidden)
                approachRateCurve = 0.15 * (13.0 - approachRate);
            else
                approachRateCurve = approachRate < 10.33 ? 0.05 * (13.0 - approachRate) : 0.3 * (approachRate - 10.33);

            double visualDensity = (1.0 + approachRateCurve) * (1.0 + density) * (1.0 + overlapp);

            return visualDensity;
        }

        private static double processOverlapp(DifficultyHitObject current)
        {
            if (current.Index < 4)
                return 0.0;

            List<OsuDifficultyHitObject> obj = [(OsuDifficultyHitObject)current, (OsuDifficultyHitObject)current.Previous(0), (OsuDifficultyHitObject)current.Previous(1), (OsuDifficultyHitObject)current.Previous(2)];

            double[] strainsTimes = [obj[0].StrainTime, obj[1].StrainTime, obj[2].StrainTime, obj[3].StrainTime];

            double?[] angleValue = [obj[0].Angle.Value, obj[1].Angle.Value, obj[2].Angle.Value, obj[3].Angle.Value];

            if (strainsTimes.Average() != strainsTimes[0])
                return 0.0;

            double[] densitys = [preempt / strainsTimes[0], preempt / strainsTimes[1], preempt / strainsTimes[1], preempt / strainsTimes[1]];

            double[] lazyJumps = [obj[0].StrainTime, obj[1].StrainTime, obj[2].StrainTime, obj[3].StrainTime];

            return 0.0;
        }

    }
}
