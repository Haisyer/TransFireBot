using PKHeX.Core;
using System.ComponentModel;
using SysBot.Base;
using System.Collections.Generic;
using System.Threading;
using System;

namespace SysBot.Pokemon
{
    public class RaidSettingsSV : IBotStateSettings, ICountSettings
    {
        private const string Hosting = nameof(Hosting);
        private const string Counts = nameof(Counts);
        private const string FeatureToggle = nameof(FeatureToggle);
        public override string ToString() => "Raid Bot Settings";

        [Category(FeatureToggle), Description("URL to Pokémon Automation's Tera Ban List json (or one matching the required structure).")]
        public string BanListURL { get; set; } = "https://raw.githubusercontent.com/PokemonAutomation/ServerConfigs-PA-SHA/main/PokemonScarletViolet/TeraAutoHost-BanList.json";

        [Category(Hosting), Description("Amount of raids before updating the ban list. If you want the global ban list off, set this to -1.")]
        public int RaidsBetweenUpdate { get; set; } = 3;

        [Category(FeatureToggle), Description("Raid embed title.")]
        public string RaidEmbedTitle { get; set; } = "Tera Raid Notification";

        [Category(FeatureToggle), Description("Raid embed description.")]
        public string[] RaidEmbedDescription { get; set; } = Array.Empty<string>();

        [Category(Hosting), Description("Input the Species to post a Thumbnail in the embeds. Ignored if 0.")]
        public Species RaidSpecies { get; set; } = Species.None;

        [Category(Hosting), Description("If the species does not have an alternate form, leave at 0.")]
        public int RaidSpeciesForm { get; set; } = 0;

        [Category(Hosting), Description("If the species is shiny set to true. False for non-shiny.")]
        public bool RaidSpeciesIsShiny { get; set; } = true;

        [Category(Hosting), Description("Raid Species IVs")]
        public string RaidSpeciesIVs { get; set; } = String.Empty;

        [Category(Hosting), Description("Raid Species nature")]
        public Nature RaidSpeciesNature { get; set; } = Nature.Random;

        [Category(Hosting), Description("Raid Species ability")]
        public Ability RaidSpeciesAbility { get; set; } = Ability.Adaptability;

        [Category(FeatureToggle), Description("If true, the bot will use a random code for the raid.")]
        public bool CodeTheRaid { get; set; } = true;

        [Category(FeatureToggle), Description("If true, the bot will post the raid code in embed info.")]
        public bool CodeInInfo { get; set; } = false;

        [Category(FeatureToggle), Description("If true, split the code and hide with spolier tag")]
        public bool CodeIfSplitHidden { get; set; } = false;

        [Category(Hosting), Description("Catch limit per player before they get added to the ban list automatically. If set to 0 this setting will be ignored.")]
        public int CatchLimit { get; set; } = 0;

        [Category(Hosting), Description("Minimum amount of seconds to wait before starting a raid.")]
        public int TimeToWait { get; set; } = 90;

        [Category(Hosting), Description("Users NIDs here are banned raiders.")]
        public RemoteControlAccessList RaiderBanList { get; set; } = new() { AllowIfEmpty = false };

        [Category(FeatureToggle), Description("Set your Switch Date/Time format in the Date/Time settings. The day will automatically rollback by 1 if the Date changes.")]
        public DTFormat DateTimeFormat { get; set; } = DTFormat.MMDDYY;

        [Category(Hosting), Description("Time to scroll down duration in milliseconds for accessing date/time settings during rollover correction. You want to have it overshoot the Date/Time setting by 1, as it will click DUP after scrolling down. [Default: 930ms]")]
        public int HoldTimeForRollover { get; set; } = 930;

        [Category(FeatureToggle), Description("If true, start the bot when you are on the HOME screen with the game closed. The bot will only run the rollover routine so you can try to configure accurate timing.")]
        public bool ConfigureRolloverCorrection { get; set; } = false;

        [Category(FeatureToggle), Description("If true, the bot will attempt take screenshots for the Raid Embeds. If you experience crashes often about \"Size/Parameter\" try setting this to false.")]
        public bool TakeScreenshot { get; set; } = true;

        [Category(Hosting), Description("Enter Discord channel ID(s) to post raid embeds to. Feature has to be initialized via \"$resv\" after every client restart.")]
        public string RaidEmbedChannelsSV { get; set; } = string.Empty;

        [Category(FeatureToggle), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }


        private int _completedRaids;

        [Category(Counts), Description("Raids Started")]
        public int CompletedRaids
        {
            get => _completedRaids;
            set => _completedRaids = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedRaids() => Interlocked.Increment(ref _completedRaids);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedRaids != 0)
                yield return $"Started Raids: {CompletedRaids}";
        }

        public enum DTFormat
        { 
            MMDDYY,
            DDMMYY,
            YYMMDD,
        }
    }
}