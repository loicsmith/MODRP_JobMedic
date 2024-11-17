using Life;
using Life.Network;

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
    }
}
