// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class PerformanceCalculator
    {
        protected readonly Ruleset Ruleset;

        protected double HardHitMuliplier; //multiplier to balance spike weigth
        protected double EasyHitMuliplier; //multiplier to balance filler weigth


        protected double HardLengthBonus; //multiplier to balance filler weigth
        protected double EasyLengthBonus; //multiplier to balance filler weigth

        protected PerformanceCalculator(Ruleset ruleset)
        {
            Ruleset = ruleset;
        }

        public Task<PerformanceAttributes> CalculateAsync(ScoreInfo score, DifficultyAttributes attributes, CancellationToken cancellationToken)
            => Task.Run(() => CreatePerformanceAttributes(score, attributes), cancellationToken);

        public PerformanceAttributes Calculate(ScoreInfo score, DifficultyAttributes attributes)
            => CreatePerformanceAttributes(score, attributes);

        public PerformanceAttributes Calculate(ScoreInfo score, IWorkingBeatmap beatmap)
            => Calculate(score, Ruleset.CreateDifficultyCalculator(beatmap).Calculate(score.Mods));

        /// <summary>
        /// Creates <see cref="PerformanceAttributes"/> to describe a score's performance.
        /// </summary>
        /// <param name="score">The score to create the attributes for.</param>
        /// <param name="attributes">The difficulty attributes for the beatmap relating to the score.</param>
        protected abstract PerformanceAttributes CreatePerformanceAttributes(ScoreInfo score, DifficultyAttributes attributes);

        protected void CalculateBaseLengthBonus(double basePP, double difficultyFactor, int totalHits)
        {
            basePP *= 0.0001;

            double hardHits = totalHits * difficultyFactor;
            double easyHits = totalHits - hardHits;

            HardLengthBonus = basePP * HardHitMuliplier * hardHits; //Length bonus for hard hit with basePP * offset
            EasyLengthBonus = basePP * EasyHitMuliplier * easyHits; //Length bonus for easy hit with basePP * offset
        }
    }
}
