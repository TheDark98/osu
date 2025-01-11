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

        private double hardHitMuliplier; //multiplier to balance spike weigth
        private double easyHitMuliplier; //multiplier to balance filler weigth

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

        protected double CalculateBaseLengthBonus(double basePP, double difficultyFactor, int totalHits)
        {
            double hardHits = totalHits * difficultyFactor;
            double easyHits = totalHits - hardHits;

            double hardLengthBonus = basePP * 0.0001 * hardHitMuliplier * hardHits; //Length bonus for hard hit with basePP * offset
            double easyLengthBonus = basePP * 0.0001 * easyHitMuliplier * easyHits; //Length bonus for easy hit with basePP * offset
            return hardLengthBonus + easyLengthBonus;
        }

        protected void SetHitMultipliers(double hardHitMuliplier, double easyHitMuliplier)
        {
            this.hardHitMuliplier = hardHitMuliplier;
            this.easyHitMuliplier = easyHitMuliplier;
        }
    }
}
