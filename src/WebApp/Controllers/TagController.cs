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
    [Route("api/Tag")]
    [Authorize(Roles = Contansts.Permissions.ReadOnly)]
    public class TagController : Controller {
        static protected IDataBackend Backend;
        static TagController() {
            if (Constants.IsMock())
                Backend = new MockMarketingDataBackend();
            else
                Backend = new DBMarketingDataBackend();
        }

        [HttpGet("[action]")]
        public Dictionary<string, Dictionary<string, Tag>> GetMetaTags() {
            return Backend.MetaTagsList().ToDictionary(x => x.Key, x => x.Value.ToDictionary(y => y.Value, y => y));
        }

        [HttpPut("[action]")]
        [Authorize(Roles = Contansts.Permissions.Editor)]
        public TagEdits EditMetaTags([FromBody] TagEdits edits) {
            return Backend.EditTags(edits);
        }
    }
}
