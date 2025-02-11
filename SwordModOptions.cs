using BepInEx.Logging;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace SwordMod;

public class SwordModOptions : OptionInterface
{
    private readonly ManualLogSource Logger;

    public SwordModOptions(SwordMod modInstance, ManualLogSource loggerSource)
    {
        Logger = loggerSource;

        AllowParries = config.Bind<bool>("AllowParries", true);
        AllowDualWielding = config.Bind<bool>("AllowDualWielding", true);
        SwingDamageModifier = config.Bind<float>("SwingDamageModifier", 0.5f, new ConfigAcceptableRange<float>(0f, 10f));
        KnockbackModifier = config.Bind<float>("KnockbackModifier", 1f, new ConfigAcceptableRange<float>(0f, 10f));
        VerticalKnockbackModifier = config.Bind<float>("VerticalKnockbackModifier", 1f, new ConfigAcceptableRange<float>(0f, 10f));
        StunModifier = config.Bind<float>("StunModifier", 1f, new ConfigAcceptableRange<float>(-10f, 10f));
        LungeModifier = config.Bind<float>("LungeModifier", 1f, new ConfigAcceptableRange<float>(0f, 10f));
        SwingTime = config.Bind<int>("SwingTime", 17, new ConfigAcceptableRange<int>(1, 80));
        ParryWindow = config.Bind<int>("ParryWindow", 3, new ConfigAcceptableRange<int>(0, 40));
        ExhaustsGourmand = config.Bind<bool>("ExhaustsGourmand", true);

        SpawnAt5P = config.Bind<bool>("SpawnAt5P", false);
        SpawnAtMoon = config.Bind<bool>("SpawnAtMoon", false);
        RemoveMoon = config.Bind<bool>("RemoveMoon", false);
        SpawnEveryCycle = config.Bind<bool>("SpawnEveryCycle", false);
        RandomArenaSwords = config.Bind<int>("RandomArenaSwords", 0, new ConfigAcceptableRange<int>(0, 20));
        ScavSpawnChance = config.Bind<float>("ScavSpawnChance", 0f, new ConfigAcceptableRange<float>(0f, 1f));

        OmnidirectionalSwings = config.Bind<bool>("OmnidirectionalSwings", false);
        HorizontalPushbackModifier = config.Bind<float>("HorizontalPushbackModifier", 0f, new ConfigAcceptableRange<float>(0f, 10f));
        VerticalPushbackModifier = config.Bind<float>("VerticalPushbackModifier", 0f, new ConfigAcceptableRange<float>(0f, 10f));
        DownswingLengthModifier = config.Bind<float>("VerticalLengthModifier", 1f, new ConfigAcceptableRange<float>(0f, 10f));
        UseStaminaMechanics = config.Bind<bool>("UseStaminaMechanics", false);
        ExhaustionRateModifier = config.Bind<float>("ExhaustionRateModifier", 1.0f, new ConfigAcceptableRange<float>(0f, 10f));
        ExhaustionDamageModifier = config.Bind<float>("ExhaustionDamageModifier", 0.2f, new ConfigAcceptableRange<float>(0f, 2f));

    }

    public Configurable<bool> AllowParries;
    public Configurable<bool> AllowDualWielding;
    public Configurable<float> SwingDamageModifier;
    public Configurable<float> KnockbackModifier;
    public Configurable<float> VerticalKnockbackModifier;
    public Configurable<float> StunModifier;
    public Configurable<float> LungeModifier;
    public Configurable<int> SwingTime;
    public Configurable<int> ParryWindow;
    public Configurable<bool> ExhaustsGourmand;

    public Configurable<bool> SpawnAt5P;
    public Configurable<bool> SpawnAtMoon;
    public Configurable<bool> RemoveMoon;
    public Configurable<bool> SpawnEveryCycle;
    public Configurable<int> RandomArenaSwords;
    public Configurable<float> ScavSpawnChance;
    //replace spear chance?

    //extras
    public Configurable<bool> OmnidirectionalSwings;
    public Configurable<float> HorizontalPushbackModifier;
    public Configurable<float> VerticalPushbackModifier;
    public Configurable<float> DownswingLengthModifier;
    public Configurable<bool> UseStaminaMechanics;
    public Configurable<float> ExhaustionRateModifier;
    public Configurable<float> ExhaustionDamageModifier;

    private UIelement[] UIArrOptions;
    private UIelement[] UIArrExtras;


