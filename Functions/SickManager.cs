using Life;
using Life.Network;
using System;
using System.Collections;
using System.Collections.Generic;
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

        private void InitDiseases()
        {
            Diseases = new List<Disease>
        {
            new Disease("Rhume", 0.1f, 2f),
            new Disease("Grippe", 0.05f, 5f),
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

        public void CheckForDiseases(Player player)
        {
            string time = EnviroSkyMgr.instance.GetTimeString();
            bool IsNight = EnviroSkyMgr.instance.Time.Hours >= 18 || EnviroSkyMgr.instance.Time.Hours <= 8;
            float ProbabilityMultiplier = 1f;

            bool IsInGlobalZone = player.setup.NetworkareaId == 0;


            ProbabilityMultiplier = IsInGlobalZone ? (IsNight ? 2f : 1.5f) : 1f;

            Console.WriteLine("Temps en jeu : " + time);
            Console.WriteLine("Il est fait nuit ? " + IsNight);
            Console.WriteLine("Proba : " + ProbabilityMultiplier);
            Console.WriteLine("Is dehors : " + IsInGlobalZone);
             
             var IsSick = OrmManager.JobMedic_SicknessManager.Query(a => a.PlayerCharacterId == player.character.Id).Result;

             foreach (var disease in Diseases)
             {
                 if (IsSick.Count != 0)
                 {
                     switch (disease.Name)
                     {
                         case "Rhume":
                             break;
                         case "Grippe":
                             break;
                     }
                 }
                 else
                 {
                     switch (disease.Name)
                     {
                         case "Rhume":

                             float ProbaRhume = disease.BaseProbability * ProbabilityMultiplier;
                             if (UnityEngine.Random.Range(0f, 100f) <= ProbaRhume)
                             {
                                 Console.WriteLine($"{player.GetFullName()} vient de contracter {disease.Name}");
                             }
                             break;
                         case "Grippe":

                             float ProbaGrippe = disease.BaseProbability * ProbabilityMultiplier;
                             if (UnityEngine.Random.Range(0f, 100f) <= ProbaGrippe)
                             {
                                 Console.WriteLine($"{player.GetFullName()} vient de contracter {disease.Name}");
                             }
                             break;
                     }
                 }
             }
        }

    }
}
