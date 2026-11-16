#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameFramework.Procedure;
using Monster;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;
using Object = UnityEngine.Object;

// Temporary Editor integration check. Import into an Editor folder only while verifying.
[InitializeOnLoad]
public static class PokemonJsonSpawnVerification
{
    private const string ArmKey = "Monster.PokemonJsonSpawnVerify";
    private static readonly string OutputDirectory = Path.GetFullPath(
        Path.Combine(Application.dataPath, "../.utmp/pokemon-json-spawn-20261117"));

    private static VerificationReport sReport = new VerificationReport();
    private static EntityComponent sEntities;
    private static PokemonData sPartyPokemon;
    private static int[] sEntityIds;
    private static int sStage;
    private static int sSpawnFrame;
    private static double sStartedAt = -1;

    static PokemonJsonSpawnVerification()
    {
        if (SessionState.GetBool(ArmKey, false))
        {
            EditorApplication.update += Tick;
        }
    }

    public static void Arm()
    {
        SessionState.SetBool(ArmKey, true);
        sReport = new VerificationReport();
        sEntities = null;
        sPartyPokemon = null;
        sEntityIds = null;
        sStage = 0;
        sSpawnFrame = 0;
        sStartedAt = -1;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(ArmKey, false))
        {
            EditorApplication.update -= Tick;
            return;
        }

        // The callback can be installed before entering Play Mode or a domain reload.
        if (!EditorApplication.isPlaying)
        {
            return;
        }

