using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Comments",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The comments module enables content items to have comments.",
    Dependencies = ["OrchardCore.Contents", "OrchardCore.ContentFields", "OrchardCore.Media"],
    Category = "Content Management"
)]
