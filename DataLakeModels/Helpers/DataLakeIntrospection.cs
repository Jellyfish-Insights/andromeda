using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataLakeModels.Helpers {

    public class DataLakeIntrospection {

        private static IEnumerable<IEntityType> GetAllEntitiesInContext<T>() where T : DbContext, new() {
            using (var context = new T()) {
                return context.Model.GetEntityTypes().ToList();
            }
        }

        public static IEnumerable<IEntityType> GetAllEntities() {
            return GetAllEntitiesInContext<DataLakeAdWordsContext>().Union(
                GetAllEntitiesInContext<DataLakeYouTubeDataContext>().Union(
                    GetAllEntitiesInContext<DataLakeYouTubeAnalyticsContext>()
                    )
                );
        }

        public static IEnumerable<IEntityType> GetIValidityRangeEntities() {
            return GetAllEntities().Where(x => x.FindProperty("ValidityStart") != null && x.FindProperty("ValidityEnd") != null);
        }
    }
}
