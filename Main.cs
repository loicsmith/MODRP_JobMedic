using Life;
using Life.BizSystem;
using Life.Network;
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

namespace MODRP_JobMedic.Main
{

    class Main : ModKit.ModKit
    {


        public Main(IGameAPI api) : base(api)
        {
            PluginInformations = new PluginInformations(AssemblyHelper.GetName(), "1.0.0", "Loicsmith");
        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();
            Logger.LogSuccess($"{PluginInformations.SourceName} v{PluginInformations.Version}", "initialisé");

            InitAAmenu();

        }


       

        public void InitAAmenu()
        {
            
        }
    }
}
