﻿using Life;
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

namespace MODRP_JobMedic.Main
{

    class Main : ModKit.ModKit
    {
        public SickManager sickManager = new SickManager();
        public CureManager cureManager = new CureManager();

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

            Console.WriteLine(EnviroSkyMgr.instance.Time.cycleLengthInMinutes);
            Console.WriteLine(EnviroSkyMgr.instance.Time.dayNightSwitch);

            

        }

        public void InitAAmenu()
        {
            _menu.AddBizTabLine(PluginInformations, new List<Activity.Type> { Activity.Type.Medical }, null, "Examiner symptômes", (ui) =>
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
        }
    }
}
