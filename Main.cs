using Life;
using Life.BizSystem;
using Life.CheckpointSystem;
using Life.DB;
using Life.Network;
using Life.UI;
using Mirror;
using ModKit.Helper;
using ModKit.Interfaces;
using ModKit.ORM;
using MODRP_JobMedic.Classes;
using MODRP_JobMedic.Functions;
using Newtonsoft.Json;
using SQLite;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using _menu = AAMenu.Menu;

namespace MODRP_JobMedic.Main
{

    class Main : ModKit.ModKit
    {
        public SickManager sickManager = new SickManager();
        public CureManager cureManager = new CureManager();

        public static string ConfigDirectoryPath;
        public static string ConfigJobMedicPath;
        public static JobMedicConfig _JobMedicConfig;

        public Main(IGameAPI api) : base(api)
        {
            PluginInformations = new PluginInformations(AssemblyHelper.GetName(), "1.0.0", "Loicsmith");

            sickManager.Context = this;
            cureManager.Context = this;
        }



        public override void OnPluginInit()
        {
            base.OnPluginInit();
            ModKit.Internal.Logger.LogSuccess($"{PluginInformations.SourceName} v{PluginInformations.Version}", "initialisé");

            Orm.RegisterTable<OrmManager.JobMedic_SicknessManager>();

            InitAAmenu();

            sickManager.InitDiseases();

            InitConfig();
            _JobMedicConfig = LoadConfigFile(ConfigJobMedicPath);

        }

        private void InitConfig()
        {
            try
            {
                ConfigDirectoryPath = DirectoryPath + "/JobMedic";
                ConfigJobMedicPath = Path.Combine(ConfigDirectoryPath, "JobMedicConfig.json");

                if (!Directory.Exists(ConfigDirectoryPath)) Directory.CreateDirectory(ConfigDirectoryPath);
                if (!File.Exists(ConfigJobMedicPath)) InitJobMedicConfig();
            }
            catch (IOException ex)
            {
                ModKit.Internal.Logger.LogError("InitDirectory", ex.Message);
            }
        }

        private void InitJobMedicConfig()
        {
            JobMedicConfig JobMedicConfig = new JobMedicConfig();
            string json = JsonConvert.SerializeObject(JobMedicConfig, Formatting.Indented);
            File.WriteAllText(ConfigJobMedicPath, json);
        }

        private JobMedicConfig LoadConfigFile(string path)
        {
            if (File.Exists(path))
            {
                string jsonContent = File.ReadAllText(path);
                JobMedicConfig JobMedicConfig = JsonConvert.DeserializeObject<JobMedicConfig>(jsonContent);

                return JobMedicConfig;
            }
            else return null;
        }

        private void SaveConfig(string path)
        {
            string json = JsonConvert.SerializeObject(_JobMedicConfig, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public void ConfigEditor(Player player)
        {
            Panel panel = PanelHelper.Create("JobMedic | Config JSON", UIPanel.PanelType.TabPrice, player, () => ConfigEditor(player));

            panel.AddTabLine($"{TextFormattingHelper.Color($"Position Point ${TextFormattingHelper.Color($"X : {_JobMedicConfig.PosX}, Y : {_JobMedicConfig.PosY}, Z : {_JobMedicConfig.PosZ}", TextFormattingHelper.Colors.Verbose)} :\n{TextFormattingHelper.Color(TextFormattingHelper.Size(TextFormattingHelper.LineHeight("Placement sur votre position lors de la sélection", 15), 15), TextFormattingHelper.Colors.Purple)}", TextFormattingHelper.Colors.Info)}", _ =>
            {
                _JobMedicConfig.PosX = player.setup.transform.position.x;
                _JobMedicConfig.PosY = player.setup.transform.position.y;
                _JobMedicConfig.PosZ = player.setup.transform.position.z;
            });
            panel.AddTabLine($"{TextFormattingHelper.Color("Prix Soins Maladie : ", TextFormattingHelper.Colors.Info)}" + $"{TextFormattingHelper.Color($"{_JobMedicConfig.PriceCure}", TextFormattingHelper.Colors.Verbose)}", _ =>
            {
                EditLineInConfig(player, "PriceCure");
            });
            panel.AddTabLine($"{TextFormattingHelper.Color("Appliquer la configuration", TextFormattingHelper.Colors.Success)}", _ =>
            {
                SaveConfig(ConfigJobMedicPath);
                panel.Refresh();
            });

            panel.NextButton("Sélectionner", () => panel.SelectTab());
            panel.AddButton("Retour", _ => AAMenu.AAMenu.menu.AdminPluginPanel(player));
            panel.CloseButton();
            panel.Display();
        }

        public void EditLineInConfig(Player player, string Param)
        {
            Panel panel = PanelHelper.Create("JobMedic | Edit JSON", UIPanel.PanelType.Input, player, () => EditLineInConfig(player, Param));
            panel.TextLines.Add($"Modification de la valeur de : \"{Param}\"");
            panel.SetInputPlaceholder("Veuillez saisir une valeur");
            panel.AddButton("Valider", (ui) =>
            {
                string input = ui.inputText;

                switch (Param)
                {
                    case "PriceCure":
                        // float
                        if (float.TryParse(input, out float Price))
                        {
                            _JobMedicConfig.PriceCure = Price;
                        }
                        else
                        {
                            player.Notify("JobMedic", "Veuillez saisir un nombre entier.", NotificationManager.Type.Error);
                        }
                        break;
                }
                panel.Previous();
            });
            panel.PreviousButton();
            panel.CloseButton();
            panel.Display();
        }

        public void CheckEditCheckpoint()
        {
            foreach (Player p in Nova.server.GetAllInGamePlayers())
            {

                // PAS BON çAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
                /*Vector3 PositionPoint = new Vector3(_JobMedicConfig.PosX, _JobMedicConfig.PosX, _JobMedicConfig.PosX);
                foreach (NCheckpoint checkpoint in Nova.server.checkpoints)
                {
                    if (checkpoint.position == PositionPoint)

                        foreach (Player p2 in Nova.server.GetAllInGamePlayers())
                        {
                            p2.DestroyCheckpoint(checkpoint);
                        }
                }*/

                cureManager.CureDiseaseCheckpoint(p);
            }
        }

        public void InitAAmenu()
        {

            _menu.AddAdminPluginTabLine(PluginInformations, 0, "JobMedic", (ui) =>
            {
                Player player = PanelHelper.ReturnPlayerFromPanel(ui);
                ConfigEditor(player);
            });

            _menu.AddBizTabLine(PluginInformations, new List<Activity.Type> { Activity.Type.Medical }, null, "Examiner les symptômes", (ui) =>
            {
                Player player = PanelHelper.ReturnPlayerFromPanel(ui);
                cureManager.CureAnalysis(player);
            });
            _menu.AddBizTabLine(PluginInformations, new List<Activity.Type> { Activity.Type.Medical }, null, "Soigner une maladie", (ui) =>
            {
                Player player = PanelHelper.ReturnPlayerFromPanel(ui);
                cureManager.CureDisease(player);
            });
        }

        public override void OnPlayerSpawnCharacter(Player player, NetworkConnection conn, Characters character)
        {
            base.OnPlayerSpawnCharacter(player, conn, character);
            sickManager.NL_DisableSickness(player);

            Nova.man.StartCoroutine(sickManager.DiseaseCheck(player));

            sickManager.CheckDiseaseOnConnection(player);

            cureManager.CureDiseaseCheckpoint(player);
        }
    }
}
