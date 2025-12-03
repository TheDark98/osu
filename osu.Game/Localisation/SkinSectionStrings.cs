// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SkinSectionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SkinSection";

        /// <summary>
        /// "Use Custom Skin Sounds"
        /// </summary>
        public static LocalisableString UseCustomSkinSounds => new TranslatableString(getKey(@"use_custom_skin_sounds"), @"Use Custom Skin Sounds");

        /// <summary>
        /// "Sound Skin"
        /// </summary>
        public static LocalisableString SoundSkin => new TranslatableString(getKey(@"sound_skin"), @"Sound Skin");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}