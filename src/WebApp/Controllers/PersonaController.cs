using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationModels;
using ApplicationModels.Models.AccountViewModels.Constants;
using ApplicationModels.Models.DataViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebApp.Controllers {
    [Route("api/Persona")]
    [Authorize(Roles = Contansts.Permissions.ReadOnly)]
    public class PersonaController : Controller {
        static protected IDataBackend Backend;
        static PersonaController() {
            if (Constants.IsMock())
                Backend = new MockMarketingDataBackend();
            else
                Backend = new DBMarketingDataBackend();
        }

        [HttpGet("[action]")]
        public List<SourceObject> GetUnAssociatedAdSets() {
            return Backend.UnAssociatedSources(SourceObjectType.AdSet);
        }

        [HttpGet("[action]/{mode?}")]
        public List<PersonaVersion> GetPersonas(ArchiveMode mode = ArchiveMode.UnArchived) {
            return Backend.PersonaVersionList(mode);
        }

        [HttpPut("[action]")]
        [Authorize(Roles = Contansts.Permissions.Editor)]
        public PersonaVersionEdits EditPersonas([FromBody] PersonaVersionEdits edits) {
            return Backend.EditPersonas(edits);
        }
    }
}
