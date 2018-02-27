using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ApplicationModels.Helpers {

    public class ApplicationIntrospection {

        public static IEnumerable<IEntityType> GetAllEntities() {
            using (var context = new ApplicationDbContext()) {
                return context.Model.GetEntityTypes().ToList();
            }
        }
    }
}
