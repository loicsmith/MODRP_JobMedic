using Life;
using Life.BizSystem;
using Life.CharacterSystem;
using Life.FarmSystem;
using Life.Network;
using Life.Network.Systems;
using Life.UI;
using Mirror;
using ModKit.Helper;
using ModKit.Interfaces;
using ModKit.Internal;
using ModKit.ORM;
using Newtonsoft.Json;
using SQLite;
using System.Collections.Generic;
using System.IO;
using _menu = AAMenu.Menu;
using System.Collections;
using UnityEngine;
using MODRP_JobMedic.Functions;
using Life.DB;
using System;
using System.Configuration;
using System.Reflection;
using MODRP_JobMedic.Classes;

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

        public void InitAAmenu()
        {
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
