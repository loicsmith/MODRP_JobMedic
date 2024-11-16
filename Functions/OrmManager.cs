using ModKit.ORM;
using SQLite;

namespace MODRP_JobMedic.Functions
{
    internal class OrmManager
    {
        public class JobMedic_SicknessManager : ModEntity<JobMedic_SicknessManager>
        {
            [AutoIncrement][PrimaryKey] public int Id { get; set; }

            public int PlayerCharacterId { get; set; }
            public string SickName { get; set; }
        }
    }
}