    public override void Initialize()
    {
        var generalTab = new OpTab(this, "Options");
        var extrasTab = new OpTab(this, "Extras");
        this.Tabs = new[]
        {
            generalTab,
            extrasTab
        };

        //General Options
        float h = 550f, g = -30f, w = 10f, d = 100f;
        UIArrOptions = new UIelement[]
        {
            //General Options
            new OpLabel(w, h, "Sword Mechanics", true),
            new OpCheckBox(AllowParries, w, h+=g){description="Allows swords to parry when striking each other."}, new OpLabel(d, h, "Allow Parries"),
            new OpCheckBox(AllowDualWielding, w, h+=g){description="Allows players to hold two swords simultaneously: an unintended, overpowered, but hilarious mechanic."}, new OpLabel(d, h, "Allow Dual-Wielding"),
            new OpUpdown(SwingDamageModifier, new Vector2(w, h+=g), 80f, 1){description="Defines how much damage the sword deals when swung by Survivor."}, new OpLabel(d, h, "Damage Multiplier"),
            new OpUpdown(KnockbackModifier, new Vector2(w, h+=g), 80f, 1){description="Multiplies the amount of knockback imparted upon creatures struct with the sword."}, new OpLabel(d, h, "Knockback Multiplier"),
            new OpUpdown(VerticalKnockbackModifier, new Vector2(w, h+=g), 80f, 1){description="Further multiplies the upward knockback from upswings. This was added solely to make the opening scene of the trailer."}, new OpLabel(d, h, "Vertical Knockback Multiplier"),
            new OpUpdown(StunModifier, new Vector2(w, h+=g), 80f, 1){description="Affects the stun time of creatures hit by the sword. Higher values = more stun; lower values = less stun."}, new OpLabel(d, h, "Stun Multiplier"),
            new OpUpdown(LungeModifier, new Vector2(w, h+=g), 80f, 1){description="Multiplies the velocity added to an airborne player when swinging the sword."}, new OpLabel(d, h, "Lunge Multiplier"),
            new OpUpdown(true, SwingTime, new Vector2(w, h+=g), 80f){description="How long the sword swings in ticks (~1/40th of a second)."}, new OpLabel(d, h, "/40s  Swing Time"),
            new OpUpdown(true, ParryWindow, new Vector2(w, h+=g), 80f){description="The delay before the sword can damage creatures in ticks (~1/40th of a second). Gives a brief amount of time to parry another player's attack."}, new OpLabel(d, h, "/40s  Parry Window"),
            new OpCheckBox(ExhaustsGourmand, w, h+=g){description="Causes Gourmand to be instantly fully exhausted after two consecutive sword swings."}, new OpLabel(d, h, "Exhausts Gourmand"),
            //Presets
            new OpLabel(w, h+=g*2, "Spawning Settings", true),
            new OpCheckBox(SpawnAt5P, w, h+=g){description="Gives Five Pebbles a sword (makes the sword accessible for Artificer)."}, new OpLabel(d, h, "Spawn at 5P"),
            new OpCheckBox(SpawnAtMoon, w, h+=g){description="Spawns a sword besides Looks to the Moon each cycle (the intended way to find the sword)."}, new OpLabel(d, h, "Spawn at Moon"),
            new OpCheckBox(RemoveMoon, w, h+=g){description="Makes Looks to the Moon unconscious and shoves her inside the wall."}, new OpLabel(d, h, "Remove Moon"),
            new OpCheckBox(SpawnEveryCycle, w, h+=g){description="Spawns a sword beside each player at the start of each cycle (the easiest way to get a sword immediately)."}, new OpLabel(d, h, "Spawn Every Cycle"),
            new OpUpdown(true, RandomArenaSwords, new Vector2(w, h+=g), 80f){description="The number of swords to spawn randomly around each arena (note: some swords may spawn inside the geometry and thus be inaccessable)."}, new OpLabel(d, h, "Random Arena Swords"),
            new OpUpdown(ScavSpawnChance, new Vector2(w, h+=g), 80f, 2){description="The chance that a scavenger will spawn with one sword. Multiplied by 4 for elite scavengers."}, new OpLabel(d, h, "Scav Spawn Chance")
        };

        generalTab.AddItems(UIArrOptions);

        h = 550f;
        UIArrExtras = new UIelement[]
        {
            new OpLabel(w, h, "Extra Mechanics", true),
            new OpCheckBox(OmnidirectionalSwings, w, h+=g){description="Allows swords to be freely swung straight up or down."}, new OpLabel(d, h, "Omnidirectional Swings"),
            new OpUpdown(HorizontalPushbackModifier, new Vector2(w, h+=g), 80f, 1){description="Multiplies the amount that players will be pushed backward upon striking a creature with a sword."}, new OpLabel(d, h, "Horizontal Pushback"),
            new OpUpdown(VerticalPushbackModifier, new Vector2(w, h+=g), 80f, 1){description="Multiplies the amount that players will be pushed upward when striking a creature with a sword (designed to allow for pogos if set over 1)."}, new OpLabel(d, h, "Vertical Pushback (set above 1 for pogos)"),
            new OpUpdown(DownswingLengthModifier, new Vector2(w, h+=g), 80f, 1){description="Multiplies the length of the sword when being swung directly downwards. Makes pogos or jump attacks easier to land."}, new OpLabel(d, h, "Downswing Length Modifier"),
            new OpCheckBox(UseStaminaMechanics, w, h+=g){description="Causes the sword's swing to be weaker if used repeatedly, even causing exhaustion if overused. Designed to make PvP more strategic."}, new OpLabel(d, h, "Use Stamina Mechanics"),
            new OpUpdown(ExhaustionRateModifier, new Vector2(w, h+=g), 80f, 1){description="Multiplies the rate at which sword swings will exhaust the player. 9.0 or above should instantly exhaust the player."}, new OpLabel(d, h, "Exhaustion Rate Modifier"),
            new OpUpdown(ExhaustionDamageModifier, new Vector2(w, h+=g), 80f, 1){description="Multiplies the swords damage when the slugcat is exhausted. The modifier will decrease from 1 to this value as the slugcat gets more tired."}, new OpLabel(d, h, "Exhausted Damage Modifier")
        };

        extrasTab.AddItems(UIArrExtras);

    }

}