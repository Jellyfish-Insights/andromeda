using System.Collections.Generic;
using System.Linq;
using ApplicationModels.Models;
using ApplicationModels.Models.DataViewModels;
using Test.Helpers;
using WebApp.Controllers;
using Xunit;
using Common;

namespace Test {

    public class PersonaTest {

        private AnalyticsPlatformSteps APS;

        public PersonaTest() {
            DatabaseReset.Drop(Databases.AnalyticsPlatform);
            DatabaseReset.Migrate(Databases.AnalyticsPlatform);
            APS = new AnalyticsPlatformSteps();
        }

        [Fact]
        public void PersonasEditing() {

            var someCampaigns = new[] {
                (new SourceCampaign() {
                    Title = "Views_Christmas_23.12.2019", Platform = Constants.FacebookSource
                },
                 new[] {
                    (new SourceAdSet() { Title = "YEAR Liberals - Film bufs" }, new[] {
                        new SourceAd() { Title = "Views 10" },
                        new SourceAd() { Title = "Traffic 10" },
                    })
                }),
                (new SourceCampaign() {
                    Title = "Views_New Year_23.12.2019", Platform = Constants.FacebookSource
                }, new[] {
                    (new SourceAdSet() { Title = "YEAR Conservatives" },
                     new[] {
                        new SourceAd() { Title = "Views 11" },
                        new SourceAd() { Title = "Traffic 11" },
                    }),
                    (new SourceAdSet() { Title = "Fans of FEE" },
                     new[] {
                        new SourceAd() { Title = "Views 12" },
                        new SourceAd() { Title = "Traffic 12" },
                    }),
                })
            };

            APS.TheseCampaignsExist(someCampaigns);
            APS.ApApTransformationsHaveRun();

            var controller = new PersonaController();
            var personas = controller.GetPersonas();
            Assert.Equal(2, personas.Count());

            var adsets = controller.GetUnAssociatedAdSets();
            Assert.Single(adsets);
            var edits = new PersonaVersionEdits(){
                Edits = new Dictionary<string, PersonaVersionEdit>(){
                    { personas[0].Id, new PersonaVersionEdit(){
                          Flag = EditType.Update,
                          Archive = true,
                          UpdateDate = personas[0].UpdateDate,
                          AddedAdSets = new List<string>(){ adsets[0].SourceId }
                      }
                    },
                    { personas[1].Id, new PersonaVersionEdit(){
                          Flag = EditType.Update,
                          UpdateDate = personas[1].UpdateDate,
                          RemovedAdSets = personas[1].AdSets.Select(x => x.SourceId).ToList()
                      }
                    },
                }
            };
            var failedEdits = controller.EditPersonas(edits);
            Assert.Empty(failedEdits.Edits);
            // Check if only items removed from persona 1 are unassociated
            adsets = controller.GetUnAssociatedAdSets();
            Assert.Contains(adsets, x => personas[1].AdSets.Any(a => x.SourceId == x.SourceId));
            // Check Archived Persona
            personas = controller.GetPersonas();
            Assert.Single(personas);
            var allPersonas = controller.GetPersonas(ArchiveMode.All);
            Assert.Equal(2, allPersonas.Count());
        }
    }
}
