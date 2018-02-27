using ApplicationModels.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ApplicationModels.Migrations {
    public partial class MetaTagSeed : Migration {
        protected override void Up(MigrationBuilder migrationBuilder) {

            using (var context = new ApplicationDbContext()) {
                foreach (var s in new string[] { "Medium", "Length", "Tone", "Topic", "Target Audience" }) {
                    context.Add(new ApplicationMetaTagType() { Type = s });
                }
                context.SaveChanges();

                var lengthType = context.ApplicationMetaTagsTypes.Single(x => x.Type == "Length");
                foreach (var s in new string[] { "0-2min", "3-5min", "6-15min", "16-44min", "22-24min", "44-48min", "45+min" }) {
                    context.Add(new ApplicationMetaTag() {
                        Type = lengthType,
                        Tag = s
                    });
                }

                var toneType = context.ApplicationMetaTagsTypes.Single(x => x.Type == "Tone");
                foreach (var(s, c) in new (string, string)[] { ("Bright/Upbeat", "#28ABE2"),
                                                               ("Warm/Hopeful", "#1B83C9"),
                                                               ("Light/Amusing", "#214291"),
                                                               ("Neutral/Dry", "#6B0D3A"),
                                                               ("Dramatic/Emotional", "#841C26"),
                                                               ("Critical/Sarcastic", "#D11111"),
                                                               ("Dark/Pessimistic", "#F28E3D"), }) {
                    context.Add(new ApplicationMetaTag() {
                        Type = toneType,
                        Tag = s,
                        Color = c
                    });
                }

                var topicType = context.ApplicationMetaTagsTypes.Single(x => x.Type == "Topic");
                foreach (var s in new string[] { "Culture", "Economics", "Education", "Individual Rights", "Political Philosophy", "Practical/Life-Skills" }) {
                    context.Add(new ApplicationMetaTag() {
                        Type = topicType,
                        Tag = s
                    });
                }

                var mediumType = context.ApplicationMetaTagsTypes.Single(x => x.Type == "Medium");
                foreach (var s in new string[] { "Audio Podcast",
                                                 "Documentary: Animated",
                                                 "Documentary: Live Action",
                                                 "Educational: Animated",
                                                 "Educational: Live Action",
                                                 "Event: Clip",
                                                 "Event: Full",
                                                 "Meme: Image",
                                                 "Meme: Video",
                                                 "Narrative: Animated",
                                                 "Narrative: Live Action",
                                                 "News Brief",
                                                 "Photo-Essay",
                                                 "Photo-Slideshow",
                                                 "Promo: Audio",
                                                 "Promo: Trailer",
                                                 "Promo: Video",
                                                 "Video-Essay", }) {
                    context.Add(new ApplicationMetaTag() {
                        Type = mediumType,
                        Tag = s
                    });
                }

                var targetAudienceType = context.ApplicationMetaTagsTypes.Single(x => x.Type == "Target Audience");
                foreach (var s in new string[] { "Secular left", "Secular moderates", "Conservatives", "APolitical" }) {
                    context.Add(new ApplicationMetaTag() {
                        Type = targetAudienceType,
                        Tag = s
                    });
                }

                context.SaveChanges();
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {}
    }
}
