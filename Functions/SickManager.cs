using Life.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MODRP_JobMedic.Functions
{
    [System.Serializable]
    public class Disease
    {
        public string Name;
        public float BaseProbability;
        public float DamagePerMinute;

        public Disease(string name, float probability, float damage)
        {
            Name = name;
            BaseProbability = probability;
            DamagePerMinute = damage;
        }
    }

    internal class SickManager
    {
        public ModKit.ModKit Context { get; set; }

        private List<Disease> Diseases;

        public void InitDiseases()
        {
            Diseases = new List<Disease>
        {
            new Disease("Rhume", 10f, 2f),
            new Disease("Grippe", 5f, 5f),
        };
        }

        public void NL_DisableSickness(Player player)
        {
            player.currentSickness.probabilityPerMinute = 0;
        }

        public IEnumerator DiseaseCheck(Player player)
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
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

            Console.WriteLine("Temps en jeu : " + time);
            Console.WriteLine("Il est fait nuit ? " + IsNight);
            Console.WriteLine("Proba : " + ProbabilityMultiplier);
            Console.WriteLine("Est dehors : " + IsInGlobalZone);
            Console.WriteLine("___________");

            var IsSick = await OrmManager.JobMedic_SicknessManager.Query(a => a.PlayerCharacterId == player.character.Id);

            if (IsSick.Count != 0)
            {
                foreach (var Sick in IsSick)
                {
                    Console.WriteLine($"{player.GetFullName()} souffre toujours de {Sick.SickName}");
                }
            }

            else
            {
                foreach (var disease in Diseases)
                {
                    float ProbaDisease = disease.BaseProbability * ProbabilityMultiplier;
                    float ProbaRandom = UnityEngine.Random.Range(0f, 100f);

                    Console.WriteLine(ProbaRandom + " --- " + ProbaDisease);
                    if ( ProbaRandom <= ProbaDisease)
                    {
                        Console.WriteLine($"{player.GetFullName()} vient de contracter {disease.Name}");

                        var SickData = new OrmManager.JobMedic_SicknessManager { PlayerCharacterId = player.character.Id, SickName = disease.Name };
                        await SickData.Save();

                        break;
                    }
                }
            }
        }

    }
}