        try
        {
            if (sStartedAt < 0)
            {
                sStartedAt = EditorApplication.timeSinceStartup;
                sReport.startedUtc = DateTime.UtcNow.ToString("o");
                WriteReport("waiting-for-main");
            }

            if (EditorApplication.timeSinceStartup - sStartedAt > 120)
            {
                throw new TimeoutException("Integration verification exceeded 120 seconds in Play Mode.");
            }

            ProcedureBase current = CurrentProcedure();
            sReport.procedure = current == null ? null : current.GetType().Name;

            if (sStage == 0)
            {
                MyProcedureIntro intro = current as MyProcedureIntro;
                if (intro != null)
                {
                    FieldInfo change = typeof(MyProcedureIntro).GetField(
                        "mChangeToMain", BindingFlags.Instance | BindingFlags.NonPublic);
                    Require(change != null, "Intro transition field was not found.");
                    change.SetValue(intro, true);
                    return;
                }

                RPGGame game = RPGGame.Instance;
                if (current is MyProcedureMain && game != null)
                {
                    if (Object.FindFirstObjectByType<Player>() == null ||
                        game.PokemonDatabase == null || !game.PokemonDatabase.mIsLoaded ||
                        game.PokemonPartyManager == null || game.PokemonPartyManager.Count == 0)
                    {
                        return;
                    }

                    Require(game.PokemonPartyManager.Count == 2, "Initial party must contain exactly two Pokemon.");
                    Require(game.PokemonDatabase.Count == 2, "JSON database must contain exactly two Pokemon.");
                    sPartyPokemon = game.PokemonPartyManager.GetPokemon(0);
                    Require(sPartyPokemon != null && sPartyPokemon.PokemonId == 25, "First party Pokemon must be Pikachu.");
                    sReport.databaseCount = game.PokemonDatabase.Count;
                    sReport.partyCount = game.PokemonPartyManager.Count;
                    game.RequestBattle();
                    sStage = 1;
                    WriteReport("waiting-for-battle-entities");
                    return;
                }

                // Also support arming an already running Battle session.
                if (current is MyProcedureBattle)
                {
                    sStage = 1;
                }
                else
                {
                    return;
                }
            }

            if (sStage == 1)
            {
                if (!(current is MyProcedureBattle))
                {
                    return;
                }

                BattlePokemon[] all = Object.FindObjectsByType<BattlePokemon>(FindObjectsSortMode.None)
                    .Where(p => p.Available).ToArray();
                Require(all.Length <= 2, "Duplicate active BattlePokemon entities were found.");
                if (all.Length != 2 || all.Any(p => p.PokemonData == null || p.PokemonData.Stats == null))
                {
                    return;
                }

                RPGGame game = RPGGame.Instance;
                Require(game != null && game.PokemonDatabase != null && game.PokemonDatabase.mIsLoaded,
                    "JSON database was not ready when Pokemon appeared.");
                Require(game.PokemonPartyManager != null && game.PokemonPartyManager.Count == 2,
                    "Party must contain exactly two Pokemon when entering Battle.");
                sReport.databaseCount = game.PokemonDatabase.Count;
                sReport.partyCount = game.PokemonPartyManager.Count;
                if (sPartyPokemon == null)
                {
                    sPartyPokemon = game.PokemonPartyManager.GetPokemon(0);
                }

                sEntities = GameEntry.GetComponent<EntityComponent>();
                Require(sEntities != null, "EntityComponent is missing.");
                Require(all.Select(p => p.PokemonData.Stats.PokemonId).OrderBy(id => id)
                    .SequenceEqual(new[] { 25, 133 }), "Spawned species must be Pikachu and Eevee.");

                foreach (BattlePokemon pokemon in all)
                {
                    PokemonData stats = pokemon.PokemonData.Stats;
                    int expectedHP = stats.PokemonId == 25 ? 22 : 25;
                    string expectedName = stats.PokemonId == 25 ? "피카츄" : "이브이";
                    bool registered = pokemon.Entity != null && sEntities.HasEntity(pokemon.Entity.Id) &&
                        ReferenceEquals(sEntities.GetEntity(pokemon.Entity.Id), pokemon.Entity);
                    bool underCanvas = pokemon.GetComponentInParent<Canvas>() != null;

                    Require(registered, "Pokemon was not registered in EntityComponent.");
                    Require(pokemon.Entity.EntityGroup != null && pokemon.Entity.EntityGroup.Name == "BattlePokemon",
                        "Pokemon belongs to the wrong Entity group.");
                    Require(!underCanvas, "Pokemon must not be parented under a UI Canvas.");
                    Require(stats.Name == expectedName && stats.Level == 5 &&
                        stats.MaxHP == expectedHP && stats.CurrentHP == expectedHP,
                        "Pokemon JSON name, level, or HP was not transferred correctly.");
                    Require(pokemon.PokemonData.TypeId == stats.PokemonId, "Entity species ID differs from Stats.");
                    Require(pokemon.PokemonData.IsEnemy == (stats.PokemonId == 133), "Enemy/player roles differ.");

                    if (stats.PokemonId == 25)
                    {
                        Require(ReferenceEquals(stats, sPartyPokemon) &&
                            ReferenceEquals(stats, game.PokemonPartyManager.GetPokemon(0)),
                            "Player Pokemon must retain the original party data instance.");
                        sReport.partyReferencePreserved = true;
                    }

                    SpriteRenderer renderer = pokemon.GetComponentInChildren<SpriteRenderer>(true);
                    Require(renderer != null && renderer.enabled && renderer.sprite != null,
                        "Pokemon SpriteRenderer is not ready.");

                    sReport.pokemon.Add(new PokemonSnapshot
                    {
                        entityId = pokemon.Entity.Id,
                        pokemonId = stats.PokemonId,
                        name = stats.Name,
                        level = stats.Level,
                        maxHP = stats.MaxHP,
                        currentHP = stats.CurrentHP,
                        assetName = pokemon.Entity.EntityAssetName,
                        group = pokemon.Entity.EntityGroup.Name,
                        isEnemy = pokemon.PokemonData.IsEnemy,
                        registered = registered,
                        underCanvas = underCanvas,
                        position = pokemon.transform.position.ToString()
                    });
                }

                sEntityIds = all.Select(p => p.Entity.Id).ToArray();
                sSpawnFrame = Time.frameCount;
                sStage = 2;
                WriteReport("spawned");
                ScreenCapture.CaptureScreenshot(Path.Combine(OutputDirectory, "spawn.png"));
                return;
            }

            if (sStage == 2)
            {
                if (Time.frameCount - sSpawnFrame < 12)
                {
                    return;
                }

                MyProcedureBattle battle = current as MyProcedureBattle;
                if (battle != null)
                {
                    battle.FinishBattle();
                }
                sStage = 3;
                WriteReport("waiting-for-cleanup");
                return;
            }

            if (sStage == 3)
            {
                if (!(current is MyProcedureMain) || Object.FindFirstObjectByType<Player>() == null)
                {
                    return;
                }

                if (sEntityIds.Any(id => sEntities.HasEntity(id) || sEntities.IsLoadingEntity(id)))
                {
                    return;
                }

                RPGGame game = RPGGame.Instance;
                Require(game != null && game.PokemonPartyManager.Count == 2, "Party changed after Battle cleanup.");
                Require(ReferenceEquals(sPartyPokemon, game.PokemonPartyManager.GetPokemon(0)),
                    "Party Pokemon data was replaced after Battle cleanup.");
                sReport.partyCount = game.PokemonPartyManager.Count;
                sReport.cleanupVerified = true;
                WriteReport("complete");
                Disarm();
            }
        }
        catch (Exception exception)
        {
            sReport.error = exception.ToString();
            WriteReport("failed");
            Disarm();
        }
    }

    private static ProcedureBase CurrentProcedure()
    {
        foreach (ProcedureComponent component in Object.FindObjectsByType<ProcedureComponent>(FindObjectsSortMode.None))
        {
            try
            {
                if (component.CurrentProcedure != null)
                {
                    return component.CurrentProcedure;
                }
            }
            catch (Exception)
            {
                // Ignore editor previews or a component still initializing.
            }
        }
        return null;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void WriteReport(string phase)
    {
        sReport.phase = phase;
        sReport.updatedUtc = DateTime.UtcNow.ToString("o");
        sReport.elapsedSeconds = sStartedAt < 0 ? 0 : EditorApplication.timeSinceStartup - sStartedAt;
        Directory.CreateDirectory(OutputDirectory);
        File.WriteAllText(Path.Combine(OutputDirectory, "integration-verification.json"), JsonUtility.ToJson(sReport, true));
    }

    private static void Disarm()
    {
        SessionState.SetBool(ArmKey, false);
        EditorApplication.update -= Tick;
    }

    [Serializable]
    private sealed class VerificationReport
    {
        public string phase;
        public string procedure;
        public string startedUtc;
        public string updatedUtc;
        public double elapsedSeconds;
        public int databaseCount;
        public int partyCount;
        public bool partyReferencePreserved;
        public bool cleanupVerified;
        public string error;
        public List<PokemonSnapshot> pokemon = new List<PokemonSnapshot>();
    }

    [Serializable]
    private sealed class PokemonSnapshot
    {
        public int entityId;
        public int pokemonId;
        public string name;
        public int level;
        public int maxHP;
        public int currentHP;
        public string assetName;
        public string group;
        public bool isEnemy;
        public bool registered;
        public bool underCanvas;
        public string position;
    }
}
#endif
