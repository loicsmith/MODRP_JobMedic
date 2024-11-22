using Life;
using Life.BizSystem;
using Life.CheckpointSystem;
using Life.Network;
using System.Collections.Generic;
using UnityEngine;

namespace MODRP_JobMedic.Functions
{
    internal class CureManager
    {
        public ModKit.ModKit Context { get; set; }

        public async void CureAnalysis(Player player)
        {
            Player SecondPlayer = player.GetClosestPlayer();

            if (SecondPlayer != null)
            {
                var activeDiseases = await OrmManager.JobMedic_SicknessManager.Query(a => a.PlayerCharacterId == SecondPlayer.character.Id);
                if (activeDiseases.Count != 0)
                {
                    foreach (var disease in activeDiseases)
                    {
                        player.Notify("Analyse médicale", $"Votre patient est atteint(e) d'un(e) {disease.SickName}", NotificationManager.Type.Info);
                    }
                }
                else
                {
                    player.Notify("Aucune maladie trouvée", "Votre patient n'est pas atteint(e) d'une maladie.", NotificationManager.Type.Info);
                }
            }
            else
            {
                player.Notify("Erreur", "Il n'y a personne à proximiter !", NotificationManager.Type.Error);
            }
        }

        public async void CureDisease(Player player)
        {
            Player SecondPlayer = player.GetClosestPlayer();

            if (SecondPlayer != null)
            {
                var activeDiseases = await OrmManager.JobMedic_SicknessManager.Query(a => a.PlayerCharacterId == SecondPlayer.character.Id);
                if (activeDiseases.Count != 0)
                {
                    foreach (var disease in activeDiseases)
                    {
                        SickManager.Instance.CureDisease(player, SecondPlayer, disease.SickName);
                    }
                }
                else
                {
                    player.Notify("Aucune maladie trouvée", "Votre patient n'est pas atteint(e) d'une maladie.", NotificationManager.Type.Info);
                }
            }
            else
            {
                player.Notify("Erreur", "Il n'y a personne à proximiter !", NotificationManager.Type.Error);
            }
        }

        public int CountMedicOnline()
        {
            int MedicPlayerOnline = 0;
            foreach (Player p in Nova.server.GetAllInGamePlayers())
            {
                if (p.HasBiz() && p.serviceMetier)
                {
                    if (Nova.biz.GetBizActivities(p.character.BizId) == new List<Activity.Type> { Activity.Type.Medical })
                    {
                        MedicPlayerOnline += 1;
                    }
                }
            }
            return MedicPlayerOnline;
        }

        public void CureDiseaseCheckpoint(Player player)
        {
            NCheckpoint CurePoint = new NCheckpoint(player.netId, new Vector3(Main.Main._JobMedicConfig.PosX, Main.Main._JobMedicConfig.PosY, Main.Main._JobMedicConfig.PosZ), (checkpoint) =>
            {
                int MedicPlayer = CountMedicOnline();

                if (MedicPlayer > 0)
                {
                    SickManager.Instance.CureDiseaseCheckpointAction(player);
                }
                else
                {
                    player.Notify("Soins", "Des médecins sont présents en ville, vous ne pouvez donc pas vous soignez !", NotificationManager.Type.Error);
                }
            });
            player.CreateCheckpoint(CurePoint);
        }
    }
}
