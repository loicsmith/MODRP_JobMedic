using Life;
using Life.CharacterSystem;
using Life.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MODRP_JobMedic.Functions
{
    [System.Serializable]
    public class Disease
    {
        public string Name;
        public float BaseProbability;
        public int DamagePerMinute;

        public Disease(string name, float probability, int damage)
        {
            Name = name;
            BaseProbability = probability;
            DamagePerMinute = damage;
        }
    }
    internal class SickManager
    {
        public ModKit.ModKit Context { get; set; }

        private static SickManager _instance = null;
        public static SickManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SickManager();
                }
                return _instance;
            }
        }

        private List<Disease> Diseases;
        private Dictionary<int, Coroutine> activeCoroutines = new Dictionary<int, Coroutine>();
        MethodInfo RpcBlood;

        public void InitDiseases()
        {
            Diseases = new List<Disease>
            {
                new Disease("Rhume", 5f, 5),
                new Disease("Grippe", 2.5f, 7),
            };

            RpcBlood = typeof(ToolsSystem).GetMethod("RpcBlood", BindingFlags.NonPublic | BindingFlags.Instance);

        }

        public void CheckDiseaseOnConnection(Player player)
        {
            if (!activeCoroutines.ContainsKey(player.character.Id))
            {
                Coroutine coroutine = Nova.man.StartCoroutine(ApplyCoroutine(player));
                activeCoroutines[player.character.Id] = coroutine;
            }
        }

        public void NL_DisableSickness(Player player)
        {
            player.currentSickness.probabilityPerMinute = 0;
        }

        public IEnumerator DiseaseCheck(Player player)
        {
            while (true)
            {
                yield return new WaitForSeconds(300f);
                CheckForDiseases(player);
            }
        }

        public async void CheckForDiseases(Player player)
        {
            string time = EnviroSkyMgr.instance.GetTimeString();
            bool IsNight = EnviroSkyMgr.instance.Time.Hours >= 18 || EnviroSkyMgr.instance.Time.Hours <= 8;
            float ProbabilityMultiplier = 1f;

            bool IsInGlobalZone = player.setup.NetworkareaId == 0;

            ProbabilityMultiplier = IsInGlobalZone ? (IsNight ? 2f : 1.5f) : 1f;

            var IsSick = await OrmManager.JobMedic_SicknessManager.Query(a => a.PlayerCharacterId == player.character.Id);

            if (IsSick.Count != 0)
            {
                foreach (var Sick in IsSick)
                {
                    Console.WriteLine($"{player.GetFullName()} souffre toujours de {Sick.SickName}");

                    if (!activeCoroutines.ContainsKey(player.character.Id))
                    {
                        Coroutine coroutine = Nova.man.StartCoroutine(ApplyCoroutine(player));
                        activeCoroutines[player.character.Id] = coroutine;
                    }
                }
            }
            else
            {
                foreach (var disease in Diseases)
                {
                    float ProbaDisease = disease.BaseProbability * ProbabilityMultiplier;
                    float ProbaRandom = UnityEngine.Random.Range(0f, 100f);

                    Console.WriteLine(ProbaRandom + " --- " + ProbaDisease);
                    if (ProbaRandom <= ProbaDisease)
                    {
                        Console.WriteLine($"{player.GetFullName()} vient de contracter {disease.Name}");
                        player.Notify("Malade", $"Vous venez de tomber malade, vous avez probablement contracté un(e) {disease.Name}");

                        var SickData = new OrmManager.JobMedic_SicknessManager { PlayerCharacterId = player.character.Id, SickName = disease.Name };
                        await SickData.Save();

                        if (!activeCoroutines.ContainsKey(player.character.Id))
                        {
                            Coroutine coroutine = Nova.man.StartCoroutine(ApplyCoroutine(player));
                            activeCoroutines[player.character.Id] = coroutine;
                        }

                        break;
                    }
                }
            }
        }

        public IEnumerator ApplyCoroutine(Player player)
        {
            while (true)
            {
                yield return new WaitForSeconds(60f);
                ApplySickToPlayer(player);
            }
        }

        public async void ApplySickToPlayer(Player player)
        {
            var activeDiseases = await OrmManager.JobMedic_SicknessManager.Query(a => a.PlayerCharacterId == player.character.Id);

            if (activeDiseases.Count != 0)
            {
                foreach (var disease in activeDiseases)
                {
                    player.Notify("Maladie", $"Vous êtes terriblement malade ! Vous avez probablement un(e) {disease.SickName}. Consultez au plus vite un médecin !", NotificationManager.Type.Warning, 10f);

                    var currentDisease = Diseases.FirstOrDefault(d => d.Name == disease.SickName);
                    if (currentDisease != null)
                    {
                        player.setup.Networkhealth -= currentDisease.DamagePerMinute;

                        Vector3 bloodPosition = player.setup.interaction.head.position;
                        Vector3 bloodDirection = Vector3.down;
                        RpcBlood.Invoke(player.setup.toolsSystem, new object[] { bloodPosition, bloodDirection });
                        RpcBlood.Invoke(player.setup.toolsSystem, new object[] { bloodPosition, bloodDirection });
                    }
                }
            }
        }

        public async void CureDisease(Player player, Player SecondPlayer, string diseaseName)
        {
            var activeDiseases = await OrmManager.JobMedic_SicknessManager.Query(a => a.PlayerCharacterId == SecondPlayer.character.Id && a.SickName == diseaseName);

            if (activeDiseases.Count != 0)
            {
                foreach (var disease in activeDiseases)
                {
                    await disease.Delete();

                    if (activeCoroutines.ContainsKey(SecondPlayer.character.Id))
                    {
                        Nova.man.StopCoroutine(activeCoroutines[SecondPlayer.character.Id]);
                        activeCoroutines.Remove(SecondPlayer.character.Id);
                    }

                    SecondPlayer.Notify("Soins réussis", $"Vous avez été soigné(e) de votre {disease.SickName}. Vous vous sentez beaucoup mieux maintenant !", NotificationManager.Type.Success);
                    player.Notify("Soins réussis", $"Vous avez soigné(e) un(e) {disease.SickName} à {SecondPlayer.GetFullName()} !", NotificationManager.Type.Success);

                }
            }
            else
            {
                player.Notify("Aucune maladie trouvée", "Votre patient n'est pas atteint(e) d'une maladie.", NotificationManager.Type.Info);
            }
        }
    }
}
